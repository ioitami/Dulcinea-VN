using System;
using System.Runtime.InteropServices;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraViewportAdjuster : MonoBehaviour
{
    private Camera cam;

    [Header("Background Settings")]
    public SpriteRenderer backgroundRenderer;

    [Header("Startup Window Settings")]
    public int startupWidth = 800;
    public int startupHeight = 600;

    [Header("Smoothing Settings")]
    [Tooltip("SmoothDamp time for camera movement/zoom")]
    public float smoothTime = 0.03f;
    [Tooltip("Speed of ratio interpolation (higher = snappier)")]
    public float ratioLerpSpeed = 10f;

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
    [DllImport("user32.dll")]
    private static extern IntPtr GetActiveWindow();

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
    }
#endif

    private IntPtr windowHandle;
    private RECT lastRect;

    // For SmoothDamp
    private Vector3 velocity = Vector3.zero;
    private float sizeVelocity = 0f;

    // Targets
    private Vector3 targetPosition;
    private float targetOrthoSize;

    // Smoothed ratios
    private float currentXRatio = 0f;
    private float currentYRatio = 0f;

    void Awake()
    {
        // Start in windowed mode
        Screen.SetResolution(startupWidth, startupHeight, false);

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        windowHandle = GetActiveWindow();
#endif
    }

    void Start()
    {
        cam = GetComponent<Camera>();
        UpdateTargetCamera(true); // snap once
        cam.transform.position = targetPosition;
        cam.orthographicSize = targetOrthoSize;
    }

    void LateUpdate()
    {
        if (CheckWindowChanged())
        {
            UpdateTargetCamera();
        }

        // Smooth follow for position
        cam.transform.position = Vector3.SmoothDamp(
            cam.transform.position,
            targetPosition,
            ref velocity,
            smoothTime
        );

        // Smooth follow for zoom
        cam.orthographicSize = Mathf.SmoothDamp(
            cam.orthographicSize,
            targetOrthoSize,
            ref sizeVelocity,
            smoothTime
        );
    }

    bool CheckWindowChanged()
    {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        if (GetWindowRect(windowHandle, out RECT rect))
        {
            if (rect.left != lastRect.left ||
                rect.top != lastRect.top ||
                rect.right != lastRect.right ||
                rect.bottom != lastRect.bottom)
            {
                lastRect = rect;
                return true;
            }
        }
#endif
        return false;
    }

    void UpdateTargetCamera(bool snapRatios = false)
    {
        if (backgroundRenderer == null) return;

        Bounds bgBounds = backgroundRenderer.bounds;
        float bgWidth = bgBounds.size.x;
        float bgHeight = bgBounds.size.y;

        int displayWidth = Display.main.systemWidth;
        int displayHeight = Display.main.systemHeight;

        int winX = 0, winY = 0, winW = Screen.width, winH = Screen.height;

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        if (GetWindowRect(windowHandle, out RECT rect))
        {
            winX = rect.left;
            winY = rect.top;
            winW = rect.right - rect.left;
            winH = rect.bottom - rect.top;
        }
#endif

        // Raw ratios
        float rawXRatio = Mathf.Clamp01((float)winX / (displayWidth - winW));
        float rawYRatio = Mathf.Clamp01((float)(displayHeight - winY - winH) / (displayHeight - winH));

        // Smooth ratios
        if (snapRatios)
        {
            currentXRatio = rawXRatio;
            currentYRatio = rawYRatio;
        }
        else
        {
            currentXRatio = Mathf.Lerp(currentXRatio, rawXRatio, Time.deltaTime * ratioLerpSpeed);
            currentYRatio = Mathf.Lerp(currentYRatio, rawYRatio, Time.deltaTime * ratioLerpSpeed);
        }

        // Visible portion
        float visibleHeight = bgHeight * ((float)winH / displayHeight);
        targetOrthoSize = visibleHeight / 2f;
        float visibleWidth = bgWidth * ((float)winW / displayWidth);

        // Clamp camera inside background
        float minX = bgBounds.min.x + visibleWidth / 2f;
        float maxX = bgBounds.max.x - visibleWidth / 2f;
        float minY = bgBounds.min.y + visibleHeight / 2f;
        float maxY = bgBounds.max.y - visibleHeight / 2f;

        float camX = Mathf.Lerp(minX, maxX, currentXRatio);
        float camY = Mathf.Lerp(minY, maxY, currentYRatio);

        targetPosition = new Vector3(camX, camY, cam.transform.position.z);
    }
}
