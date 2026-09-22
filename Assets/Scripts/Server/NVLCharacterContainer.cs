using Mirror;
using UnityEngine;

public class NVLCharacterContainer : NetworkBehaviour
{
    [SyncVar]
    public string characterStableID;

    [SyncVar]
    public int windowNumber;

    [Header("Wiring")]
    public GameObject visual;
    public bool IsVisualActive => visual != null && visual.activeSelf;

    public override void OnStartServer()
    {
        base.OnStartServer();

        AttachToWindowParent();
        RegisterWithManager();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (isServer) return;

        AttachToWindowParent();
        RegisterWithManager();
    }

    public void SetVisualActive(bool active)
    {
        if (visual != null)
        {
            visual.SetActive(active);
        }
    }

    private void AttachToWindowParent()
    {
        if (CharacterManager.instance == null)
        {
            Debug.LogWarning("[NVLCharacterContainer] No CharacterManager.instance found to parent under.");
            return;
        }

        Transform parent = windowNumber == 2
            ? CharacterManager.instance.characterSpriteParent_Window2
            : CharacterManager.instance.characterSpriteParent_Window1;

        transform.SetParent(parent, false);
        transform.localPosition = Vector3.zero;
    }

    private void RegisterWithManager()
    {
        if (CharacterManager.instance == null) return;
        if (string.IsNullOrEmpty(characterStableID)) return;

        CharacterManager.instance.RegisterSpawnedContainer(characterStableID, gameObject);
    }
}