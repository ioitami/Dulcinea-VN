using UnityEngine;

[ExecuteAlways] // Updates in Editor too
[RequireComponent(typeof(Camera))]
public class CanvasCameraFramer : MonoBehaviour
{
    [Tooltip("The world-space canvas to frame in view.")]
    public RectTransform targetCanvas;

    private Camera cam;
    private Vector2 lastScreenSize;
    private Vector3 lastCanvasScale;
    private Vector2 lastCanvasSize;

    void Awake()
    {
        cam = GetComponent<Camera>();
        CacheCanvasState();
        UpdateCameraFraming();
    }

    void Update()
    {
        // Check if screen resolution changed
        if (Screen.width != (int)lastScreenSize.x || Screen.height != (int)lastScreenSize.y)
        {
            UpdateCameraFraming();
            CacheCanvasState();
        }

        // Check if canvas dimensions or scale changed
        if (targetCanvas != null)
        {
            if (targetCanvas.rect.size != lastCanvasSize ||
                targetCanvas.lossyScale != lastCanvasScale)
            {
                UpdateCameraFraming();
                CacheCanvasState();
            }
        }
    }

    void CacheCanvasState()
    {
        if (targetCanvas != null)
        {
            lastCanvasSize = targetCanvas.rect.size;
            lastCanvasScale = targetCanvas.lossyScale;
        }
        lastScreenSize = new Vector2(Screen.width, Screen.height);
    }

    void UpdateCameraFraming()
    {
        if (targetCanvas == null) return;

        float width = targetCanvas.rect.width * targetCanvas.lossyScale.x;
        float height = targetCanvas.rect.height * targetCanvas.lossyScale.y;
        Vector3 canvasCenter = targetCanvas.position;

        if (cam.orthographic)
        {
            // ----- ORTHOGRAPHIC -----
            cam.transform.position = canvasCenter + new Vector3(0, 0, -10);
            cam.transform.rotation = Quaternion.identity;

            cam.orthographicSize = height / 2f;
            float screenAspect = (float)Screen.width / Screen.height;
            float canvasAspect = width / height;

            if (canvasAspect > screenAspect)
            {
                cam.orthographicSize = (width / 2f) / screenAspect;
            }
        }
        else
        {
            // ----- PERSPECTIVE -----
            float fov = cam.fieldOfView * Mathf.Deg2Rad;
            float aspect = (float)Screen.width / Screen.height;

            // Distance needed for height
            float distHeight = (height / 2f) / Mathf.Tan(fov / 2f);

            // Distance needed for width
            float fovWidth = 2f * Mathf.Atan(Mathf.Tan(fov / 2f) * aspect);
            float distWidth = (width / 2f) / Mathf.Tan(fovWidth / 2f);

            float dist = Mathf.Max(distHeight, distWidth);

            cam.transform.position = canvasCenter - cam.transform.forward * dist;
            cam.transform.LookAt(canvasCenter, Vector3.up);
        }
    }
}
