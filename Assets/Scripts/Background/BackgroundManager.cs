using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class BackgroundManager : MonoBehaviour
{
    [SerializeField]
    public Transform backgroundSpirteParent_MainMenu;
    public Transform backgroundSpriteParent_Window1;
    public Transform backgroundSpriteParent_Window2;

    [Header("Backgrounds List")]
    public List<BackgroundPreset> backgrounds = new List<BackgroundPreset>();

    // Add other functions to manage bg as needed

    private void Awake()
    {
        RemoveChildren(backgroundSpriteParent_Window1);
        RemoveChildren(backgroundSpriteParent_Window2);

        // Set Main Menu BG based on Save?
        SetMainMenuBackground(0);

        // Set Default backgroundSprite for window2 (When player opens window 2 before starting/loading game on win 1)
    }

    void RemoveChildren(Transform parent)
    {
        foreach (Transform child in parent)
        {
            Destroy(child.gameObject);
        }
    }

    // The options menu / dialogue nodes are only ever driven from window 1,
    // but guard anyway: a pure client should never be the one deciding
    // what's on screen — it only ever follows what the host sets.
    private bool CanSetBackgroundLocally()
    {
        return !(NetworkClient.active && !NetworkServer.active);
    }

    // WILL REPLACE PREVIOUS BG IF ANY
    public void SetBackground(string backgroundName, int windowNumber)
    {
        if (!CanSetBackgroundLocally()) return;

        ApplyBackgroundLocally(backgroundName, windowNumber);

        if (NetworkServer.active)
        {
            BackgroundTarget target = windowNumber == 1 ? BackgroundTarget.Window1 : BackgroundTarget.Window2;
            NVLNetworkPlayer.hostInstance?.SetBackground(target, backgroundName);
        }
    }

    public void SetBackground(int bgIndex, int windowNumber)
    {
        if (bgIndex < 0 || bgIndex >= backgrounds.Count)
        {
            Debug.LogWarning($"Background index '{bgIndex}' is out of range!");
            return;
        }

        // Resolve to a name before doing anything else — syncing a raw
        // index is fragile if the two windows' backgrounds lists were
        // ever built or ordered slightly differently.
        SetBackground(backgrounds[bgIndex].backgroundName, windowNumber);
    }

    public void SetMainMenuBackground(string backgroundName)
    {
        if (!CanSetBackgroundLocally()) return;

        ApplyMainMenuBackgroundLocally(backgroundName);

        if (NetworkServer.active)
            NVLNetworkPlayer.hostInstance?.SetBackground(BackgroundTarget.MainMenu, backgroundName);
    }

    public void SetMainMenuBackground(int bgIndex)
    {
        if (bgIndex < 0 || bgIndex >= backgrounds.Count)
        {
            Debug.LogWarning($"Background number '{bgIndex}' not found!");
            return;
        }

        SetMainMenuBackground(backgrounds[bgIndex].backgroundName);
    }

    // Applied on this window directly, and on the other window via
    // NVLNetworkPlayer's synced background hooks — same method, so both
    // windows always agree.
    public void ApplyBackgroundLocally(string backgroundName, int windowNumber)
    {
        BackgroundPreset preset = backgrounds.Find(b => b.backgroundName == backgroundName);

        if (preset == null)
        {
            Debug.LogWarning($"Background '{backgroundName}' not found!");
            return;
        }

        Transform parent = windowNumber == 1 ? backgroundSpriteParent_Window1 : backgroundSpriteParent_Window2;

        RemoveChildren(parent);

        GameObject bgInstance = Instantiate(preset.backgroundPrefab);
        bgInstance.transform.SetParent(parent, false);
        bgInstance.transform.localPosition = Vector3.zero;
    }

    public void ApplyMainMenuBackgroundLocally(string backgroundName)
    {
        BackgroundPreset preset = backgrounds.Find(b => b.backgroundName == backgroundName);

        if (preset == null)
        {
            Debug.LogWarning($"Background '{backgroundName}' not found!");
            return;
        }

        Transform parent = backgroundSpirteParent_MainMenu;

        RemoveChildren(parent);

        GameObject bgInstance = Instantiate(preset.backgroundPrefab);
        bgInstance.transform.SetParent(parent, false);
        bgInstance.transform.localPosition = Vector3.zero;
    }
}

[System.Serializable]
public class BackgroundPreset
{
    public string backgroundName;
    public GameObject backgroundPrefab;
}