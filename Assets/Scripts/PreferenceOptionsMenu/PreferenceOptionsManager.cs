using System;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;

// One dropdown entry. Add/remove entries in resolutionOptions (Inspector or
// code) to change what's offered — nothing else needs to change.
[Serializable]
public class ResolutionOption
{
    public int width;
    public int height;

    public ResolutionOption(int width, int height)
    {
        this.width = width;
        this.height = height;
    }

    public string Label => $"{width}x{height}";
}

public class PreferenceOptionsManager : MonoBehaviour
{
    [Header("Resolution")]
    public TMP_Dropdown resolutionDropdown;

    // Default list — edit here to add/remove supported resolutions.
    public List<ResolutionOption> resolutionOptions = new List<ResolutionOption>
    {
        new ResolutionOption(1920, 1440),
        new ResolutionOption(1600, 1200),
        new ResolutionOption(1440, 1050),
        new ResolutionOption(1280, 960),
        new ResolutionOption(1024, 768),
        new ResolutionOption(800, 600),
        new ResolutionOption(640, 480)
    };

    private void Start()
    {
        PopulateResolutionDropdown();
    }

    private void PopulateResolutionDropdown()
    {
        if (resolutionDropdown == null) return;

        List<string> labels = new List<string>();
        foreach (ResolutionOption option in resolutionOptions)
            labels.Add(option.Label);

        resolutionDropdown.ClearOptions();
        resolutionDropdown.AddOptions(labels);

        int currentIndex = IndexOfResolution(Screen.width, Screen.height);
        resolutionDropdown.SetValueWithoutNotify(Mathf.Max(currentIndex, 0));

        resolutionDropdown.onValueChanged.RemoveListener(OnResolutionDropdownChanged);
        resolutionDropdown.onValueChanged.AddListener(OnResolutionDropdownChanged);
    }

    private int IndexOfResolution(int width, int height)
    {
        for (int i = 0; i < resolutionOptions.Count; i++)
        {
            if (resolutionOptions[i].width == width && resolutionOptions[i].height == height)
                return i;
        }

        return -1;
    }

    // Wired to the dropdown's OnValueChanged in the Inspector.
    // Reads the label straight off the dropdown's own option list rather
    // than indexing into resolutionOptions — if the two ever have a
    // different count (e.g. leftover entries from scene setup), indexing
    // into a second list silently applies the wrong entry. Reading back
    // what's actually on screen can't desync like that.
    public void OnResolutionDropdownChanged(int index)
    {
        if (resolutionDropdown == null) return;
        if (index < 0 || index >= resolutionDropdown.options.Count) return;

        SetResolution(resolutionDropdown.options[index].text);
    }

    public void SetResolution(int width, int height)
    {
        SetResolution($"{width}x{height}");
    }

    // The options menu is only reachable from window 1, but guard anyway:
    // a pure client should never be the one deciding resolution.
    public void SetResolution(string resolution)
    {
        if (NetworkClient.active && !NetworkServer.active) return;

        ApplyResolutionLocally(resolution);

        if (NetworkServer.active && TryParseResolution(resolution, out int width, out int height))
            NVLNetworkPlayer.hostInstance?.SetResolution(width, height);
    }

    // Applied on this window directly, and on window 2 via NVLNetworkPlayer's
    // synced resolution hook — same method, so both windows always agree.
    public void ApplyResolutionLocally(string resolution)
    {
        if (!TryParseResolution(resolution, out int width, out int height)) return;

        Screen.SetResolution(width, height, Screen.fullScreenMode);

        if (resolutionDropdown != null)
        {
            int index = IndexOfResolution(width, height);
            if (index >= 0)
                resolutionDropdown.SetValueWithoutNotify(index);
        }
    }

    private static bool TryParseResolution(string resolution, out int width, out int height)
    {
        width = 0;
        height = 0;

        if (string.IsNullOrEmpty(resolution)) return false;

        string[] parts = resolution.Split('x');
        if (parts.Length != 2) return false;
        if (!int.TryParse(parts[0], out width)) return false;
        if (!int.TryParse(parts[1], out height)) return false;

        return true;
    }
}
