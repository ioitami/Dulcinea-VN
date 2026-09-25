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

        uiController.optionsMenu.saveLoadDisplayMenu.RefreshAllSaveLoadSlots();
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

    public void LoadWindows()
    {
        uiController.DisableAllScreens();

        uiController.EnableScreen("Window1");
        uiController.EnableScreen("Window2");

        //GameSingleton.instance.cameraManager.MoveCameraToLocation((int)MainCameraID.Window1, (int)MainCameraLocations.Window1);
       // GameSingleton.instance.cameraManager.MoveCameraToLocation((int)MainCameraID.Window2, (int)MainCameraLocations.Window2);

        ToggleAVL(true);
        ToggleNVL(true);
    }

    // OVERLAY CANVAS CONTROLS

    public void LoadLoadMenu()
    {
        uiController.EnableScreen("OptionsMenu");
        uiController.EnableScreen("SaveLoadDisplayMenu");
        uiController.optionsMenu.saveLoadDisplayMenu.OpenLastVisitedLoadPage();

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

        uiController.EnableScreen("OptionsMenu");
        uiController.EnableScreen("SaveLoadDisplayMenu");
        uiController.optionsMenu.saveLoadDisplayMenu.OpenLastVisitedSavePage();

        GameSingleton.instance.dialogueManager.StopFastForward();
        GameSingleton.instance.dialogueManager.SetGlobalAllowDialogueClick(false);

        // UI Transition Animation here

        // =====
    }

    public void LoadPreferencesOptionsMenu()
    {
        uiController.EnableScreen("OptionsMenu");
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

        uiController.DisableScreen("SaveLoadDisplayMenu");

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