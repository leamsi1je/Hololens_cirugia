// HoloLensStreamReceiver.cs
// -----------------------------------------------------------------------
// LADO HOLOLENS (Build UWP / ARM64)
// No requiere plugins nativos: usa System.Net.Sockets, 100% compatible UWP.
// VERSION UDP: se registra con el servidor mandando paquetes HELLO
// periódicos, y recibe los frames partidos en chunks, reensamblándolos.
// Si falta un chunk de un frame, ese frame se descarta (no se espera
// reenvío) — prioriza baja latencia sobre completitud, ideal para WiFi.
//
// SETUP EN LA ESCENA:
// 1. Crea un Quad (GameObject > 3D Object > Quad) a la distancia deseada
//    de la cámara (ej. 2 metros al frente).
// 2. Crea un material con shader "Unlit/Texture" y asígnalo al Quad.
// 3. Agrega este script a un GameObject (puede ser el mismo Quad).
// 4. Arrastra el Renderer del Quad al campo "targetRenderer".
// 5. Configura "serverIp" con la IP de la PC (Bridge) en tu red local.
// 6. Build > Universal Windows Platform > ARM64. Habilita capability
//    "Internet Client" en Publishing Settings (Player Settings).
//
// PROTOCOLO: ver comentario en NdiToHoloLensSender.cs (mismo formato).
// -----------------------------------------------------------------------

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

public class HoloLensStreamReceiver : MonoBehaviour
{
    [Header("Conexión")]
    public string serverIp = "192.168.1.100"; // IP de la PC que corre el Sender
    public int serverPort = 9999;
    public float helloIntervalSeconds = 1f;   // frecuencia de registro/keepalive hacia el servidor

    [Header("Salida de video")]
    public Renderer targetRenderer; // el Quad/pantalla donde se muestra el stream

    UdpClient _udp;
    IPEndPoint _serverEndPoint;
    Thread _receiveThread;
    Thread _helloThread;
    volatile bool _running;

    readonly ConcurrentQueue<byte[]> _frameQueue = new ConcurrentQueue<byte[]>();
    Texture2D _tex;

    // Reensamblado del frame en curso
    readonly object _reassemblyLock = new object();
    uint _currentFrameId;
    bool _currentFrameActive;
    int _currentTotalChunks;
    int _currentReceivedChunks;
    byte[][] _currentChunks;

    void Start()
    {
        _tex = new Texture2D(2, 2, TextureFormat.RGB24, false);
        if (targetRenderer != null)
            targetRenderer.material.mainTexture = _tex;

        _serverEndPoint = new IPEndPoint(IPAddress.Parse(serverIp), serverPort);
        _udp = new UdpClient(0); // puerto local aleatorio
        _running = true;

        _receiveThread = new Thread(ReceiveLoop) { IsBackground = true };
        _receiveThread.Start();

        _helloThread = new Thread(HelloLoop) { IsBackground = true };
        _helloThread.Start();
    }

    // Manda un HELLO periódico para registrarse/mantenerse vivo ante el servidor.
    void HelloLoop()
    {
        byte[] hello = new byte[] { 0x02 };
        while (_running)
        {
            try { _udp.Send(hello, hello.Length, _serverEndPoint); }
            catch (Exception e) { Debug.LogWarning($"[HoloLensStreamReceiver] No se pudo enviar HELLO: {e.Message}"); }

            Thread.Sleep((int)(helloIntervalSeconds * 1000));
        }
    }

    void ReceiveLoop()
    {
        var anyEp = new IPEndPoint(IPAddress.Any, 0);
        int chunkCount = 0;
        while (_running)
        {
            try
            {
                byte[] packet = _udp.Receive(ref anyEp);
                chunkCount++;
                if (chunkCount <= 5 || chunkCount % 50 == 0)
                    Debug.Log($"[HoloLensStreamReceiver] Paquete UDP recibido #{chunkCount}, {packet.Length} bytes, primer byte={packet[0]}");

                if (packet.Length < 9 || packet[0] != 0x01) continue; // no es FRAME_CHUNK

                uint frameId = BitConverter.ToUInt32(packet, 1);
                ushort chunkIndex = BitConverter.ToUInt16(packet, 5);
                ushort totalChunks = BitConverter.ToUInt16(packet, 7);
                int payloadLen = packet.Length - 9;

                lock (_reassemblyLock)
                {
                    // Frame nuevo: descarta cualquier reensamblado incompleto anterior.
                    if (!_currentFrameActive || frameId != _currentFrameId)
                    {
                        _currentFrameId = frameId;
                        _currentFrameActive = true;
                        _currentTotalChunks = totalChunks;
                        _currentReceivedChunks = 0;
                        _currentChunks = new byte[totalChunks][];
                    }

                    if (chunkIndex < _currentChunks.Length && _currentChunks[chunkIndex] == null)
                    {
                        byte[] payload = new byte[payloadLen];
                        Buffer.BlockCopy(packet, 9, payload, 0, payloadLen);
                        _currentChunks[chunkIndex] = payload;
                        _currentReceivedChunks++;

                        if (_currentReceivedChunks == _currentTotalChunks)
                        {
                            // Frame completo: ensambla y encola.
                            int totalLen = 0;
                            foreach (var c in _currentChunks) totalLen += c.Length;

                            byte[] full = new byte[totalLen];
                            int pos = 0;
                            foreach (var c in _currentChunks)
                            {
                                Buffer.BlockCopy(c, 0, full, pos, c.Length);
                                pos += c.Length;
                            }

                            while (_frameQueue.Count > 1) _frameQueue.TryDequeue(out _); // solo el más reciente
                            _frameQueue.Enqueue(full);
                            Debug.Log($"[HoloLensStreamReceiver] Frame completo ensamblado: {full.Length} bytes, frameId={frameId}");

                            _currentFrameActive = false;
                        }
                    }
                }
            }
            catch (SocketException)
            {
                break; // socket cerrado
            }
            catch (ObjectDisposedException)
            {
                break;
            }
        }
    }

    void Update()
    {
        // Decodifica y aplica el frame más reciente en el hilo principal (requerido por Unity)
        if (_frameQueue.TryDequeue(out var jpg))
        {
            if (_tex.LoadImage(jpg)) // LoadImage redimensiona automáticamente
            {
                if (targetRenderer != null)
                    targetRenderer.material.mainTexture = _tex;
            }
        }
    }

    void OnDestroy()
    {
        _running = false;
        try { _udp?.Close(); } catch { }
    }
}