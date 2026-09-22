using Mirror;
using UnityEngine;

public class NVLSyncSpriteRenderer : NetworkBehaviour
{
    public SpriteRenderer spriteRenderer;

    [Header("Sprite Library")]
    public Sprite[] availableSprites;

    [SyncVar(hook = nameof(OnSpriteChanged))]
    private string syncedSpriteName = "";

    private void Awake()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
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

    public void ApplyCurrentSprite()
    {
        OnSpriteChanged(syncedSpriteName, syncedSpriteName);
    }

    private void OnSpriteChanged(string oldName, string newName)
    {
        if (string.IsNullOrEmpty(newName))
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = null;
            }

            return;
        }

        Sprite found = FindSprite(newName);

        if (found != null)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = found;
            }
        }
        else
        {
            Debug.LogWarning($"[NVLSyncSpriteRenderer] Sprite '{newName}' not found in library.");
        }
    }

    private void Update()
    {
        if (!isServer) return;
        if (spriteRenderer == null || spriteRenderer.sprite == null) return;

        if (spriteRenderer.sprite.name != syncedSpriteName)
        {
            SetSprite(spriteRenderer.sprite);
        }
    }

    private Sprite FindSprite(string spriteName)
    {
        foreach (Sprite sprite in availableSprites)
        {
            if (sprite != null && sprite.name == spriteName)
            {
                return sprite;
            }
        }

        return null;
    }

}