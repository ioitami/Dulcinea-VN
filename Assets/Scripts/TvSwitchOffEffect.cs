using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TVSwitchEffect : MonoBehaviour
{
    [Header("References")]
    public GameObject spriteParent;
    public Image blackOverlay;
    public Image whiteLine;
    public Image whiteFlash;

    [Header("Timings")]
    public float collapseDuration = 0.4f;
    public float expandDuration = 0.4f;
    public float flashDuration = 0.15f;
    public float fadeDuration = 0.5f;

    private RectTransform whiteLineRect;
    private Coroutine runningRoutine;


    private void Awake()
    {
        whiteLineRect = whiteLine.GetComponent<RectTransform>();
        InitializeState();
        spriteParent.SetActive(false); // start disabled
    }

    private void InitializeState()
    {
        SetAlpha(blackOverlay, 0);
        SetAlpha(whiteLine, 0);
        SetAlpha(whiteFlash, 0);
    }

    // 🔌 Call this to play the power-OFF effect
    public void PlayOffEffect()
    {
        spriteParent.SetActive(true);
        if (runningRoutine != null) StopCoroutine(runningRoutine);
        runningRoutine = StartCoroutine(TVOffRoutine());
    }

    // ⚡ Call this to play the power-ON effect
    public void PlayOnEffect()
    {
        spriteParent.SetActive(true);
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

        // Step 2: Collapse into white line
        SetAlpha(whiteLine, 1);
        whiteLineRect.anchorMin = new Vector2(0, 0.5f);
        whiteLineRect.anchorMax = new Vector2(1, 0.5f);
        whiteLineRect.sizeDelta = new Vector2(0, Screen.height);

        float elapsed = 0f;
        float startHeight = Screen.height;
        float endHeight = 2f;

        while (elapsed < collapseDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0, 1, elapsed / collapseDuration);
            float h = Mathf.Lerp(startHeight, endHeight, t);
            whiteLineRect.sizeDelta = new Vector2(0, h);
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

        // Done – keep screen black but disable after a short delay (optional)
        yield return new WaitForSeconds(0.05f);
        spriteParent.SetActive(false);
    }

    private IEnumerator TVOnRoutine()
    {
        // Step 1: Start with full black
        SetAlpha(blackOverlay, 1);
        SetAlpha(whiteLine, 0);
        SetAlpha(whiteFlash, 0);

        // Step 2: Expand from white line
        SetAlpha(whiteLine, 1);
        whiteLineRect.anchorMin = new Vector2(0, 0.5f);
        whiteLineRect.anchorMax = new Vector2(1, 0.5f);
        whiteLineRect.sizeDelta = new Vector2(0, 2f);

        float elapsed = 0f;
        float startHeight = 2f;
        float endHeight = Screen.height;

        while (elapsed < expandDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0, 1, elapsed / expandDuration);
            float h = Mathf.Lerp(startHeight, endHeight, t);
            whiteLineRect.sizeDelta = new Vector2(0, h);
            yield return null;
        }

        // Step 3: Flash white
        SetAlpha(whiteFlash, 1);
        yield return new WaitForSeconds(flashDuration);
        SetAlpha(whiteFlash, 0);
        SetAlpha(whiteLine, 0);

        // Step 4: Fade black overlay out
        float fadeTime = 0f;
        while (fadeTime < fadeDuration)
        {
            fadeTime += Time.deltaTime;
            float a = Mathf.Lerp(1f, 0f, fadeTime / fadeDuration);
            SetAlpha(blackOverlay, a);
            yield return null;
        }

        InitializeState();

        // Done – disable object to clean up
        yield return new WaitForSeconds(0.05f);
        spriteParent.SetActive(false);
    }

    private void SetAlpha(Image img, float a)
    {
        if (img == null) return;
        var c = img.color;
        c.a = a;
        img.color = c;
    }
}
