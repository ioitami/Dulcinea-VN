using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class SpriteAnimationPoint
{
    [Header("Target Transform Values")]
    public Vector3 targetLocalPosition;

    [Tooltip("Scale multiplier relative to the original scale (1 = same size, 1.2 = 20% bigger).")]
    public Vector3 relativeScale = Vector3.one;

    [Tooltip("Target local rotation in Euler angles (relative, additive).")]
    public Vector3 relativeLocalRotation; // in degrees

    [Header("Timing")]
    public float duration = 1f;
}

[System.Serializable]
public class SpriteAnimation
{
    [Header("Animation Settings")]
    public string animationName;

    [Tooltip("If true, starts from the sprite's current local transform.")]
    public bool startFromCurrentTransform = true;

    [Tooltip("Only used if 'startFromCurrentTransform' is false.")]
    public Vector3 startLocalPosition;

    [Tooltip("Only used if 'startFromCurrentTransform' is false.")]
    public Vector3 startLocalScale = Vector3.one;

    [Tooltip("Only used if 'startFromCurrentTransform' is false.")]
    public Vector3 startLocalRotation; // Euler angles

    [Tooltip("Sequence of points and durations.")]
    public List<SpriteAnimationPoint> points = new List<SpriteAnimationPoint>();
}

public class SpriteAnimationManager : MonoBehaviour
{
    [Header("Available Animations")]
    public List<SpriteAnimation> animations = new List<SpriteAnimation>();
    public GameObject test;
    private void Start()
    {
        PlayAnimation("Wobble", test.transform);
    }
    /// <summary>
    /// Plays a named animation on the given sprite transform.
    /// </summary>
    public void PlayAnimation(string animationName, Transform spriteTransform, System.Action onComplete = null)
    {
        SpriteAnimation anim = animations.Find(a => a.animationName == animationName);
        if (anim == null)
        {
            Debug.LogWarning($"Animation '{animationName}' not found.");
            return;
        }

        StartCoroutine(PlayAnimationRoutine(anim, spriteTransform, onComplete));
    }

    private IEnumerator PlayAnimationRoutine(SpriteAnimation anim, Transform spriteTransform, System.Action onComplete)
    {
        // Capture the original base transform (so relative changes make sense)
        Vector3 baseScale = spriteTransform.localScale;
        Quaternion baseRotation = spriteTransform.localRotation;

        if (!anim.startFromCurrentTransform)
        {
            spriteTransform.localPosition = anim.startLocalPosition;
            spriteTransform.localScale = anim.startLocalScale;
            spriteTransform.localRotation = Quaternion.Euler(anim.startLocalRotation);
            baseScale = spriteTransform.localScale;
            baseRotation = spriteTransform.localRotation;
        }

        foreach (var point in anim.points)
        {
            Vector3 startPos = spriteTransform.localPosition;
            Vector3 endPos = point.targetLocalPosition;

            Vector3 startScale = spriteTransform.localScale;
            Vector3 endScale = Vector3.Scale(baseScale, point.relativeScale); // relative to base scale

            Quaternion startRot = spriteTransform.localRotation;
            Quaternion endRot = baseRotation * Quaternion.Euler(point.relativeLocalRotation); // relative rotation

            float elapsed = 0f;

            while (elapsed < point.duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / point.duration);

                spriteTransform.localPosition = Vector3.Lerp(startPos, endPos, t);
                spriteTransform.localScale = Vector3.Lerp(startScale, endScale, t);
                spriteTransform.localRotation = Quaternion.Lerp(startRot, endRot, t);

                yield return null;
            }

            spriteTransform.localPosition = endPos;
            spriteTransform.localScale = endScale;
            spriteTransform.localRotation = endRot;

            // update new base for subsequent relative steps
            baseScale = endScale;
            baseRotation = endRot;
        }

        onComplete?.Invoke();
    }
}
