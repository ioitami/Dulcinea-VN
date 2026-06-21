using Mirror;
using UnityEngine;

public class NVLNetworkManager : NetworkManager
{
    [Header("NVL Cameras")]
    public Camera serverCamera;
    public Camera clientCamera;

    public static NVLNetworkManager instance { get; private set; }

    public override void Awake()
    {
        base.Awake();

        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    public override void OnStartHost()
    {
        base.OnStartHost();

        Debug.Log("[NVLNetworkManager] Started as host.");

        // Host sees server camera
        if (serverCamera != null) serverCamera.gameObject.SetActive(true);
        if (clientCamera != null) clientCamera.gameObject.SetActive(false);

        GameSingleton.instance.dialogueManager.isMainServer = true;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        Debug.Log("[NVLNetworkManager] Started as client.");
    }

    public override void OnClientConnect()
    {
        base.OnClientConnect();

        Debug.Log("[NVLNetworkManager] Client connected.");

        // If this is a pure client (not the host), show client camera
        if (!NetworkServer.active)
        {
            if (serverCamera != null) serverCamera.gameObject.SetActive(false);
            if (clientCamera != null) clientCamera.gameObject.SetActive(true);

            GameSingleton.instance.dialogueManager.isMainServer = false;
        }
    }

    public override void OnStopClient()
    {
        base.OnStopClient();
        Debug.Log("[NVLNetworkManager] Client disconnected.");
    }

    public override void OnStopHost()
    {
        base.OnStopHost();
        Debug.Log("[NVLNetworkManager] Host stopped.");
    }
}