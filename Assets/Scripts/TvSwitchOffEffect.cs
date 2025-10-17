using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TVSwitchEffect : MonoBehaviour
{
    public GameObject parentControl;
    [Header("References")]
    public Image blackOverlay;
    public Image whiteLine;
    public Image whiteFlash;

    [Header("Timings")]
    public float collapseDuration = 0.4f;
    public float expandDuration = 0.6f;
    public float flashDuration = 0.15f;
    public float fadeDuration = 0.5f;

    [Header("Bar Widths")]
    public float minBarWidth = 1f;     // initial width (slit)
    public float maxBarWidth = 100f;   // width before full screen expansion

    [Header("Animation Curves")]
    public AnimationCurve expansionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public AnimationCurve collapseCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private RectTransform whiteLineRect;
    private Coroutine runningRoutine;

    private void Awake()
    {
        whiteLineRect = whiteLine.GetComponent<RectTransform>();
        InitializeState();
        parentControl.SetActive(false);
    }

    private void InitializeState()
    {
        SetAlpha(blackOverlay, 0);
        SetAlpha(whiteLine, 0);
        SetAlpha(whiteFlash, 0);
    }

    // 🔌 Collapse (TV off)
    public void PlayOffEffect()
    {
        parentControl.SetActive(true);
        if (runningRoutine != null) StopCoroutine(runningRoutine);
        runningRoutine = StartCoroutine(TVOffRoutine());
    }

    // ⚡ Expand (TV on)
    public void PlayOnEffect()
    {
        parentControl.SetActive(true);
        if (runningRoutine != null) StopCoroutine(runningRoutine);
        runningRoutine = StartCoroutine(TVOnRoutine());
    }

    private IEnumerator TVOffRoutine()
    {
        InitializeState();

        // Step 1: Flash white
        SetAlpha(whiteFlash, 1);
        yield return new WaitForSeconds(flashDuration);
        SetAlpha(whiteFlash, 0);

        // Step 2: Collapse horizontally into a vertical line
        SetAlpha(whiteLine, 1);
        whiteLineRect.anchorMin = new Vector2(0.5f, 0);
        whiteLineRect.anchorMax = new Vector2(0.5f, 1);
        whiteLineRect.sizeDelta = new Vector2(Screen.width, 0);

        float elapsed = 0f;
        while (elapsed < collapseDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / collapseDuration);
            float curveValue = collapseCurve.Evaluate(t);
            float w = Mathf.Lerp(Screen.width, minBarWidth, curveValue);
            whiteLineRect.sizeDelta = new Vector2(w, 0);
            yield return null;
        }

        // Step 3: Fade to black
        SetAlpha(whiteLine, 0);
        float fadeTime = 0f;
        while (fadeTime < fadeDuration)
        {
            fadeTime += Time.deltaTime;
            float a = Mathf.Lerp(0f, 1f, fadeTime / fadeDuration);
            SetAlpha(blackOverlay, a);
            yield return null;
        }

        SetAlpha(blackOverlay, 1);
        yield return new WaitForSeconds(0.05f);
        parentControl.SetActive(false);
    }

    private IEnumerator TVOnRoutine()
    {
        // Step 1: Start black
        SetAlpha(blackOverlay, 1);
        SetAlpha(whiteLine, 0);
        SetAlpha(whiteFlash, 0);

        // Step 2: Begin with thin slit
        SetAlpha(whiteLine, 1);
        whiteLineRect.anchorMin = new Vector2(0.5f, 0);
        whiteLineRect.anchorMax = new Vector2(0.5f, 1);
        whiteLineRect.sizeDelta = new Vector2(minBarWidth, 0);

        float elapsed = 0f;
        while (elapsed < expandDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / expandDuration);
            float curveValue = expansionCurve.Evaluate(t);

            // You can use the curve to create a “flicker” or overshoot
            float w = Mathf.Lerp(minBarWidth, Screen.width/2, curveValue);
            whiteLineRect.sizeDelta = new Vector2(w, 0);
            yield return null;
        }

        // Step 3: Flash white
        SetAlpha(whiteFlash, 1);
        yield return new WaitForSeconds(flashDuration);
        SetAlpha(whiteFlash, 0);
        SetAlpha(whiteLine, 0);

        // Step 4: Fade out black overlay
        float fadeTime = 0f;
        while (fadeTime < fadeDuration)
        {
            fadeTime += Time.deltaTime;
            float a = Mathf.Lerp(1f, 0f, fadeTime / fadeDuration);
            SetAlpha(blackOverlay, a);
            yield return null;
        }

        InitializeState();
        yield return new WaitForSeconds(0.05f);
        parentControl.SetActive(false);
    }

    private void SetAlpha(Image img, float a)
    {
        if (img == null) return;
        var c = img.color;
        c.a = a;
        img.color = c;
    }
}
