using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    [Header("UI Components")]
    public Image nextIcon;

    [Header("Typing Settings")]
    public float typingSpeed = 0.03f;
    public float fastForwardDelay = 0.01f;

    [Header("Next Icon Blink Settings")]
    public float blinkSpeed = 0.5f;

    // Accessed by nodes directly
    public DialogueBlock currentBlock;
    public bool requireClickToContinue;

    // Internal state
    private int currentNodeIndex;
    private bool isTyping;
    private bool waitingForClick;
    private string lastTypedText;
    private Action pendingOnComplete;

    private Coroutine typingCoroutine;
    private Coroutine blinkCoroutine;
    private Coroutine fastForwardCoroutine;

    public bool isFastForwarding { get; private set; }

    // ===========================
    // Public API
    // ===========================

    public void PlayBlock(DialogueBlock block)
    {
        if (block == null) return;

        currentBlock = block;
        currentNodeIndex = 0;
        isTyping = false;
        waitingForClick = false;
        requireClickToContinue = false;
        isFastForwarding = false;

        // Clear the textbox for the new block
        if (currentBlock.textBox != null)
            currentBlock.textBox.text = "";

        SetNextIconVisible(false);
        ProcessNextNode();
    }

    public void OnContinueClicked()
    {
        

        if (isFastForwarding)
        {
            StopFastForward();
            return;
        }

        if (isTyping)
        {
            SkipTyping();
            return;
        }

        if (waitingForClick)
        {
            waitingForClick = false;
            SetNextIconVisible(false);

            // Fire the onComplete that was suspended, which advances to the next node
            Action callback = pendingOnComplete;
            pendingOnComplete = null;
            callback?.Invoke();
        }
    }

    public void StartFastForward()
    {
        if (isFastForwarding) return;
        isFastForwarding = true;

        if (fastForwardCoroutine != null) StopCoroutine(fastForwardCoroutine);
        fastForwardCoroutine = StartCoroutine(FastForwardRoutine());
    }

    public void StopFastForward()
    {
        isFastForwarding = false;

        if (fastForwardCoroutine != null)
        {
            StopCoroutine(fastForwardCoroutine);
            fastForwardCoroutine = null;
        }
    }

    public void ChangeTypingSpeed(float speed)
    {
        typingSpeed = Mathf.Max(speed, 0.0001f);
    }

    // ===========================
    // Node Processing
    // ===========================

    private void ProcessNextNode()
    {
        if (currentBlock == null)
        {
            Debug.Log("[DialogueManager] No current block to process.");
            return;
        }

        // Hard stop — do not advance until the player clicks
        if (waitingForClick) return;

        if (currentNodeIndex >= currentBlock.nodes.Length)
        {
            OnBlockFinished();
            return;
        }

        DialogueBlockNode node = currentBlock.nodes[currentNodeIndex];
        currentNodeIndex++;

        if (node == null)
        {
            ProcessNextNode();
            return;
        }

        node.Execute(this, ProcessNextNode);
    }

    // ===========================
    // Typing (called by DialogueTextNode)
    // ===========================

    public void StartTyping(string text, float speed, bool append, bool requireClick, Action onComplete)
    {
        if (append && currentBlock.textBox != null)
            lastTypedText = currentBlock.textBox.text + text;
        else
            lastTypedText = text;


        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeRoutine(text, speed, append, requireClick, onComplete));
    }

    private IEnumerator TypeRoutine(string text, float speed, bool append, bool requireClick, Action onComplete)
    {
        isTyping = true;

        if (currentBlock.textBox != null && !append)
            currentBlock.textBox.text = "";

        int i = 0;
        while (i < text.Length)
        {
            if (text[i] == '<')
            {
                // Find the closing bracket and append the whole tag at once
                int closeIndex = text.IndexOf('>', i);
                if (closeIndex != -1)
                {
                    string tag = text.Substring(i, closeIndex - i + 1);
                    if (currentBlock.textBox != null)
                        currentBlock.textBox.text += tag;
                    i = closeIndex + 1;
                    continue;
                }
            }

            if (currentBlock.textBox != null)
                currentBlock.textBox.text += text[i];

            i++;
            yield return new WaitForSeconds(speed);
        }

        isTyping = false;
        OnTextFinished(requireClick, onComplete);
    }

    private void SkipTyping()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        if (currentBlock.textBox != null)
            currentBlock.textBox.text = lastTypedText;

        isTyping = false;

        // Determine requireClick from the node that was interrupted
        int lastIndex = currentNodeIndex - 1;
        bool requireClick = requireClickToContinue;
        if (lastIndex >= 0 && lastIndex < currentBlock.nodes.Length)
            if (currentBlock.nodes[lastIndex] is DialogueTextNode tn)
                requireClick = tn.requirePlayerClickContinue || requireClickToContinue;

        // Re-create the onComplete that TypeRoutine would have called
        OnTextFinished(requireClick, ProcessNextNode);
    }

    private void OnTextFinished(bool requireClick, Action onComplete)
    {
        if (requireClick || requireClickToContinue)
        {
            waitingForClick = true;
            pendingOnComplete = onComplete;
            SetNextIconVisible(true);
        }
        else
        {
            onComplete?.Invoke();
        }
    }

    // ===========================
    // Fast Forward
    // ===========================

    private IEnumerator FastForwardRoutine()
    {
        while (isFastForwarding)
        {
            if (isTyping)
                SkipTyping();
            else if (waitingForClick)
                OnContinueClicked();

            yield return new WaitForSeconds(fastForwardDelay);
        }
    }

    // ===========================
    // Next Icon
    // ===========================

    private void SetNextIconVisible(bool visible)
    {
        if (nextIcon == null) return;

        nextIcon.gameObject.SetActive(visible);

        if (visible)
        {
            if (blinkCoroutine != null) StopCoroutine(blinkCoroutine);
            blinkCoroutine = StartCoroutine(BlinkIcon());
        }
        else
        {
            if (blinkCoroutine != null)
            {
                StopCoroutine(blinkCoroutine);
                blinkCoroutine = null;
            }
        }
    }

    private IEnumerator BlinkIcon()
    {
        while (true)
        {
            yield return Fade(nextIcon, 1f, 0f, blinkSpeed);
            yield return Fade(nextIcon, 0f, 1f, blinkSpeed);
        }
    }

    private IEnumerator Fade(Image image, float from, float to, float duration)
    {
        float t = 0f;
        Color c = image.color;
        while (t < duration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(from, to, t / duration);
            image.color = c;
            yield return null;
        }
        c.a = to;
        image.color = c;
    }

    // ===========================
    // Completion
    // ===========================

    private void OnBlockFinished()
    {
        SetNextIconVisible(false);
        currentBlock = null;
        Debug.Log("[DialogueManager] Block finished.");
    }
}