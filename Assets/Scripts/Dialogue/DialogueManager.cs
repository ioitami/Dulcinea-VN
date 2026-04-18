using System;
using System.Collections;
using System.ComponentModel;
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

    [Header("Player Settings")]
    public bool GlobalAllowDialogueClick = true;

    [Header("Dialogue Info")]
    // Accessed by nodes directly
    public DialogueBlock currentBlock;
    public bool clickToContinueEnabled;


    // Internal state
    [SerializeField]
    public DialogueGroup currentGroup;
    [SerializeField]
    private int currentBlockIndex; // DialogueBlocks within a DialogueGroup

    private Action onBlockComplete;

    [SerializeField]
    private int currentNodeIndex; // DialogueNodes within a DialogueBlock
    [SerializeField]
    private bool isTyping;
    [SerializeField]
    private bool isWaitingForClick;
    private string lastTypedText;
    private Action pendingOnComplete;

    private Coroutine typingCoroutine;
    private Coroutine blinkCoroutine;
    private Coroutine fastForwardCoroutine;


    [SerializeField]
    public bool isFastForwarding = false;

    // ===========================
    // Public API
    // ===========================
    public bool savedGlobalAllowDialogueClickBool;
    public void SetGlobalAllowDialogueClick(bool allow)
    {
        savedGlobalAllowDialogueClickBool = GlobalAllowDialogueClick;
        GlobalAllowDialogueClick = allow;
    }

    public void RememberGlobalAllowDialogueClickBool()
    {
        GlobalAllowDialogueClick = savedGlobalAllowDialogueClickBool;
    }

    public void PlayGroup(DialogueGroup group)
    {
        if (group == null) return;

        currentGroup = group;
        currentBlockIndex = 0;

        PlayNextBlockInGroup();
    }

    private void PlayNextBlockInGroup()
    {
        if (currentGroup == null) return;

        if (currentBlockIndex >= currentGroup.blocks.Count)
        {
            OnGroupFinished();
            return;
        }

        DialogueBlock block = currentGroup.blocks[currentBlockIndex];
        currentBlockIndex++;

        if (block == null)
        {
            PlayNextBlockInGroup();
            return;
        }

        PlayBlock(block, PlayNextBlockInGroup);
    }



    public void PlayBlock(DialogueBlock block, Action onComplete = null)
    {
        if (block == null) return;

        currentBlock = block;
        currentNodeIndex = 0;
        isTyping = false;
        isWaitingForClick = false;
        clickToContinueEnabled = false;
        onBlockComplete = onComplete;

        if (currentBlock.textBox != null)
            currentBlock.textBox.text = "";

        SetNextIconVisible(false);
        ProcessNextNode();
    }

    public void PlaySpecificBlockInGroup(DialogueGroup group, DialogueBlock block = null)
    {
        if (group == null)
        {
            Debug.Log("No DialogueGroup detected");
            return;
        }

        if (block == null)
        {
            PlayGroup(group);
            return;
        }

        int index = group.blocks.IndexOf(block);

        if (index == -1)
        {
            Debug.LogWarning($"[DialogueManager] Block '{block.ID}' not found in group '{group.ID}'. Playing group from start.");
            PlayGroup(group);
            return;
        }

        currentGroup = group;
        currentBlockIndex = index;

        PlayNextBlockInGroup();
    }



    public void DialogueContinueClicked()
    {
        if (!GlobalAllowDialogueClick) return;

        if (isTyping)
        {
            SkipTyping();
            return;
        }

        if (isWaitingForClick)
        {
            isWaitingForClick = false;
            SetNextIconVisible(false);

            Action callback = pendingOnComplete;
            pendingOnComplete = null;
            callback?.Invoke();
        }
    }

    public void StartFastForward()
    {
        if (!GlobalAllowDialogueClick) return;
        if (isFastForwarding) return;

        Debug.Log("Starting FastForward");
        isFastForwarding = true;
        isWaitingForClick = false;


        if (isTyping)
        {
            SkipTyping();
        }
        else
        {
            OnNodeCompletedFastForward();
        }
    }

    public void StopFastForward()
    {
        Debug.Log("Stopping FastForward");
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

        // Do not advance if waiting for click is true and fast forwarding is disabled
        if (isFastForwarding == false && isWaitingForClick == true)
        {
            return;
        }

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

        if (isFastForwarding)
            node.Execute(this, OnNodeCompletedFastForward);
        else
            node.Execute(this, ProcessNextNode);
    }

    private void OnNodeCompletedFastForward()
    {
        if (!isFastForwarding)
        {
            ProcessNextNode();
            return;
        }

        if (fastForwardCoroutine != null) StopCoroutine(fastForwardCoroutine);

        fastForwardCoroutine = StartCoroutine(FastForwardDelayRoutine());
    }

    private IEnumerator FastForwardDelayRoutine()
    {
        yield return new WaitForSeconds(fastForwardDelay);

        if (isFastForwarding)
            ProcessNextNode();
    }

    // ===========================
    // Typing (called by DialogueTextNode)
    // ===========================

    public void StartTyping(string text, float speed, bool append, bool requireClick, Action onComplete)
    {
        Debug.Log("Currently Typing in Text Node... Group: " + currentGroup.name + ", DialogueBlock index: " + currentBlockIndex + ", DialogueNode index: " + currentNodeIndex);

        if (append && currentBlock.textBox != null)
            lastTypedText = currentBlock.textBox.text + text;
        else
            lastTypedText = text;


        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeRoutine(text, speed, append, requireClick, onComplete));

        if (isFastForwarding)
            SkipTyping();
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
        bool requireClick = clickToContinueEnabled;

        if (lastIndex >= 0 && lastIndex < currentBlock.nodes.Length)
            if (currentBlock.nodes[lastIndex] is DialogueTextNode tn)
                requireClick = tn.requirePlayerClickContinue || clickToContinueEnabled;

        // Re-create the onComplete that TypeRoutine would have called
        OnTextFinished(requireClick, ProcessNextNode);
    }

    private void OnTextFinished(bool requireClick, Action onComplete)
    {
        if(isFastForwarding)
        {
            // Don't wait for click
            isWaitingForClick = false;
            OnNodeCompletedFastForward();
            return;
        }

        if (requireClick || clickToContinueEnabled)
        {
            isWaitingForClick = true;
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
        Debug.Log("FastForwarding...");

        while (isFastForwarding)
        {
            Debug.Log("FastForwarding tick...");

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


        Action callback = onBlockComplete;
        onBlockComplete = null;
        callback?.Invoke();
    }

    private void OnGroupFinished()
    {
        currentGroup = null;
        Debug.Log("[DialogueManager] Group finished.");
    }

}