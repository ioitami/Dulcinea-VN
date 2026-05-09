using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using TMPro;
using UnityEngine;

public class ServerManager : MonoBehaviour
{
    [Header("Server Settings")]
    public int port = 7777;

    private TcpListener server;
    private TcpClient client;
    private bool isRunning = false;
    private Thread serverThread;
    private Thread clientThread;

    public TextMeshProUGUI servertestText;

    private void Start()
    {
        if (IsServerAlreadyRunning())
        {
            SetAsClient();
        }
        else
        {
            SetAsServer();
        }
    }

    private void OnApplicationQuit()
    {
        isRunning = false;

        // Only stop the server if this instance is the main server
        if (GameSingleton.instance.dialogueManager.isMainServer)
        {
            if (server != null)
            {
                server.Stop();
                server = null;
            }

            if (serverThread != null && serverThread.IsAlive)
            {
                serverThread.Abort();
                serverThread = null;
            }

            Debug.Log("[ServerManager] Main server stopped.");
        }

        // Always clean up client connection
        if (client != null)
        {
            client.Close();
            client = null;
        }

        if (clientThread != null && clientThread.IsAlive)
        {
            clientThread.Abort();
            clientThread = null;
        }

        Debug.Log("[ServerManager] Client disconnected.");
    }

    // ===========================
    // Server Detection
    // ===========================

    private bool IsServerAlreadyRunning()
    {
        try
        {
            TcpClient testClient = new TcpClient();
            testClient.Connect(IPAddress.Loopback, port);
            testClient.Close();

            Debug.Log("[ServerManager] Existing server detected.");
            return true;
        }
        catch
        {
            Debug.Log("[ServerManager] No existing server detected.");
            return false;
        }
    }

    // ===========================
    // Server
    // ===========================

    private void SetAsServer()
    {
        Debug.Log("[ServerManager] Setting as main server.");
        GameSingleton.instance.dialogueManager.isMainServer = true;

        try
        {
            server = new TcpListener(IPAddress.Loopback, port);
            isRunning = true;
            server.Start();

            serverThread = new Thread(ServerLoop);
            serverThread.IsBackground = true;
            serverThread.Start();

            Debug.Log($"[ServerManager] Server started on port {port}.");

            servertestText.text = "Main Window";
        }
        catch (Exception e)
        {
            Debug.LogError($"[ServerManager] Failed to start server: {e.Message}");
        }
    }

    private void ServerLoop()
    {
        while (isRunning)
        {
            try
            {
                if (server.Pending())
                {
                    TcpClient incomingClient = server.AcceptTcpClient();
                    Debug.Log("[ServerManager] Client connected.");
                    incomingClient.Close();
                }

                Thread.Sleep(100);
            }
            catch (Exception e)
            {
                if (isRunning)
                    Debug.LogError($"[ServerManager] Server error: {e.Message}");
            }
        }
    }

    // ===========================
    // Client
    // ===========================

    private void SetAsClient()
    {
        Debug.Log("[ServerManager] Setting as client.");
        GameSingleton.instance.dialogueManager.isMainServer = false;
        servertestText.text = "Window 2";

        clientThread = new Thread(ConnectToServer);
        clientThread.IsBackground = true;
        clientThread.Start();
    }

    private void ConnectToServer()
    {
        try
        {
            client = new TcpClient();
            client.Connect(IPAddress.Loopback, port);

            Debug.Log($"[ServerManager] Connected to server on port {port}.");
        }
        catch (Exception e)
        {
            Debug.LogError($"[ServerManager] Failed to connect to server: {e.Message}");
        }
    }
}