using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(CanvasGroup))]
public class UIFadeMoveIn : MonoBehaviour
{
    [Header("Timing")]
    public float initialDelay = 0f; // delay before animation starts

    [Header("Movement Settings")]
    public Vector3 moveOffset = new Vector3(0, -50f, 0); // start offset (from below)

    [Header("Duration Settings")]
    public float minDuration = 0.2f;
    public float maxDuration = 0.4f;

    [Header("Easing")]
    public AnimationCurve ease = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Vector3 initialPosition;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();
        initialPosition = rectTransform.anchoredPosition;
        canvasGroup.alpha = 0f;
    }

    private void OnEnable()
    {
        canvasGroup.alpha = 0f;
        StopAllCoroutines();
        StartCoroutine(AnimateIn());
    }

    private void OnDisable()
    {
        canvasGroup.alpha = 0f;
        StopAllCoroutines();
    }

    private IEnumerator AnimateIn()
    {
        Debug.Log("1");
        // Wait for optional delay before animation starts
        if (initialDelay > 0f)
            yield return new WaitForSeconds(initialDelay);

        float duration = Random.Range(minDuration, maxDuration);

        // Start from offset + transparent
        rectTransform.anchoredPosition = initialPosition + moveOffset;
        canvasGroup.alpha = 0f;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime; // unaffected by Time.timeScale
            float t = ease.Evaluate(elapsed / duration);

            rectTransform.anchoredPosition = Vector3.Lerp(
                initialPosition + moveOffset, initialPosition, t);

            canvasGroup.alpha = Mathf.Lerp(0f, 1f, t);

            yield return null;
        }
        Debug.Log("2");

        rectTransform.anchoredPosition = initialPosition;
        canvasGroup.alpha = 1f;
    }
}
