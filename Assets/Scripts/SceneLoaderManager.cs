using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class SceneLoaderManager : MonoBehaviour
{
    [Header("UI")]
    public UIController uiController;

    private void Start()
    {
        if (uiController == null)
        {
            uiController = FindAnyObjectByType<UIController>();
        }

        uiController.saveLoadDisplayMenu.RefreshAllSaveLoadSlots();
        uiController.optionsMenu.optionsTab.SetOptionsTabButtonListeners();

        LoadMainMenu();
    }

    // TODO: SCENE TRANSITIONS WILL GO HERE (TRANSITION FUNCTIONS SOMEWHERE ELSE)
    public void LoadMainMenu()
    {
        // TODO: ADD SCENE TRANSITION HERE

        //==============

        uiController.DisableAllScreens();

        uiController.EnableScreen("MainMenu");
        GameSingleton.instance.cameraManager.MoveCameraToLocation((int)MainCameraID.Window1, (int)MainCameraLocations.MainMenu);

        // Display Dulci here? Or maybe full BG art
    }

    public void LoadWindow1()
    {
        uiController.DisableAllScreens();

        GameSingleton.instance.cameraManager.EnableMainCamera((int)MainCameraID.Window1);
        ToggleAVL(true);
        ToggleNVL(true);
        GameSingleton.instance.cameraManager.MoveCameraToLocation((int)MainCameraID.Window1, (int)MainCameraLocations.Window1);
    }

    // OVERLAY CANVAS CONTROLS

    public void LoadLoadMenu()
    {
        uiController.EnableScreen("SaveLoadOptionsMenu");

        uiController.optionsMenu.gameObject.SetActive(true);
        uiController.saveLoadDisplayMenu.gameObject.SetActive(true);
        uiController.saveLoadDisplayMenu.OpenLastVisitedLoadPage();

        GameSingleton.instance.dialogueManager.StopFastForward();
        GameSingleton.instance.dialogueManager.SetGlobalAllowDialogueClick(false);

        // UI Transition Animation here

        // =====
    }

    public void LoadSaveMenu()
    {
        StartCoroutine(LoadSaveMenuRoutine());
    }

    private IEnumerator LoadSaveMenuRoutine()
    {
        // Capture screenshot first before opening save menu
        yield return StartCoroutine(
            GameSingleton.instance.gameStateManager.CaptureScreenshotRoutine()
        );

        uiController.EnableScreen("SaveLoadOptionsMenu");

        uiController.optionsMenu.gameObject.SetActive(true);
        uiController.saveLoadDisplayMenu.gameObject.SetActive(true);
        uiController.saveLoadDisplayMenu.OpenLastVisitedSavePage();

        GameSingleton.instance.dialogueManager.StopFastForward();
        GameSingleton.instance.dialogueManager.SetGlobalAllowDialogueClick(false);

        // UI Transition Animation here

        // =====
    }

    public void LoadPreferencesOptionsMenu()
    {
        uiController.EnableScreen("PreferencesOptionsMenu");

        GameSingleton.instance.dialogueManager.StopFastForward();
        GameSingleton.instance.dialogueManager.SetGlobalAllowDialogueClick(false);

        // UI Transition Animation here

        // =====
    }

    public void ClosePreferencesOptionsMenu()
    {
        // UI Transition Animation here

        // =====

        uiController.DisableScreen("PreferencesOptionsMenu");

        GameSingleton.instance.dialogueManager.RememberGlobalAllowDialogueClickBool();
    }

    public void CloseSaveLoadOptionsMenu()
    {
        // UI Transition Animation here

        // =====

        uiController.DisableScreen("SaveLoadOptionsMenu");

        GameSingleton.instance.dialogueManager.RememberGlobalAllowDialogueClickBool();
    }

    public void ResetAVLChoiceContainer()
    {
        foreach (Transform obj in uiController.avl.avlChoiceContainer)
        {
            Destroy(obj.gameObject);
        }
    }

    public void LoadDialogueLogHistory()
    {
        uiController.EnableScreen("DialogueLogHistory");

        GameSingleton.instance.dialogueManager.StopFastForward();
        GameSingleton.instance.dialogueManager.SetGlobalAllowDialogueClick(false);
        // UI Transition Animation here

        // =====
    }

    public void CloseDialogueLogHistory()
    {
        uiController.DisableScreen("DialogueLogHistory");

        GameSingleton.instance.dialogueManager.RememberGlobalAllowDialogueClickBool();
        // UI Transition Animation here

        // =====
    }

    public void ToggleAVL(bool toggle)
    {
        if (toggle == true)
        {
            uiController.EnableScreen("AVL");
        }
        else
        {
            uiController.DisableScreen("AVL");
        }
    }

    public void ToggleNVL(bool toggle)
    {
        if (toggle == true)
        {
            uiController.EnableScreen("NVL");
        }
        else
        {
            uiController.DisableScreen("NVL");
        }
    }
}