
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneLoaderManager : MonoBehaviour
{

    //public List<Canvas> currentActiveUI;

    [Header("UI")]
    public UIController uiController;

    private void Start()
    {
        if(uiController == null)
        {
            uiController = FindAnyObjectByType<UIController>();
        }

        LoadMainMenu();
    }

    // TODO: SCENE TRANSITIONS WILL GO HERE (TRANSITION FUNCTIONS SOMEWHERE ELSE)
    public void LoadMainMenu()
    {
        // TODO: ADD SCENE TRANSITION HERE

        //==============

        GameSingleton.instance.cameraManager.DisableAllOverlay();

        GameSingleton.instance.cameraManager.EnableOverlay((int)ScreenID.MainMenu);
        GameSingleton.instance.cameraManager.MoveCameraToLocation((int)MainCameraID.Window1, (int)MainCameraLocations.MainMenu);


        // Display Dulci here? Or maybe full BG art

    }
    public void LoadWindow1()
    {
        GameSingleton.instance.cameraManager.DisableAllOverlay();

        GameSingleton.instance.cameraManager.EnableMainCamera((int)MainCameraID.Window1);
        ToggleAVL(true);
        ToggleNVL(true);
        GameSingleton.instance.cameraManager.MoveCameraToLocation((int)MainCameraID.Window1, (int)MainCameraLocations.Window1);

    }



    // OVERLAY CANVAS CONTROLS

    public void LoadLoadMenu()
    {
        GameSingleton.instance.cameraManager.EnableOverlay((int)ScreenID.SaveLoadOptionsMenu);

        uiController.saveLoadMenu.saveMenu.gameObject.SetActive(false);
        uiController.saveLoadMenu.loadMenu.gameObject.SetActive(true);

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

        GameSingleton.instance.cameraManager.EnableOverlay((int)ScreenID.SaveLoadOptionsMenu);

        uiController.saveLoadMenu.saveMenu.gameObject.SetActive(true);
        uiController.saveLoadMenu.loadMenu.gameObject.SetActive(false);

        GameSingleton.instance.dialogueManager.StopFastForward();
        GameSingleton.instance.dialogueManager.SetGlobalAllowDialogueClick(false);

        // UI Transition Animation here

        // =====

    }

    public void LoadPreferencesOptionsMenu()
    {
        GameSingleton.instance.cameraManager.EnableOverlay((int)ScreenID.PreferencesOptionsMenu);

        GameSingleton.instance.dialogueManager.StopFastForward();
        GameSingleton.instance.dialogueManager.SetGlobalAllowDialogueClick(false);

        // UI Transition Animation here

        // =====
    }

    public void ClosePreferencesOptionsMenu()
    {
        // UI Transition Animation here

        // =====

        GameSingleton.instance.cameraManager.DisableOverlay((int)ScreenID.PreferencesOptionsMenu);

        GameSingleton.instance.dialogueManager.RememberGlobalAllowDialogueClickBool();
    }

    public void CloseSaveLoadOptionsMenu()
    {
        // UI Transition Animation here

        // =====


        GameSingleton.instance.cameraManager.DisableOverlay((int)ScreenID.SaveLoadOptionsMenu);

        GameSingleton.instance.dialogueManager.RememberGlobalAllowDialogueClickBool();
    }

    public void LoadDialogueLogHistory()
    {

        GameSingleton.instance.cameraManager.EnableOverlay((int)ScreenID.DialogueLogHistory);

        GameSingleton.instance.dialogueManager.StopFastForward();
        GameSingleton.instance.dialogueManager.SetGlobalAllowDialogueClick(false);
        // UI Transition Animation here

        // =====
    }

    public void CloseDialogueLogHistory()
    {
        GameSingleton.instance.cameraManager.DisableOverlay((int)ScreenID.DialogueLogHistory);

        GameSingleton.instance.dialogueManager.RememberGlobalAllowDialogueClickBool();
        // UI Transition Animation here

        // =====
    }

    public void ToggleAVL(bool toggle)
    {
        if (toggle == true)
        {
            GameSingleton.instance.cameraManager.EnableOverlay((int)ScreenID.AVL);
        }
        else
        {
            GameSingleton.instance.cameraManager.DisableOverlay((int)ScreenID.AVL);
        }

    }

    public void ToggleNVL(bool toggle)
    {
        if (toggle == true)
        {
            GameSingleton.instance.cameraManager.EnableOverlay((int)ScreenID.NVL);
        }
        else
        {
            GameSingleton.instance.cameraManager.DisableOverlay((int)ScreenID.NVL);
        }

    }

}
