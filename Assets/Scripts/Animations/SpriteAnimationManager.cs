using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// EXAMPLE USE:
//void Start()
//{
//     animationManager.PlayAnimation(animationName:animName, spriteTransform: character.ingameContainerObj.transform, onComplete:() => GameSingleton.instance.characterManager.SetCharacterMood("d",1));
//}


[System.Serializable]
public class SpriteAnimationPoint
{
    [Header("Target Transform Values")]
    public Vector3 targetLocalPosition;
    public Vector3 relativeScale = Vector3.one;
    public Vector3 relativeLocalRotation;

    [Header("Timing & Easing")]
    [Min(0f)] public float duration = 1f;
    [Min(0f)] public float delayAfter = 0f;
    public AnimationCurve movementCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

    [Header("Events")]
    public UnityEvent onPointReached;
}

public enum LoopType { None, Loop, PingPong }

[System.Serializable]
public class SpriteAnimation
{
    [Header("Animation Settings")]
    public string animationName;

    public bool startFromCurrentTransform = true;
    public Vector3 startLocalPosition;
    public Vector3 startLocalScale = Vector3.one;
    public Vector3 startLocalRotation;

    [Header("Playback Options")]
    public LoopType loopType = LoopType.None;
    public int loopCount = 0; // 0 = infinite

    [Header("Sequence")]
    public List<SpriteAnimationPoint> points = new List<SpriteAnimationPoint>();
}

public class SpriteAnimationManager : MonoBehaviour
{
    [Header("Available Animations")]
    public List<SpriteAnimation> animations = new List<SpriteAnimation>();

    // Track one coroutine per Transform
    private Dictionary<Transform, Coroutine> activeRoutines = new Dictionary<Transform, Coroutine>();

    public void PlayAnimation(string animationName, Transform spriteTransform, System.Action onComplete = null)
    {
        SpriteAnimation anim = animations.Find(a => a.animationName == animationName);
        if (anim == null)
        {
            Debug.LogWarning($"Animation '{animationName}' not found.");
            return;
        }

        // Stop any existing animation for this sprite first
        StopAnimationForTransform(spriteTransform);

        Coroutine routine = StartCoroutine(PlayAnimationRoutine(anim, spriteTransform, () =>
        {
            activeRoutines.Remove(spriteTransform);
            onComplete?.Invoke();
        }));

        activeRoutines[spriteTransform] = routine;
    }

    private IEnumerator PlayAnimationRoutine(SpriteAnimation anim, Transform spriteTransform, System.Action onComplete)
    {
        if (anim.points.Count == 0)
        {
            Debug.LogWarning($"Animation '{anim.animationName}' has no points.");
            yield break;
        }

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

        bool reverse = false;
        int currentLoop = 0;

        while (true)
        {
            List<SpriteAnimationPoint> activePoints = reverse ? new List<SpriteAnimationPoint>(anim.points) : anim.points;
            if (reverse) activePoints.Reverse();

            foreach (var point in activePoints)
            {
                Vector3 startPos = spriteTransform.localPosition;
                Vector3 endPos = point.targetLocalPosition;
                Vector3 startScale = spriteTransform.localScale;
                Vector3 endScale = Vector3.Scale(baseScale, point.relativeScale);
                Quaternion startRot = spriteTransform.localRotation;
                Quaternion endRot = baseRotation * Quaternion.Euler(point.relativeLocalRotation);

                float elapsed = 0f;
                while (elapsed < point.duration)
                {
                    elapsed += Time.deltaTime;
                    float rawT = Mathf.Clamp01(elapsed / point.duration);
                    float curvedT = point.movementCurve.Evaluate(rawT);

                    spriteTransform.localPosition = Vector3.Lerp(startPos, endPos, curvedT);
                    spriteTransform.localScale = Vector3.Lerp(startScale, endScale, curvedT);
                    spriteTransform.localRotation = Quaternion.Lerp(startRot, endRot, curvedT);

                    yield return null;
                }

                spriteTransform.localPosition = endPos;
                spriteTransform.localScale = endScale;
                spriteTransform.localRotation = endRot;

                point.onPointReached?.Invoke();

                if (point.delayAfter > 0f)
                    yield return new WaitForSeconds(point.delayAfter);

                baseScale = endScale;
                baseRotation = endRot;
            }

            if (anim.loopType == LoopType.None)
                break;

            if (anim.loopType == LoopType.PingPong)
                reverse = !reverse;

            currentLoop++;
            if (anim.loopCount > 0 && currentLoop >= anim.loopCount)
                break;
        }

        onComplete?.Invoke();
    }

    // Stop animation for a specific Transform
    public void StopAnimationForTransform(Transform target)
    {
        if (activeRoutines.TryGetValue(target, out Coroutine routine))
        {
            if (routine != null)
                StopCoroutine(routine);
            activeRoutines.Remove(target);
        }
    }

    // Stop all running animations
    public void StopAllAnimations()
    {
        foreach (var kvp in activeRoutines)
        {
            if (kvp.Value != null)
                StopCoroutine(kvp.Value);
        }
        activeRoutines.Clear();
    }

    // Instantly jump a specific sprite to the end of a named animation
    public void SkipToEnd(string animationName, Transform spriteTransform)
    {
        SpriteAnimation anim = animations.Find(a => a.animationName == animationName);
        if (anim == null || anim.points.Count == 0)
        {
            Debug.LogWarning($"Animation '{animationName}' not found or has no points.");
            return;
        }

        StopAnimationForTransform(spriteTransform);

        SpriteAnimationPoint lastPoint = anim.points[anim.points.Count - 1];

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

        Vector3 finalPosition = lastPoint.targetLocalPosition;
        Vector3 finalScale = Vector3.Scale(baseScale, lastPoint.relativeScale);
        Quaternion finalRotation = baseRotation * Quaternion.Euler(lastPoint.relativeLocalRotation);

        spriteTransform.localPosition = finalPosition;
        spriteTransform.localScale = finalScale;
        spriteTransform.localRotation = finalRotation;
    }


    // Instantly completes all currently running animations,
    // moving every sprite to its final state.
    public void SkipAllToEnd()
    {
        // Make a copy of the current running animations to avoid modification errors
        List<(Transform, string)> activeAnimations = new List<(Transform, string)>();

        foreach (var kvp in activeRoutines)
        {
            Transform spriteTransform = kvp.Key;

            // Try to find which animation this sprite is playing
            // (optional Edepends on your tracking setup)
            // For now, assume we skip to the last animation played on it
            // You can extend this later with per-transform tracking.
            SpriteAnimation anim = FindLastPlayedAnimationFor(spriteTransform);
            if (anim != null)
                activeAnimations.Add((spriteTransform, anim.animationName));
        }

        // Stop everything first
        StopAllAnimations();

        // Move each sprite to its final animation state
        foreach (var (spriteTransform, animName) in activeAnimations)
        {
            SkipToEnd(animName, spriteTransform);
        }
    }


    // Optional helper that tries to find the last animation
    // played for a given transform. You can expand this later
    // by storing last-played animation names in a dictionary.
    private SpriteAnimation FindLastPlayedAnimationFor(Transform spriteTransform)
    {
        // PLACEHOLDER Ereturn the first or default animation
        // extend this to remember last-played animation per sprite.
        if (animations.Count > 0)
            return animations[0];
        return null;
    }

    // Returns true if any sprite is currently running an animation.
    public bool IsAnyAnimationPlaying()
    {
        return activeRoutines.Count > 0;
    }

    public bool IsAnimationPlayingFor(Transform spriteTransform)
    {
        return activeRoutines.ContainsKey(spriteTransform);
    }
}
