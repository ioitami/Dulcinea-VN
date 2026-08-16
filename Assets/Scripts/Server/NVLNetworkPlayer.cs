using Mirror;
using UnityEngine;

public class NVLNetworkPlayer : NetworkBehaviour
{
    public static NVLNetworkPlayer localPlayer { get; private set; }

    // The host's own player object — the single canonical instance whose
    // session-state SyncVars every window (host and client) observes.
    public static NVLNetworkPlayer hostInstance { get; private set; }

    // Static caches of the last-known session state, kept in sync via the
    // SyncVar hooks below. NVLNetworkManager.OnClientDisconnect reads these
    // instead of instance fields, since by the time that callback runs the
    // underlying NetworkIdentity may already be torn down.
    public static bool CachedRequiresServer { get; private set; }
    public static bool CachedAwaitingWindowCloseChoice { get; private set; }
    public static string CachedGroupIfHostCloses { get; private set; } = "";
    public static string CachedBlockIfHostCloses { get; private set; } = "";
    public static string CachedGroupIfClientCloses { get; private set; } = "";
    public static string CachedBlockIfClientCloses { get; private set; } = "";

    public override void OnStartLocalPlayer()
    {
        localPlayer = this;

        if (isServer)
            hostInstance = this;

        Debug.Log("[NVLNetworkPlayer] Local player started.");
    }

    public override void OnStopClient()
    {
        base.OnStopClient();

        if (localPlayer == this)
            localPlayer = null;
    }

    public override void OnStopServer()
    {
        base.OnStopServer();

        if (hostInstance == this)
            hostInstance = null;
    }

    // ===========================
    // Session state
    // Only ever written through hostInstance so every window agrees on one
    // canonical value. Written from server-side DialogueManager logic.
    // String fields are declared before the bools that hook off them so a
    // single sync batch always applies the outcome IDs before the flags
    // that depend on them fire their hooks.
    // ===========================

    [SyncVar] private string outcomeGroupIfHostCloses = "";
    [SyncVar] private string outcomeBlockIfHostCloses = "";
    [SyncVar] private string outcomeGroupIfClientCloses = "";
    [SyncVar] private string outcomeBlockIfClientCloses = "";

    [SyncVar(hook = nameof(OnRequiresServerChanged))]
    private bool requiresServer = false;

    [SyncVar(hook = nameof(OnAwaitingWindowCloseChoiceChanged))]
    private bool awaitingWindowCloseChoice = false;

    // "WIDTHxHEIGHT" — a single string avoids any cross-field sync-order
    // hazard. Only ever set by window 1 (the host); window 2 just follows.
    [SyncVar(hook = nameof(OnResolutionChanged))]
    private string syncedResolution = "";

    public bool RequiresServer => requiresServer;
    public bool AwaitingWindowCloseChoice => awaitingWindowCloseChoice;

    [Server]
    public void SetRequiresServer(bool value)
    {
        requiresServer = value;
    }

    [Server]
    public void SetResolution(int width, int height)
    {
        syncedResolution = $"{width}x{height}";
    }

    [Server]
    public void SetAwaitingWindowCloseChoice(bool value, string groupIfHostCloses = "", string blockIfHostCloses = "", string groupIfClientCloses = "", string blockIfClientCloses = "")
    {
        outcomeGroupIfHostCloses = groupIfHostCloses;
        outcomeBlockIfHostCloses = blockIfHostCloses;
        outcomeGroupIfClientCloses = groupIfClientCloses;
        outcomeBlockIfClientCloses = blockIfClientCloses;
        awaitingWindowCloseChoice = value;
    }

    public bool TryGetHostCloseOutcome(out string groupID, out string blockID)
    {
        groupID = outcomeGroupIfHostCloses;
        blockID = outcomeBlockIfHostCloses;
        return !string.IsNullOrEmpty(groupID);
    }

    public bool TryGetClientCloseOutcome(out string groupID, out string blockID)
    {
        groupID = outcomeGroupIfClientCloses;
        blockID = outcomeBlockIfClientCloses;
        return !string.IsNullOrEmpty(groupID);
    }

    private void OnRequiresServerChanged(bool oldValue, bool newValue)
    {
        CachedRequiresServer = newValue;

        if (GameSingleton.instance != null && GameSingleton.instance.dialogueManager != null)
            GameSingleton.instance.dialogueManager.requiresServer = newValue;

        if (NVLNetworkManager.instance != null)
            NVLNetworkManager.instance.EvaluateWindowRequirement();
    }

    private void OnAwaitingWindowCloseChoiceChanged(bool oldValue, bool newValue)
    {
        CachedAwaitingWindowCloseChoice = newValue;
        CachedGroupIfHostCloses = outcomeGroupIfHostCloses;
        CachedBlockIfHostCloses = outcomeBlockIfHostCloses;
        CachedGroupIfClientCloses = outcomeGroupIfClientCloses;
        CachedBlockIfClientCloses = outcomeBlockIfClientCloses;
    }

    private void OnResolutionChanged(string oldValue, string newValue)
    {
        if (string.IsNullOrEmpty(newValue)) return;

        if (GameSingleton.instance != null && GameSingleton.instance.preferenceOptionsManager != null)
            GameSingleton.instance.preferenceOptionsManager.ApplyResolutionLocally(newValue);
    }

    // ===========================
    // Client -> Server input commands
    // Only used by a pure client (the window that is NOT hosting); the
    // host drives DialogueManager directly without a network round-trip.
    // ===========================

    [Command]
    public void CmdRequestContinue()
    {
        GameSingleton.instance.dialogueManager.DialogueContinueClicked();
    }

    [Command]
    public void CmdRequestStartFastForward()
    {
        GameSingleton.instance.dialogueManager.StartFastForward();
    }

    [Command]
    public void CmdRequestStopFastForward()
    {
        GameSingleton.instance.dialogueManager.StopFastForward();
    }

    [Command]
    public void CmdSelectChoice(int choiceIndex)
    {
        GameSingleton.instance.dialogueManager.ResolveActiveChoiceByIndex(choiceIndex);
    }

    // ===========================
    // Server -> Client choice display
    // Both host and client instantiate identical choice UI from their own
    // local (identical) scene data, looked up by block ID + node index.
    // ===========================

    [ClientRpc]
    public void RpcShowChoiceUI(string blockID, int nodeIndex)
    {
        GameSingleton.instance.dialogueManager.ShowChoiceUILocally(blockID, nodeIndex);
    }

    [ClientRpc]
    public void RpcHideChoiceUI()
    {
        GameSingleton.instance.dialogueManager.HideChoiceUILocally();
    }
}
