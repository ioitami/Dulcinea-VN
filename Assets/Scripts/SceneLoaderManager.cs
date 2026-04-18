
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

        GameSingleton.instance.cameraManager.EnableMainCamera((int)MainCameraID.MainMenu);

        LoadBaseCharacterLayerScreen();

        // Display Dulci here? Or maybe full BG art

    }
    public void LoadWindow1()
    {
        GameSingleton.instance.cameraManager.DisableAllOverlayCanvas();

        GameSingleton.instance.cameraManager.EnableMainCamera((int)MainCameraID.Window1);

        GameSingleton.instance.cameraManager.EnableOverlayCamera((int)OverlayCameraID.AVL);
        GameSingleton.instance.cameraManager.EnableOverlayCanvas((int)OverlayCameraID.AVL);

        LoadBaseCharacterLayerScreen();
    }



    // OVERLAY CAMERA CONTROLS

    public void LoadBaseCharacterLayerScreen()
    {
        GameSingleton.instance.cameraManager.EnableOverlayCamera((int)OverlayCameraID.CharacterScreen);
        GameSingleton.instance.cameraManager.EnableOverlayCanvas((int)OverlayCameraID.CharacterScreen);

        GameSingleton.instance.characterManager.HideAllCharacters();
    }

    public void LoadCurrentCharacterLayerScreen()
    {
        GameSingleton.instance.cameraManager.EnableOverlayCamera((int)OverlayCameraID.CharacterScreen);
        GameSingleton.instance.cameraManager.EnableOverlayCanvas((int)OverlayCameraID.CharacterScreen);
    }

    public void LoadLoadMenu()
    {
        GameSingleton.instance.cameraManager.EnableOverlayCamera((int)OverlayCameraID.SaveLoadOptionsMenu);
        GameSingleton.instance.cameraManager.EnableOverlayCanvas((int)OverlayCameraID.SaveLoadOptionsMenu);
        uiController.saveLoadMenu.saveMenu.gameObject.SetActive(false);
        uiController.saveLoadMenu.loadMenu.gameObject.SetActive(true);

        // UI Transition Animation here

        // =====
    }

    public void LoadSaveMenu()
    {
        GameSingleton.instance.cameraManager.EnableOverlayCamera((int)OverlayCameraID.SaveLoadOptionsMenu);
        GameSingleton.instance.cameraManager.EnableOverlayCanvas((int)OverlayCameraID.SaveLoadOptionsMenu);
        uiController.saveLoadMenu.saveMenu.gameObject.SetActive(true);
        uiController.saveLoadMenu.loadMenu.gameObject.SetActive(false);

        // UI Transition Animation here

        // =====

    }

    public void LoadPreferencesOptionsMenu()
    {
        GameSingleton.instance.cameraManager.EnableOverlayCamera((int)OverlayCameraID.PreferencesOptionsMenu);
        GameSingleton.instance.cameraManager.EnableOverlayCanvas((int)OverlayCameraID.PreferencesOptionsMenu);

        // UI Transition Animation here

        // =====
    }

    public void ClosePreferencesOptionsMenu()
    {
        // UI Transition Animation here

        // =====
        GameSingleton.instance.cameraManager.DisableOverlayCamera((int)OverlayCameraID.PreferencesOptionsMenu);
        GameSingleton.instance.cameraManager.DisableOverlayCanvas((int)OverlayCameraID.PreferencesOptionsMenu);
    }

    public void CloseSaveLoadOptionsMenu()
    {
        // UI Transition Animation here

        // =====

        GameSingleton.instance.cameraManager.DisableOverlayCamera((int)OverlayCameraID.SaveLoadOptionsMenu);
        GameSingleton.instance.cameraManager.DisableOverlayCanvas((int)OverlayCameraID.SaveLoadOptionsMenu);
    }

    public void LoadDialogueLogHistory()
    {
        GameSingleton.instance.cameraManager.EnableOverlayCamera((int)OverlayCameraID.DialogueLogHistory);
        GameSingleton.instance.cameraManager.EnableOverlayCanvas((int)OverlayCameraID.DialogueLogHistory);

        GameSingleton.instance.dialogueManager.StopFastForward();
        GameSingleton.instance.dialogueManager.SetGlobalAllowDialogueClick(false);
        // UI Transition Animation here

        // =====
    }

    public void CloseDialogueLogHistory()
    {
        GameSingleton.instance.cameraManager.DisableOverlayCamera((int)OverlayCameraID.DialogueLogHistory);
        GameSingleton.instance.cameraManager.DisableOverlayCanvas((int)OverlayCameraID.DialogueLogHistory);

        GameSingleton.instance.dialogueManager.RememberGlobalAllowDialogueClickBool();
        // UI Transition Animation here

        // =====
    }

    public void ToggleAVL(bool toggle)
    {
        if (toggle == true)
        {
            GameSingleton.instance.cameraManager.EnableOverlayCamera((int)OverlayCameraID.AVL);
        }
        else
        {
            GameSingleton.instance.cameraManager.EnableOverlayCamera((int)OverlayCameraID.AVL);
        }

    }



}
