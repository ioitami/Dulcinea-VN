using System.Collections;
using Mirror;
using UnityEngine;

public class NVLNetworkManager : NetworkManager
{
    [Header("NVL Cameras")]
    public Camera serverCamera;
    public Camera clientCamera;

    [Header("NVL Window Prompt")]
    public NVLWindowPromptUI windowPromptUI;

    public static NVLNetworkManager instance { get; private set; }

    private bool isBlockingForWindowRequirement = false;

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

        EvaluateWindowRequirement();
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

    // ===========================
    // Player spawn -> re-evaluate window requirement
    // Fires for every connection's player, including the host's own, so
    // this is the single place a "window count changed" check needs to run.
    // ===========================

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        base.OnServerAddPlayer(conn);
        EvaluateWindowRequirement();
    }

    // ===========================
    // Disconnects — the auto-close-together rule and the special
    // window-close narrative checkpoint both hinge on these two hooks.
    // ===========================

    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        bool wasHostsOwnConnection = NetworkServer.active && conn == NetworkServer.localConnection;

        base.OnServerDisconnect(conn);

        if (wasHostsOwnConnection) return;

        Debug.Log("[NVLNetworkManager] Second window disconnected.");

        NVLNetworkPlayer hostPlayer = NVLNetworkPlayer.hostInstance;

        if (hostPlayer != null && hostPlayer.AwaitingWindowCloseChoice)
        {
            Debug.Log("[NVLNetworkManager] Second window closed during a window-choice checkpoint — resolving that outcome.");

            hostPlayer.TryGetClientCloseOutcome(out string groupID, out string blockID);
            hostPlayer.SetAwaitingWindowCloseChoice(false);
            hostPlayer.SetRequiresServer(false);

            if (DialogueLookup.TryFindGroupAndBlock(groupID, blockID, out DialogueGroup group, out DialogueBlock block))
                GameSingleton.instance.dialogueManager.PlaySpecificBlockInGroup(group, block);
            else
                Debug.LogWarning($"[NVLNetworkManager] Could not resolve client-close outcome '{groupID}'/'{blockID}'.");

            EvaluateWindowRequirement();
            return;
        }

        if (hostPlayer != null && hostPlayer.RequiresServer)
        {
            Debug.Log("[NVLNetworkManager] Required second window closed unexpectedly — closing host window too.");
            QuitApplication();
            return;
        }

        // The second window closing was the expected/required action (the
        // "please close the second window" prompt) — resume solo play.
        EvaluateWindowRequirement();
    }

    public override void OnClientDisconnect()
    {
        bool weAreStillHost = NetworkServer.active;

        base.OnClientDisconnect();

        // If we're the host, this fires as part of our own local shutdown
        // sequence, not because the host disappeared on us.
        if (weAreStillHost) return;

        Debug.Log("[NVLNetworkManager] Lost connection to host window.");

        if (NVLNetworkPlayer.CachedAwaitingWindowCloseChoice)
        {
            StartCoroutine(PromoteToHostAndResolve(
                NVLNetworkPlayer.CachedGroupIfHostCloses,
                NVLNetworkPlayer.CachedBlockIfHostCloses));
        }
        else
        {
            // Host window closed unexpectedly (not the special checkpoint)
            // — this window can't continue on its own, so close it too.
            Debug.Log("[NVLNetworkManager] Host window closed unexpectedly — closing this window too.");
            QuitApplication();
        }
    }

    // ===========================
    // Window-close narrative checkpoint: promote this (surviving) client
    // to host and resume the story at the appropriate outcome.
    // ===========================

    private IEnumerator PromoteToHostAndResolve(string groupID, string blockID)
    {
        Debug.Log("[NVLNetworkManager] Promoting this window to host after the original host closed.");

        if (NetworkClient.active)
            StopClient();

        // Give the OS a moment to release the port the old host was using.
        yield return new WaitForSeconds(0.5f);

        StartHost();

        float timeout = 5f;
        while ((!NetworkServer.active || NVLNetworkPlayer.hostInstance == null) && timeout > 0f)
        {
            timeout -= Time.deltaTime;
            yield return null;
        }

        if (!NetworkServer.active || NVLNetworkPlayer.hostInstance == null)
        {
            Debug.LogError("[NVLNetworkManager] Failed to promote this window to host.");
            yield break;
        }

        NVLNetworkPlayer.hostInstance.SetAwaitingWindowCloseChoice(false);
        NVLNetworkPlayer.hostInstance.SetRequiresServer(false);

        if (DialogueLookup.TryFindGroupAndBlock(groupID, blockID, out DialogueGroup group, out DialogueBlock block))
            GameSingleton.instance.dialogueManager.PlaySpecificBlockInGroup(group, block);
        else
            Debug.LogWarning($"[NVLNetworkManager] Could not resolve host-close outcome '{groupID}'/'{blockID}'.");

        EvaluateWindowRequirement();
    }

    // ===========================
    // Window requirement evaluation — shows/hides the adjustable prompt on
    // this window and gates dialogue clicking until the requirement (2
    // windows, or exactly 1 window) is met.
    // ===========================

    public void EvaluateWindowRequirement()
    {
        if (NetworkServer.active)
        {
            bool requiresTwo = NVLNetworkPlayer.hostInstance != null && NVLNetworkPlayer.hostInstance.RequiresServer;
            bool secondWindowConnected = NumConnectedRealClients() > 0;

            if (requiresTwo && !secondWindowConnected)
            {
                windowPromptUI?.ShowWaitingForSecondWindow();
                SetDialogueBlockedForWindowRequirement(true);
            }
            else
            {
                windowPromptUI?.Hide();
                SetDialogueBlockedForWindowRequirement(false);
            }
        }
        else if (NetworkClient.active)
        {
            bool requiresTwo = NVLNetworkPlayer.localPlayer != null && NVLNetworkPlayer.localPlayer.RequiresServer;

            if (!requiresTwo)
            {
                windowPromptUI?.ShowPleaseCloseSecondWindow();
                SetDialogueBlockedForWindowRequirement(true);
            }
            else
            {
                windowPromptUI?.Hide();
                SetDialogueBlockedForWindowRequirement(false);
            }
        }
    }

    private void SetDialogueBlockedForWindowRequirement(bool blocked)
    {
        if (blocked == isBlockingForWindowRequirement) return;
        isBlockingForWindowRequirement = blocked;

        DialogueManager dialogueManager = GameSingleton.instance != null ? GameSingleton.instance.dialogueManager : null;
        if (dialogueManager == null) return;

        if (blocked)
            dialogueManager.SetGlobalAllowDialogueClick(false);
        else
            dialogueManager.RememberGlobalAllowDialogueClickBool();
    }

    private int NumConnectedRealClients()
    {
        int count = 0;

        foreach (var kv in NetworkServer.connections)
        {
            NetworkConnectionToClient conn = kv.Value;
            if (conn == null) continue;
            if (conn == NetworkServer.localConnection) continue;
            if (conn.isReady) count++;
        }

        return count;
    }

    private void QuitApplication()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
