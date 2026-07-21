// NdiToHoloLensSender.cs
// -----------------------------------------------------------------------
// LADO PC (Build Standalone Windows, NO UWP)
// Requiere: paquete KlakNDI (jp.keijiro.klak.ndi) ya instalado.
//
// SETUP EN LA ESCENA:
// 1. Crea un GameObject vacío llamado "NdiBridge".
// 2. Agrega el componente "Ndi Receiver" de KlakNDI a ese mismo GameObject
//    (o a otro) y configúralo con el nombre de tu fuente NDI.
// 3. Agrega este script (NdiToHoloLensSender) al GameObject "NdiBridge".
// 4. Arrastra el componente NdiReceiver al campo "ndiReceiver" en el Inspector.
// 5. Build > Standalone Windows (x64). Este build corre en la PC, no en HoloLens.
// -----------------------------------------------------------------------

using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;
using Klak.Ndi; // NdiReceiver

public class NdiToHoloLensSender : MonoBehaviour
{
    [Header("Referencia al receptor NDI de KlakNDI")]
    public NdiReceiver ndiReceiver;

    [Header("Configuración de red")]
    public int listenPort = 9999;
    [Range(10, 90)] public int jpegQuality = 70; // más bajo = menos latencia/bandwidth, más artefactos
    [Range(1, 60)] public int maxSendFps = 30;   // limita el envío para no saturar la red

    TcpListener _listener;
    readonly ConcurrentBag<TcpClient> _clients = new ConcurrentBag<TcpClient>();
    Thread _acceptThread;
    Texture2D _readTex;
    float _lastSendTime;
    bool _running;

    void Start()
    {
        _running = true;
        _listener = new TcpListener(IPAddress.Any, listenPort);
        _listener.Start();

        _acceptThread = new Thread(AcceptLoop) { IsBackground = true };
        _acceptThread.Start();

        Debug.Log($"[NdiToHoloLensSender] Escuchando en puerto {listenPort}. Esperando HoloLens...");
    }

    void AcceptLoop()
    {
        while (_running)
        {
            try
            {
                var client = _listener.AcceptTcpClient();
                client.NoDelay = true; // desactiva Nagle: reduce latencia
                _clients.Add(client);
                Debug.Log("[NdiToHoloLensSender] Cliente HoloLens conectado.");
            }
            catch (SocketException)
            {
                break; // listener detenido
            }
        }
    }

    void Update()
    {
        if (ndiReceiver == null) return;
        if (Time.time - _lastSendTime < 1f / maxSendFps) return;

        var srcTex = ndiReceiver.texture; // RenderTexture con el frame NDI actual
        if (srcTex == null) return;

        _lastSendTime = Time.time;
        SendFrame(srcTex);
    }

    void SendFrame(Texture srcTex)
    {
        // Lee la RenderTexture a una Texture2D en CPU (bloqueante, simple).
        // Para mayor performance, migrar a AsyncGPUReadback más adelante.
        var rt = srcTex as RenderTexture;
        if (rt == null) return;

        if (_readTex == null || _readTex.width != rt.width || _readTex.height != rt.height)
            _readTex = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);

        var prevActive = RenderTexture.active;
        RenderTexture.active = rt;
        _readTex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        _readTex.Apply(false);
        RenderTexture.active = prevActive;

        byte[] jpg = _readTex.EncodeToJPG(jpegQuality);
        byte[] lenPrefix = BitConverter.GetBytes(jpg.Length);

        // Envía a todos los clientes conectados; descarta los caídos.
        var stillConnected = new ConcurrentBag<TcpClient>();
        foreach (var c in _clients)
        {
            try
            {
                if (!c.Connected) continue;
                var stream = c.GetStream();
                stream.Write(lenPrefix, 0, 4);
                stream.Write(jpg, 0, jpg.Length);
                stillConnected.Add(c);
            }
            catch
            {
                try { c.Close(); } catch { }
            }
        }
        // reemplaza la bolsa de clientes activos
        while (!_clients.IsEmpty) _clients.TryTake(out _);
        foreach (var c in stillConnected) _clients.Add(c);
    }

    void OnDestroy()
    {
        _running = false;
        try { _listener?.Stop(); } catch { }
        foreach (var c in _clients) { try { c.Close(); } catch { } }
    }
}