using UnityEngine;

public enum DisplayMode
{
    Fullscreen,
    Borderless,
    Windowed
}

public class PreferencesOptionsMenu : MonoBehaviour
{
    public Transform preferencesOptionsCameraParent;

    // <summary>
    /// Changes screen mode.
    /// </summary>
    /// <param name="mode">Fullscreen, Borderless, Windowed</param>
    /// <param name="width">Used only in Windowed mode</param>
    /// <param name="height">Used only in Windowed mode</param>
    public void SetScreenMode(DisplayMode mode, int width = 1280, int height = 720)
    {
        switch (mode)
        {
            case DisplayMode.Fullscreen:
                SetFullscreenExclusive();
                break;

            case DisplayMode.Borderless:
                SetBorderlessFullscreen();
                break;

            case DisplayMode.Windowed:
                SetWindowed(width, height);
                break;
        }

        // Reapply 16:9 letterbox if camera has Letterbox16x9
        var cam = Camera.main.GetComponent<LetterboxAdjust>();
        if (cam != null)
            cam.ApplyLetterbox();
    }

    // ============================================
    // FULLSCREEN (Exclusive)
    // ============================================
    private void SetFullscreenExclusive()
    {
        Resolution best = GetLargest16x9Resolution();
        Screen.SetResolution(best.width, best.height, FullScreenMode.ExclusiveFullScreen);
    }

    // ============================================
    // BORDERLESS (Fullscreen Window)
    // ============================================
    private void SetBorderlessFullscreen()
    {
        Resolution best = GetLargest16x9Resolution();
        Screen.SetResolution(best.width, best.height, FullScreenMode.FullScreenWindow);
    }

    // ============================================
    // WINDOWED
    // ============================================
    private void SetWindowed(int width, int height)
    {
        Screen.SetResolution(width, height, FullScreenMode.Windowed);
    }

    // ============================================
    // Picks largest 16:9 resolution available
    // ============================================
    private Resolution GetLargest16x9Resolution()
    {
        Resolution[] resolutions = Screen.resolutions;
        Resolution best = resolutions[0];

        foreach (var r in resolutions)
        {
            float aspect = (float)r.width / r.height;
            if (Mathf.Abs(aspect - (16f / 9f)) < 0.01f)
            {
                if (r.width > best.width)
                    best = r;
            }
        }

        return best;
    }
}
