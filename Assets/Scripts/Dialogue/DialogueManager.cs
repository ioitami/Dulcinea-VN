using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class DialogueManager : MonoBehaviour
{
    [Header("UI Components")]
    public Image nextIconWindow1;
    public Image nextIconWindow2;
    public TextMeshProUGUI window1TextBox;
    public TextMeshProUGUI window2TextBox;

    [Header("Typing Settings")]
    public float typingSpeed = 0.03f;
    public float fastForwardDelay = 0.01f;

    [Header("Next Icon Blink Settings")]
    public float blinkSpeed = 0.5f;

    [Header("Player Settings")]
    public bool GlobalAllowDialogueClick = true;

    [Header("Server Info")]
    public bool requiresServer = false;
    public bool isMainServer = true;

    // The single always-present track driving normal linear play.
    private DialogueTrack primaryTrack;

    // Non-null only while a split is active.
    private DialogueTrack window1SplitTrack;
    private DialogueTrack window2SplitTrack;

    private bool linkedSplitContinue = false;
    private bool toggleBothWindowClicksAllow = false;
    private bool requireConvergenceClick = false;
    private bool awaitingSplitConvergenceClick = false;
    private DialogueGroup pendingConvergenceGroup;
    private DialogueBlock pendingConvergenceBlock;
    private Coroutine convergenceBlinkCoroutine1;
    private Coroutine convergenceBlinkCoroutine2;

    private Coroutine clientIconBlinkCoroutine1;
    private Coroutine clientIconBlinkCoroutine2;
    private Image clientActiveIcon;


    private class PendingSplitEnd
    {
        public string splitID;
        public DialogueGroup nextGroup;
        public DialogueBlock nextBlock;
    }

    private readonly List<PendingSplitEnd> pendingSplitEnds = new List<PendingSplitEnd>();

    // Set by DialogueTrack.ProcessNextNode for the duration of a node's
    // synchronous Execute call, so existing node classes reading
    // manager.currentBlock/isFastForwarding/etc. keep working unchanged.
    public DialogueTrack ActiveTrack { get; set; }

    private void Awake()
    {
        primaryTrack = new DialogueTrack(this, 0);
    }

    // ===========================
    // Backward-compatible surface for existing node classes
    // ===========================

    public DialogueBlock currentBlock => (ActiveTrack ?? primaryTrack).currentBlock;
    public DialogueGroup currentGroup => (ActiveTrack ?? primaryTrack).currentGroup;
    public int CurrentNodeIndex => (ActiveTrack ?? primaryTrack).currentNodeIndex;

    public bool isFastForwarding
    {
        get => (ActiveTrack ?? primaryTrack).isFastForwarding;
        set { (ActiveTrack ?? primaryTrack).isFastForwarding = value; }
    }

    public bool clickToContinueEnabled
    {
        get => (ActiveTrack ?? primaryTrack).clickToContinueEnabled;
        set { (ActiveTrack ?? primaryTrack).clickToContinueEnabled = value; }
    }

    public void StartTyping(string text, float speed, bool append, bool requireClick, Action onComplete)
    {
        (ActiveTrack ?? primaryTrack).StartTyping(text, speed, append, requireClick, onComplete);
    }

    public void RegisterActiveChoice(DialogueChoiceNode node, Action onComplete)
    {
        DialogueTrack track = ActiveTrack ?? primaryTrack;
        track.activeChoiceNode = node;
        track.activeChoiceOnComplete = onComplete;
    }


    public void SetNextIconVisibleLocally(int windowNumber, bool visible)
    {
        Image icon = windowNumber == 1 ? nextIconWindow1 : nextIconWindow2;
        if (icon == null) return;

        if (visible)
        {
            icon.gameObject.SetActive(true);

            if (windowNumber == 1)
            {
                if (clientIconBlinkCoroutine1 != null) StopCoroutine(clientIconBlinkCoroutine1);
                clientIconBlinkCoroutine1 = RunBlink(icon);
            }
            else
            {
                if (clientIconBlinkCoroutine2 != null) StopCoroutine(clientIconBlinkCoroutine2);
                clientIconBlinkCoroutine2 = RunBlink(icon);
            }
        }
        else
        {
            icon.gameObject.SetActive(false);

            if (windowNumber == 1)
            {
                if (clientIconBlinkCoroutine1 != null)
                {
                    StopCoroutine(clientIconBlinkCoroutine1);
                    clientIconBlinkCoroutine1 = null;
                }
            }
            else
            {
                if (clientIconBlinkCoroutine2 != null)
                {
                    StopCoroutine(clientIconBlinkCoroutine2);
                    clientIconBlinkCoroutine2 = null;
                }
            }
        }
    }

    public Coroutine RunBlink(Image icon)
    {
        return StartCoroutine(BlinkIconRoutine(icon));
    }

    private IEnumerator BlinkIconRoutine(Image icon)
    {
        while (true)
        {
            yield return Fade(icon, 1f, 0f, blinkSpeed);
            yield return Fade(icon, 0f, 1f, blinkSpeed);
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
    // Track lookup
    // ===========================

    // Returns the split track for that window if a split is active,
    // otherwise the primary track regardless of which window is asking.
    public DialogueTrack GetTrack(int windowNumber)
    {
        if (window1SplitTrack != null && windowNumber == 1) return window1SplitTrack;
        if (window2SplitTrack != null && windowNumber == 2) return window2SplitTrack;
        return primaryTrack;
    }

    public bool IsSplitActive => window1SplitTrack != null && window2SplitTrack != null;
    public DialogueGroup SplitWindow1Group => window1SplitTrack?.currentGroup;
    public DialogueBlock SplitWindow1Block => window1SplitTrack?.currentBlock;
    public DialogueGroup SplitWindow2Group => window2SplitTrack?.currentGroup;
    public DialogueBlock SplitWindow2Block => window2SplitTrack?.currentBlock;
    public bool LinkedSplitContinue => linkedSplitContinue;
    public bool ToggleBothWindowClicksAllow => toggleBothWindowClicksAllow;
    public bool AwaitingSplitConvergenceClick => awaitingSplitConvergenceClick;
    public DialogueGroup PendingConvergenceGroup => pendingConvergenceGroup;
    public DialogueBlock PendingConvergenceBlock => pendingConvergenceBlock;

    public void SetLinkedSplitContinue(bool value)
    {
        linkedSplitContinue = value;
    }

    public void SetToggleBothWindowClicksAllow(bool value)
    {
        toggleBothWindowClicksAllow = value;
    }

    // ===========================
    // Networking authority
    // ===========================

    public bool CanDriveDialogueLocally()
    {
        return NetworkServer.active || !NetworkClient.active;
    }

    public void RequestContinue(int windowNumber)
    {
        if (CanDriveDialogueLocally())
            ContinueClicked(windowNumber);
        else
            NVLNetworkPlayer.localPlayer?.CmdRequestContinue(windowNumber);
    }

    public void ContinueClicked(int windowNumber)
    {
        if (!GlobalAllowDialogueClick) return;

        if (awaitingSplitConvergenceClick)
        {
            awaitingSplitConvergenceClick = false;
            ShowConvergenceIcons(false);

            DialogueGroup group = pendingConvergenceGroup;
            DialogueBlock block = pendingConvergenceBlock;
            pendingConvergenceGroup = null;
            pendingConvergenceBlock = null;

            primaryTrack.PlaySpecificBlockInGroup(group, block);
            return;
        }

        if (window1SplitTrack != null && window2SplitTrack != null && linkedSplitContinue)
        {
            window1SplitTrack.HandleContinueClick();
            window2SplitTrack.HandleContinueClick();
            return;
        }

        DialogueTrack track = GetTrack(windowNumber);
        bool isPrimaryTrack = track == primaryTrack;

        if (!(isPrimaryTrack && toggleBothWindowClicksAllow) && !IsClickFromMatchingWindow(track, windowNumber))
        {
            return;
        }

        track.HandleContinueClick();
    }

    private bool IsClickFromMatchingWindow(DialogueTrack track, int windowNumber)
    {
        if (track.currentBlock == null) return true;

        DialogueBlockWindow expectedWindow = windowNumber == 1 ? DialogueBlockWindow.AVL : DialogueBlockWindow.NVL;

        return track.currentBlock.window == expectedWindow;
    }

    public void RequestStartFastForward(int windowNumber)
    {
        if (CanDriveDialogueLocally())
            StartFastForward(windowNumber);
        else
            NVLNetworkPlayer.localPlayer?.CmdRequestStartFastForward(windowNumber);
    }

    public void RequestStopFastForward(int windowNumber)
    {
        if (CanDriveDialogueLocally())
            StopFastForward(windowNumber);
        else
            NVLNetworkPlayer.localPlayer?.CmdRequestStopFastForward(windowNumber);
    }

    public void StartFastForward(int windowNumber)
    {
        if (!GlobalAllowDialogueClick) return;
        GetTrack(windowNumber).StartFastForward();
    }

    public void StopFastForward(int windowNumber)
    {
        GetTrack(windowNumber).StopFastForward();
    }

    public void StopFastForward()
    {
        primaryTrack.StopFastForward();
        window1SplitTrack?.StopFastForward();
        window2SplitTrack?.StopFastForward();
    }

    public void SetRequiresServer(bool value)
    {
        if (!CanDriveDialogueLocally()) return;

        requiresServer = value;

        if (NetworkServer.active)
            NVLNetworkPlayer.hostInstance?.SetRequiresServer(value);
    }

    public void BeginWindowCloseChoice(string groupIfHostCloses, string blockIfHostCloses, string groupIfClientCloses, string blockIfClientCloses)
    {
        if (!NetworkServer.active) return;

        NVLNetworkPlayer.hostInstance?.SetAwaitingWindowCloseChoice(true, groupIfHostCloses, blockIfHostCloses, groupIfClientCloses, blockIfClientCloses);
    }

    public void StopAllDialogueActivity()
    {
        primaryTrack.StopAllActivity();
        window1SplitTrack?.StopAllActivity();
        window2SplitTrack?.StopAllActivity();

        window1SplitTrack = null;
        window2SplitTrack = null;
        pendingSplitEnds.Clear();
        linkedSplitContinue = false;
        toggleBothWindowClicksAllow = false;
        requireConvergenceClick = false;
        awaitingSplitConvergenceClick = false;
        pendingConvergenceGroup = null;
        pendingConvergenceBlock = null;
        ShowConvergenceIcons(false);
    }

    // ===========================
    // Split tracks
    // ===========================

    public void BeginSplit(DialogueGroup group1, DialogueBlock block1, DialogueGroup group2, DialogueBlock block2, bool requireClickToConverge = false)
    {
        if (!CanDriveDialogueLocally()) return;

        if (window1SplitTrack != null || window2SplitTrack != null)
        {
            Debug.LogWarning("[DialogueManager] A split is already active; ignoring nested SplitPlayGroupNode.");
            return;
        }

        if (primaryTrack.currentBlock != null)
        {
            GameSingleton.instance.gameStateManager.RegisterVisitedBlock(primaryTrack.currentBlock.ID);
        }

        requireConvergenceClick = requireClickToConverge;

        window1SplitTrack = new DialogueTrack(this, 1);
        window2SplitTrack = new DialogueTrack(this, 2);

        window1SplitTrack.PlaySpecificBlockInGroup(group1, block1);
        window2SplitTrack.PlaySpecificBlockInGroup(group2, block2);
    }

    public void ReportSplitEnd(string splitID, DialogueGroup nextGroup, DialogueBlock nextBlock)
    {
        if (!CanDriveDialogueLocally()) return;

        pendingSplitEnds.Add(new PendingSplitEnd { splitID = splitID, nextGroup = nextGroup, nextBlock = nextBlock });

        if (pendingSplitEnds.Count < 2) return;

        if (pendingSplitEnds[0].splitID != pendingSplitEnds[1].splitID)
        {
            Debug.LogWarning($"[DialogueManager] EndSplitGroupNode ID mismatch: '{pendingSplitEnds[0].splitID}' vs '{pendingSplitEnds[1].splitID}'.");
        }

        // Whichever node finishes first will decide and resolve what the next node group/block will be
        PendingSplitEnd resolved = pendingSplitEnds[0];
        pendingSplitEnds.Clear();

        window1SplitTrack = null;
        window2SplitTrack = null;
        linkedSplitContinue = false;

        if (requireConvergenceClick)
        {
            BeginSplitConvergenceWait(resolved.nextGroup, resolved.nextBlock);
        }
        else
        {
            primaryTrack.PlaySpecificBlockInGroup(resolved.nextGroup, resolved.nextBlock);
        }
    }

    public void BeginSplitConvergenceWait(DialogueGroup nextGroup, DialogueBlock nextBlock)
    {
        awaitingSplitConvergenceClick = true;
        pendingConvergenceGroup = nextGroup;
        pendingConvergenceBlock = nextBlock;

        ShowConvergenceIcons(true);
    }

    private void ShowConvergenceIcons(bool visible)
    {
        if (nextIconWindow1 != null)
        {
            nextIconWindow1.gameObject.SetActive(visible);

            if (visible)
            {
                if (convergenceBlinkCoroutine1 != null) StopCoroutine(convergenceBlinkCoroutine1);
                convergenceBlinkCoroutine1 = RunBlink(nextIconWindow1);
            }
            else if (convergenceBlinkCoroutine1 != null)
            {
                StopCoroutine(convergenceBlinkCoroutine1);
                convergenceBlinkCoroutine1 = null;
            }
        }

        if (nextIconWindow2 != null)
        {
            nextIconWindow2.gameObject.SetActive(visible);

            if (visible)
            {
                if (convergenceBlinkCoroutine2 != null) StopCoroutine(convergenceBlinkCoroutine2);
                convergenceBlinkCoroutine2 = RunBlink(nextIconWindow2);
            }
            else if (convergenceBlinkCoroutine2 != null)
            {
                StopCoroutine(convergenceBlinkCoroutine2);
                convergenceBlinkCoroutine2 = null;
            }
        }
    }

    // Forced convergence between the split
    public void ForceEndSplit(DialogueGroup nextGroup, DialogueBlock nextBlock)
    {
        if (!CanDriveDialogueLocally()) return;
        if (window1SplitTrack == null && window2SplitTrack == null) return;

        pendingSplitEnds.Clear();
        window1SplitTrack = null;
        window2SplitTrack = null;
        linkedSplitContinue = false;

        primaryTrack.PlaySpecificBlockInGroup(nextGroup, nextBlock);
    }

    private bool previousGlobalAllowDialogueClick = true;

    public void SetGlobalAllowDialogueClick(bool allow)
    {
        previousGlobalAllowDialogueClick = GlobalAllowDialogueClick;
        GlobalAllowDialogueClick = allow;
    }

    public void RememberGlobalAllowDialogueClickBool()
    {
        GlobalAllowDialogueClick = previousGlobalAllowDialogueClick;
    }

    // ===========================
    // Networked choice tracking
    // ===========================

    public void ShowChoiceUILocally(int windowNumber, string blockID, int nodeIndex)
    {
        if (!DialogueLookup.TryFindBlockByID(blockID, out DialogueBlock block)) return;
        if (nodeIndex < 0 || nodeIndex >= block.nodes.Length) return;

        if (block.nodes[nodeIndex] is DialogueChoiceNode choiceNode)
        {
            GetTrack(windowNumber).activeChoiceNode = choiceNode;
            choiceNode.DisplayChoicesLocally(this, windowNumber, blockID, nodeIndex);
        }
    }

    public void HideChoiceUILocally(int windowNumber)
    {
        DialogueTrack track = GetTrack(windowNumber);
        track.activeChoiceNode?.CleanupChoicesLocally();
        track.activeChoiceNode = null;
        track.activeChoiceOnComplete = null;
    }

    public void ResolveActiveChoiceByIndex(int windowNumber, int choiceIndex)
    {
        if (!CanDriveDialogueLocally()) return;

        DialogueTrack track = GetTrack(windowNumber);
        track.activeChoiceNode?.ResolveChoice(choiceIndex, this, track, track.activeChoiceOnComplete);
    }

    // ===========================
    // Entry points
    // ===========================

    public void PlayGroup(DialogueGroup group)
    {
        if (!CanDriveDialogueLocally()) return;
        if (group == null) return;

        primaryTrack.PlayGroup(group);
    }

    public void PlayBlock(DialogueBlock block, Action onComplete = null)
    {
        if (!CanDriveDialogueLocally()) return;
        if (block == null) return;

        (ActiveTrack ?? primaryTrack).PlayBlock(block, onComplete);
    }

    public void PlaySpecificBlockInGroup(DialogueGroup group, DialogueBlock block = null)
    {
        if (!CanDriveDialogueLocally()) return;

        (ActiveTrack ?? primaryTrack).PlaySpecificBlockInGroup(group, block);
    }
}