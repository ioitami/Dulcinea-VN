using UnityEngine;

[RequireComponent(typeof(Camera))]
public class LetterboxAdjust : MonoBehaviour
{
    private Camera cam;
    private float targetAspect = 16f / 9f;

    void Awake()
    {
        cam = GetComponent<Camera>();
        ApplyLetterbox();
    }

    void OnPreCull() => ApplyLetterbox();

    public void ApplyLetterbox()
    {
        float windowAspect = (float)Screen.width / Screen.height;
        float scaleHeight = windowAspect / targetAspect;

        if (scaleHeight < 1f)
        {
            // Add black bars top/bottom
            Rect rect = cam.rect;
            rect.width = 1f;
            rect.height = scaleHeight;
            rect.x = 0;
            rect.y = (1f - scaleHeight) / 2f;
            cam.rect = rect;
        }
        else
        {
            // Add black bars left/right
            float scaleWidth = 1f / scaleHeight;
            Rect rect = cam.rect;
            rect.width = scaleWidth;
            rect.height = 1f;
            rect.x = (1f - scaleWidth) / 2f;
            rect.y = 0;
            cam.rect = rect;
        }
    }
}
