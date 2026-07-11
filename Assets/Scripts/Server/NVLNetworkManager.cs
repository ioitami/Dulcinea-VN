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

        // Disable both cameras on startup
        // They will be enabled when host or client starts
        if (serverCamera != null) serverCamera.gameObject.SetActive(false);
        if (clientCamera != null) clientCamera.gameObject.SetActive(false);
    }

    public override void OnStartHost()
    {
        base.OnStartHost();
        Debug.Log("[NVLNetworkManager] Started as host.");

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

        // Pure client — not the host
        if (!NetworkServer.active)
        {

            if (serverCamera != null) serverCamera.gameObject.SetActive(false);
            if (clientCamera != null) clientCamera.gameObject.SetActive(true);

            GameSingleton.instance.dialogueManager.isMainServer = false;

            Debug.Log("[NVLNetworkManager] Client camera activated.");
        }
    }

    public override void OnStopClient()
    {
        base.OnStopClient();
        Debug.Log("[NVLNetworkManager] Client disconnected.");

        if (!NetworkServer.active)
        {
            if (clientCamera != null) clientCamera.gameObject.SetActive(false);
        }
    }

    public override void OnStopHost()
    {
        base.OnStopHost();
        Debug.Log("[NVLNetworkManager] Host stopped.");

        if (serverCamera != null) serverCamera.gameObject.SetActive(false);
    }
}