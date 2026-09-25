using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public abstract class UIScreenBase : MonoBehaviour
{
    [Tooltip("Lookup key used by UIController.EnableScreen/DisableScreen/GetScreen.")]
    public string screenName;

    public CanvasGroup canvasGroup;

    private static readonly Dictionary<string, UIScreenBase> registry = new Dictionary<string, UIScreenBase>();

    // Called by UIController.Awake() for every UIScreenBase in the scene.
    public void Register()
    {
        if (string.IsNullOrEmpty(screenName)) return;

        if (registry.ContainsKey(screenName))
        {
            Debug.LogWarning($"[UIScreenBase] Duplicate screen name '{screenName}' on '{name}' — overwriting previous registration.");
        }

        registry[screenName] = this;
    }

    protected virtual void OnDestroy()
    {
        if (string.IsNullOrEmpty(screenName)) return;

        if (registry.TryGetValue(screenName, out UIScreenBase current) && current == this)
        {
            registry.Remove(screenName);
        }
    }

    public void SetScreenActive(bool active)
    {
        if (canvasGroup == null)
        {
            Debug.LogError($"[UIScreenBase] '{name}' has no CanvasGroup assigned.");
            return;
        }

        canvasGroup.alpha = active ? 1f : 0f;
        canvasGroup.interactable = active;
        canvasGroup.blocksRaycasts = active;
    }

    public static UIScreenBase Get(string screenName)
    {
        registry.TryGetValue(screenName, out UIScreenBase screen);

        if (screen == null)
        {
            Debug.LogError($"[UIScreenBase] No screen found with name '{screenName}'.");
        }

        return screen;
    }

    public static T Get<T>(string screenName) where T : UIScreenBase
    {
        return Get(screenName) as T;
    }

    public static IEnumerable<UIScreenBase> All => registry.Values;
}