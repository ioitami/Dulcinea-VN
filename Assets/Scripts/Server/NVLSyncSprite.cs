using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class NVLSyncSprite : NetworkBehaviour
{
    public Image image;

    [Header("Sprite Library")]
    public Sprite[] availableSprites;

    [SyncVar(hook = nameof(OnSpriteChanged))]
    private string syncedSpriteName = "";

    private void Awake()
    {
        if (image == null)
            image = GetComponent<Image>();
    }

    [Server]
    public void SetSprite(Sprite newSprite)
    {
        if (newSprite == null)
        {
            syncedSpriteName = "";
            return;
        }

        syncedSpriteName = newSprite.name;
    }

    private void OnSpriteChanged(string oldName, string newName)
    {
        if (string.IsNullOrEmpty(newName))
        {
            if (image != null)
                image.sprite = null;
            return;
        }

        Sprite found = FindSprite(newName);

        if (found != null)
        {
            if (image != null)
                image.sprite = found;
        }
        else
        {
            Debug.LogWarning($"[NVLSyncSprite] Sprite '{newName}' not found in library.");
        }
    }

    private void Update()
    {
        if (!isServer) return;
        if (image == null || image.sprite == null) return;

        if (image.sprite.name != syncedSpriteName)
            SetSprite(image.sprite);
    }

    private Sprite FindSprite(string spriteName)
    {
        foreach (Sprite sprite in availableSprites)
        {
            if (sprite != null && sprite.name == spriteName)
                return sprite;
        }

        return null;
    }
}