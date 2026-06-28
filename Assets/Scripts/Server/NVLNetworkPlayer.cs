using Mirror;
using UnityEngine;

public class NVLNetworkPlayer : NetworkBehaviour
{
    public static NVLNetworkPlayer localPlayer { get; private set; }

    public override void OnStartLocalPlayer()
    {
        localPlayer = this;
        Debug.Log("[NVLNetworkPlayer] Local player started.");
    }

    // ===========================
    // Client Å® Server commands
    // (empty for now)
    // ===========================
}