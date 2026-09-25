using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueTrack
{
    // 0 for the primary track (not bound to a specific window)
    // 1 or 2 for split track

    public readonly int windowNumber;
    private readonly DialogueManager manager;

    public DialogueGroup currentGroup;
    public DialogueBlock currentBlock;
    public int currentBlockIndex;
    public int currentNodeIndex;
    public bool isTyping;
    public bool isWaitingForClick;
    public bool isFastForwarding;
    public bool clickToContinueEnabled;
    public string lastTypedText;

    public Action onBlockComplete;
    public Action pendingOnComplete;
    public Coroutine typingCoroutine;
    public Coroutine fastForwardCoroutine;
    public Coroutine blinkCoroutine;
    private Image activeIcon;

    public DialogueChoiceNode activeChoiceNode;
    public Action activeChoiceOnComplete;

    public DialogueTrack(DialogueManager manager, int windowNumber)
    {
        this.manager = manager;
        this.windowNumber = windowNumber;
    }

    public void PlayGroup(DialogueGroup group)
    {
        currentGroup = group;
        currentBlockIndex = 0;

        PlayNextBlockInGroup();
    }

    public void PlaySpecificBlockInGroup(DialogueGroup group, DialogueBlock block)
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
            Debug.LogWarning($"[DialogueTrack] Block '{block.ID}' not found in group '{group.ID}'. Playing group from start.");
            PlayGroup(group);
            return;
        }

        currentGroup = group;
        currentBlockIndex = index;

        PlayNextBlockInGroup();
    }

    private void PlayNextBlockInGroup()
    {
        if (currentGroup == null) return;

        if (currentBlockIndex >= currentGroup.blocks.Count)
        {
            currentGroup = null;
            return;
        }

        DialogueBlock block = currentGroup.blocks[currentBlockIndex];
        currentBlockIndex++;

        if (block == null)
        {
            PlayNextBlockInGroup();
            return;
        }

        if (isFastForwarding && !GameSingleton.instance.gameStateManager.HasVisitedBlock(block.ID))
        {
            StopFastForward();
            Debug.Log($"[DialogueTrack] Fast forward stopped. Block '{block.ID}' not yet visited.");
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

        GameSingleton.instance.gameStateManager.CaptureBlockStartCharacterSnapshot();

        if (currentBlock.textBox != null)
        {
            currentBlock.textBox.text = "";
        }

        SetNextIconVisible(false);
        ProcessNextNode();
    }

    public void ProcessNextNode()
    {
        if (currentBlock == null)
        {
            Debug.Log("[DialogueTrack] No current block to process.");
            return;
        }

        if (isFastForwarding == false && isWaitingForClick == true) return;

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

        // Valid only for the duration of this synchronous call
        DialogueTrack previousActive = manager.ActiveTrack;
        manager.ActiveTrack = this;

        if (isFastForwarding)
        {
            node.Execute(manager, OnNodeCompletedFastForward);
        }
        else
        {
            node.Execute(manager, ProcessNextNode);
        }

        manager.ActiveTrack = previousActive;
    }

    private void OnNodeCompletedFastForward()
    {
        if (!isFastForwarding)
        {
            ProcessNextNode();
            return;
        }

        if (fastForwardCoroutine != null) manager.StopCoroutine(fastForwardCoroutine);

        fastForwardCoroutine = manager.StartCoroutine(FastForwardDelayRoutine());
    }

    private IEnumerator FastForwardDelayRoutine()
    {
        yield return new WaitForSeconds(manager.fastForwardDelay);

        if (isFastForwarding)
        {
            ProcessNextNode();
        }
    }

    public void StartFastForward()
    {
        if (isFastForwarding) return;

        if (currentBlock != null && !GameSingleton.instance.gameStateManager.HasVisitedBlock(currentBlock.ID))
        {
            Debug.Log("[DialogueTrack] Block not yet visited, fast forward blocked.");
            return;
        }

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
        isFastForwarding = false;

        if (fastForwardCoroutine != null)
        {
            manager.StopCoroutine(fastForwardCoroutine);
            fastForwardCoroutine = null;
        }
    }

    public void StartTyping(string text, float speed, bool append, bool requireClick, Action onComplete)
    {
        if (append && currentBlock.textBox != null)
        {
            lastTypedText = currentBlock.textBox.text + text;
        }
        else
        {
            lastTypedText = text;
        }

        if (typingCoroutine != null) manager.StopCoroutine(typingCoroutine);
        typingCoroutine = manager.StartCoroutine(TypeRoutine(text, speed, append, requireClick, onComplete));

        if (isFastForwarding)
        {
            SkipTyping();
        }
    }

    private IEnumerator TypeRoutine(string text, float speed, bool append, bool requireClick, Action onComplete)
    {
        isTyping = true;

        if (currentBlock.textBox != null && !append)
        {
            currentBlock.textBox.text = "";
        }

        int i = 0;
        while (i < text.Length)
        {
            if (text[i] == '<')
            {
                int closeIndex = text.IndexOf('>', i);
                if (closeIndex != -1)
                {
                    string tag = text.Substring(i, closeIndex - i + 1);
                    if (currentBlock.textBox != null)
                    {
                        currentBlock.textBox.text += tag;
                    }
                    i = closeIndex + 1;
                    continue;
                }
            }

            if (currentBlock.textBox != null)
            {
                currentBlock.textBox.text += text[i];
            }

            i++;
            yield return new WaitForSeconds(speed);
        }

        isTyping = false;
        OnTextFinished(requireClick, onComplete);
    }

    public void SkipTyping()
    {
        if (typingCoroutine != null)
        {
            manager.StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        if (currentBlock.textBox != null)
        {
            currentBlock.textBox.text = lastTypedText;
        }

        isTyping = false;

        int lastIndex = currentNodeIndex - 1;
        bool requireClick = clickToContinueEnabled;

        if (lastIndex >= 0 && lastIndex < currentBlock.nodes.Length)
        {
            if (currentBlock.nodes[lastIndex] is DialogueTextNode tn)
            {
                requireClick = tn.requirePlayerClickContinue || clickToContinueEnabled;
            }
        }

        OnTextFinished(requireClick, ProcessNextNode);
    }

    private void OnTextFinished(bool requireClick, Action onComplete)
    {
        if (isFastForwarding)
        {
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

    public void HandleContinueClick()
    {
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

    private void OnBlockFinished()
    {
        SetNextIconVisible(false);

        if (currentBlock != null)
        {
            GameSingleton.instance.gameStateManager.RegisterVisitedBlock(currentBlock.ID);
        }

        currentBlock = null;

        Action callback = onBlockComplete;
        onBlockComplete = null;
        callback?.Invoke();
    }

    public void WaitForClick(Action onComplete)
    {
        if (isFastForwarding)
        {
            onComplete?.Invoke();
            return;
        }

        isWaitingForClick = true;
        pendingOnComplete = onComplete;
        SetNextIconVisible(true);
    }

    public void StopAllActivity()
    {
        if (typingCoroutine != null)
        {
            manager.StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        if (fastForwardCoroutine != null)
        {
            manager.StopCoroutine(fastForwardCoroutine);
            fastForwardCoroutine = null;
        }

        if (blinkCoroutine != null)
        {
            manager.StopCoroutine(blinkCoroutine);
            blinkCoroutine = null;
        }

        isTyping = false;
        isWaitingForClick = false;
        isFastForwarding = false;
    }

    private Image ResolveNextIcon()
    {
        if (currentBlock == null) return null;

        return currentBlock.window == DialogueBlockWindow.AVL ? manager.nextIconWindow1 : manager.nextIconWindow2;
    }

    private void SetNextIconVisible(bool visible)
    {
        if (visible)
        {
            Image icon = ResolveNextIcon();
            if (icon == null) return;

            activeIcon = icon;
            icon.gameObject.SetActive(true);

            if (blinkCoroutine != null) manager.StopCoroutine(blinkCoroutine);
            blinkCoroutine = manager.RunBlink(icon);
        }
        else
        {
            if (activeIcon != null)
            {
                activeIcon.gameObject.SetActive(false);
                activeIcon = null;
            }

            if (blinkCoroutine != null)
            {
                manager.StopCoroutine(blinkCoroutine);
                blinkCoroutine = null;
            }
        }
    }
}