// HoloLensStreamReceiver.cs
// -----------------------------------------------------------------------
// LADO HOLOLENS (Build UWP / ARM64)
// No requiere plugins nativos: usa System.Net.Sockets, 100% compatible UWP.
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
// -----------------------------------------------------------------------

using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

public class HoloLensStreamReceiver : MonoBehaviour
{
    [Header("Conexión")]
    public string serverIp = "192.168.1.100"; // IP de la PC que corre el Sender
    public int serverPort = 9999;
    public float reconnectDelaySeconds = 2f;

    [Header("Salida de video")]
    public Renderer targetRenderer; // el Quad/pantalla donde se muestra el stream

    Thread _networkThread;
    volatile bool _running;
    readonly ConcurrentQueue<byte[]> _frameQueue = new ConcurrentQueue<byte[]>();
    Texture2D _tex;

    void Start()
    {
        _tex = new Texture2D(2, 2, TextureFormat.RGB24, false);
        if (targetRenderer != null)
            targetRenderer.material.mainTexture = _tex;

        _running = true;
        _networkThread = new Thread(NetworkLoop) { IsBackground = true };
        _networkThread.Start();
    }

    void NetworkLoop()
    {
        while (_running)
        {
            try
            {
                using (var client = new TcpClient())
                {
                    client.NoDelay = true;
                    client.Connect(serverIp, serverPort);
                    Debug.Log("[HoloLensStreamReceiver] Conectado al servidor NDI Bridge.");

                    using (var stream = client.GetStream())
                    {
                        var lenBuf = new byte[4];
                        while (_running && client.Connected)
                        {
                            ReadExact(stream, lenBuf, 4);
                            int len = BitConverter.ToInt32(lenBuf, 0);
                            if (len <= 0 || len > 50_000_000) throw new Exception("Frame length inválido");

                            var frameBuf = new byte[len];
                            ReadExact(stream, frameBuf, len);

                            // Descarta frames viejos si el consumidor va más lento (baja latencia > calidad)
                            while (_frameQueue.Count > 1) _frameQueue.TryDequeue(out _);
                            _frameQueue.Enqueue(frameBuf);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[HoloLensStreamReceiver] Conexión perdida/fallida: {e.Message}. Reintentando...");
            }

            Thread.Sleep((int)(reconnectDelaySeconds * 1000));
        }
    }

    static void ReadExact(NetworkStream stream, byte[] buffer, int count)
    {
        int offset = 0;
        while (offset < count)
        {
            int read = stream.Read(buffer, offset, count - offset);
            if (read <= 0) throw new Exception("Conexión cerrada por el servidor");
            offset += read;
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
    }
}