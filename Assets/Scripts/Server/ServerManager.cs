using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
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
    private List<TcpClient> connectedClients = new List<TcpClient>();

    private void Start()
    {
        if (IsServerAlreadyRunning())
            SetAsClient();
        else
            SetAsServer();
    }

    private void OnApplicationQuit()
    {
        isRunning = false;

        if (GameSingleton.instance.dialogueManager.isMainServer)
        {
            DisconnectAllClients();

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

                    lock (connectedClients)
                    {
                        connectedClients.Add(incomingClient);
                    }

                    // Send requiresServer status to newly connected client
                    bool requiresServer = GameSingleton.instance.dialogueManager.requiresServer;
                    SendCommandToClient(incomingClient, $"<SetRequiresServer({requiresServer})>");
                }

                lock (connectedClients)
                {
                    connectedClients.RemoveAll(c =>
                    {
                        if (c == null) return true;

                        try
                        {
                            if (c.Client.Poll(0, SelectMode.SelectRead))
                            {
                                byte[] buffer = new byte[1];
                                if (c.Client.Receive(buffer, SocketFlags.Peek) == 0)
                                {
                                    c.Close();
                                    return true;
                                }
                            }
                            return false;
                        }
                        catch
                        {
                            return true;
                        }
                    });
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

    private void SendCommandToClient(TcpClient targetClient, string command)
    {
        try
        {
            NetworkStream stream = targetClient.GetStream();
            byte[] data = System.Text.Encoding.UTF8.GetBytes(command);
            stream.Write(data, 0, data.Length);
            stream.Flush();
        }
        catch (Exception e)
        {
            Debug.LogError($"[ServerManager] Failed to send to specific client: {e.Message}");
        }
    }

    public void MirrorCommandToClient(string command)
    {
        if (!GameSingleton.instance.dialogueManager.isMainServer) return;
        if (!GameSingleton.instance.dialogueManager.requiresServer) return;

        SendCommandToNVL(command);
    }

    // ===========================
    // Client
    // ===========================

    private void SetAsClient()
    {
        Debug.Log("[ServerManager] Setting as client.");
        GameSingleton.instance.dialogueManager.isMainServer = false;
        isRunning = true;

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

            // Request requiresServer status immediately on connect
            NetworkStream stream = client.GetStream();
            byte[] request = System.Text.Encoding.UTF8.GetBytes("<GetRequiresServer>");
            stream.Write(request, 0, request.Length);
            stream.Flush();

            ReceiveLoop();
        }
        catch (Exception e)
        {
            UnityMainThreadDispatcher.instance.Enqueue(() =>
            {
                Debug.LogError($"[ServerManager] Failed to connect to server: {e.Message}");
            });
        }
    }

    private void ReceiveLoop()
    {
        try
        {
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[4096];

            while (isRunning && client.Connected)
            {
                try
                {
                    if (stream.DataAvailable)
                    {
                        int bytesRead = stream.Read(buffer, 0, buffer.Length);
                        string command = System.Text.Encoding.UTF8.GetString(buffer, 0, bytesRead);

                        Debug.Log($"[ServerManager] Command received: {command}");

                        UnityMainThreadDispatcher.instance.Enqueue(() =>
                        {
                            if (GameSingleton.instance.serverCommands == null)
                            {
                                Debug.LogError("[ServerManager] serverCommands is null on client.");
                                return;
                            }

                            GameSingleton.instance.serverCommands.ParseAndExecute(command);
                        });
                    }
                }
                catch (Exception innerException)
                {
                    UnityMainThreadDispatcher.instance.Enqueue(() =>
                    {
                        Debug.LogError($"[ServerManager] Receive error: {innerException.Message}");
                    });
                }

                Thread.Sleep(50);
            }
        }
        catch (Exception e)
        {
            UnityMainThreadDispatcher.instance.Enqueue(() =>
            {
                Debug.LogError($"[ServerManager] Receive loop crashed: {e.Message}");
            });
        }
    }

    // ===========================
    // Commands
    // ===========================

    public void SendCommandToNVL(string command)
    {
        if (!GameSingleton.instance.dialogueManager.isMainServer)
        {
            Debug.LogWarning("[ServerManager] SendCommandToNVL called but this is not the main server.");
            return;
        }

        if (connectedClients.Count == 0)
        {
            Debug.LogWarning("[ServerManager] No clients connected.");
            return;
        }

        Debug.Log($"[ServerManager] Sending command: {command}");

        lock (connectedClients)
        {
            List<TcpClient> disconnectedClients = new List<TcpClient>();

            foreach (TcpClient connectedClient in connectedClients)
            {
                if (connectedClient == null || !connectedClient.Connected)
                {
                    disconnectedClients.Add(connectedClient);
                    continue;
                }

                try
                {
                    NetworkStream stream = connectedClient.GetStream();

                    if (!stream.CanWrite)
                    {
                        disconnectedClients.Add(connectedClient);
                        continue;
                    }

                    byte[] data = System.Text.Encoding.UTF8.GetBytes(command);
                    stream.Write(data, 0, data.Length);
                    stream.Flush();
                }
                catch (Exception e)
                {
                    Debug.LogError($"[ServerManager] Failed to send command: {e.Message}");
                    disconnectedClients.Add(connectedClient);
                }
            }

            foreach (TcpClient disconnected in disconnectedClients)
            {
                connectedClients.Remove(disconnected);
                Debug.LogWarning("[ServerManager] Removed disconnected client.");
            }
        }
    }

    public void DisconnectAllClients()
    {
        if (!GameSingleton.instance.dialogueManager.isMainServer)
        {
            Debug.LogWarning("[ServerManager] DisconnectAllClients called but this is not the main server.");
            return;
        }

        lock (connectedClients)
        {
            foreach (TcpClient connectedClient in connectedClients)
            {
                if (connectedClient != null && connectedClient.Connected)
                    connectedClient.Close();
            }

            connectedClients.Clear();
        }

        Debug.Log("[ServerManager] All clients disconnected.");
    }
}