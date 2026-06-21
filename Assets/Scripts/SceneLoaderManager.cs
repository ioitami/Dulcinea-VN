
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

        GameSingleton.instance.cameraManager.DisableAllOverlayCanvas();

        GameSingleton.instance.cameraManager.EnableOverlayCanvas((int)ScreenID.MainMenu);
        GameSingleton.instance.cameraManager.MoveCameraToLocation((int)MainCameraID.Window1, (int)MainCameraLocations.MainMenu);


        LoadBaseCharacterLayerScreen();

        // Display Dulci here? Or maybe full BG art

    }
    public void LoadWindow1()
    {
        GameSingleton.instance.cameraManager.DisableAllOverlayCanvas();

        GameSingleton.instance.cameraManager.EnableMainCamera((int)MainCameraID.Window1);
        ToggleAVL(true);
        GameSingleton.instance.cameraManager.MoveCameraToLocation((int)MainCameraID.Window1, (int)MainCameraLocations.Window1);


        LoadBaseCharacterLayerScreen();
    }



    // OVERLAY CANVAS CONTROLS

    public void LoadBaseCharacterLayerScreen()
    {
        GameSingleton.instance.cameraManager.EnableOverlayCanvas((int)ScreenID.CharacterScreen);

        GameSingleton.instance.characterManager.HideAllCharacters();
    }

    public void LoadCurrentCharacterLayerScreen()
    {
        GameSingleton.instance.cameraManager.EnableOverlayCanvas((int)ScreenID.CharacterScreen);
    }

    public void LoadLoadMenu()
    {
        GameSingleton.instance.cameraManager.EnableOverlayCanvas((int)ScreenID.SaveLoadOptionsMenu);

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

        GameSingleton.instance.cameraManager.EnableOverlayCanvas((int)ScreenID.SaveLoadOptionsMenu);

        uiController.saveLoadMenu.saveMenu.gameObject.SetActive(true);
        uiController.saveLoadMenu.loadMenu.gameObject.SetActive(false);

        GameSingleton.instance.dialogueManager.StopFastForward();
        GameSingleton.instance.dialogueManager.SetGlobalAllowDialogueClick(false);

        // UI Transition Animation here

        // =====

    }

    public void LoadPreferencesOptionsMenu()
    {
        GameSingleton.instance.cameraManager.EnableOverlayCanvas((int)ScreenID.PreferencesOptionsMenu);


        // UI Transition Animation here

        // =====
    }

    public void ClosePreferencesOptionsMenu()
    {
        // UI Transition Animation here

        // =====

        GameSingleton.instance.cameraManager.DisableOverlayCanvas((int)ScreenID.PreferencesOptionsMenu);
    }

    public void CloseSaveLoadOptionsMenu()
    {
        // UI Transition Animation here

        // =====


        GameSingleton.instance.cameraManager.DisableOverlayCanvas((int)ScreenID.SaveLoadOptionsMenu);

        GameSingleton.instance.dialogueManager.RememberGlobalAllowDialogueClickBool();
    }

    public void LoadDialogueLogHistory()
    {

        GameSingleton.instance.cameraManager.EnableOverlayCanvas((int)ScreenID.DialogueLogHistory);

        GameSingleton.instance.dialogueManager.StopFastForward();
        GameSingleton.instance.dialogueManager.SetGlobalAllowDialogueClick(false);
        // UI Transition Animation here

        // =====
    }

    public void CloseDialogueLogHistory()
    {
        GameSingleton.instance.cameraManager.DisableOverlayCanvas((int)ScreenID.DialogueLogHistory);

        GameSingleton.instance.dialogueManager.RememberGlobalAllowDialogueClickBool();
        // UI Transition Animation here

        // =====
    }

    public void ToggleAVL(bool toggle)
    {
        if (toggle == true)
        {
            GameSingleton.instance.cameraManager.EnableOverlayCanvas((int)ScreenID.AVL);
        }
        else
        {
            GameSingleton.instance.cameraManager.DisableOverlayCanvas((int)ScreenID.AVL);
        }

    }



}
