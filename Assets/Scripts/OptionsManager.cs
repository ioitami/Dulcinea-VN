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

public class OptionsManager : MonoBehaviour
{
    [Header("Resolution")]
    public TMP_Dropdown resolutionDropdown;

    // Default list — edit here to add/remove supported resolutions.
    public List<ResolutionOption> resolutionOptions = new List<ResolutionOption>
    {
        new ResolutionOption(1920, 1080),
        new ResolutionOption(1600, 900),
        new ResolutionOption(1280, 720),
        new ResolutionOption(960, 540),
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
    public void OnResolutionDropdownChanged(int index)
    {
        if (index < 0 || index >= resolutionOptions.Count) return;

        ResolutionOption chosen = resolutionOptions[index];
        SetResolution(chosen.width, chosen.height);
    }

    // The options menu is only reachable from window 1, but guard anyway:
    // a pure client should never be the one deciding resolution.
    public void SetResolution(int width, int height)
    {
        if (NetworkClient.active && !NetworkServer.active) return;

        ApplyResolutionLocally($"{width}x{height}");

        if (NetworkServer.active)
            NVLNetworkPlayer.hostInstance?.SetResolution(width, height);
    }

    // Applied on this window directly, and on window 2 via NVLNetworkPlayer's
    // synced resolution hook — same method, so both windows always agree.
    public void ApplyResolutionLocally(string resolution)
    {
        string[] parts = resolution.Split('x');
        if (parts.Length != 2) return;
        if (!int.TryParse(parts[0], out int width)) return;
        if (!int.TryParse(parts[1], out int height)) return;

        Screen.SetResolution(width, height, Screen.fullScreenMode);

        if (resolutionDropdown != null)
        {
            int index = IndexOfResolution(width, height);
            if (index >= 0)
                resolutionDropdown.SetValueWithoutNotify(index);
        }
    }
}
