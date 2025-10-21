using Ink.Parsed;
using System.Collections.Generic;
using UnityEngine;

public class SceneLoaderManager : MonoBehaviour
{

    //public List<Canvas> currentActiveUI;

    [Header("UI")]
    public UIController uiController;



    // TODO: SCENE TRANSITIONS WILL GO HERE (TRANSITION FUNCTIONS SOMEWHERE ELSE)
    public void LoadMainMenu()
    {
        // TODO: ADD SCENE TRANSITION HERE

        //==============

        GameSingleton.instance.cameraManager.DisableAllOverlayCanvas();
        GameSingleton.instance.cameraManager.EnableCamera(isMain: true, (int)MainCameraID.MainMenu);

    }
    public void LoadWindow1()
    {
        GameSingleton.instance.cameraManager.EnableCamera(isMain: true, (int)MainCameraID.Window1);
        GameSingleton.instance.cameraManager.EnableCamera(isMain: false, (int)OverlayCameraID.NVL);

        GameSingleton.instance.cameraManager.DisableAllOverlayCanvas();
        GameSingleton.instance.cameraManager.EnableOverlayCanvas((int)OverlayCameraID.NVL);
    }



    // OVERLAY CAMERA CONTROLS

    public void LoadLoadMenu()
    {
        GameSingleton.instance.cameraManager.EnableCamera(isMain: false, (int)OverlayCameraID.SaveLoadOptionsMenu);
        GameSingleton.instance.cameraManager.EnableOverlayCanvas((int)OverlayCameraID.SaveLoadOptionsMenu);
        uiController.saveLoadMenu.saveMenu.gameObject.SetActive(false);
        uiController.saveLoadMenu.loadMenu.gameObject.SetActive(true);

        // UI Transition Animation here

    }

    public void LoadSaveMenu()
    {
        GameSingleton.instance.cameraManager.EnableCamera(isMain: false, (int)OverlayCameraID.SaveLoadOptionsMenu);
        GameSingleton.instance.cameraManager.EnableOverlayCanvas((int)OverlayCameraID.SaveLoadOptionsMenu);
        uiController.saveLoadMenu.saveMenu.gameObject.SetActive(true);
        uiController.saveLoadMenu.loadMenu.gameObject.SetActive(false);

        // UI Transition Animation here

    }

    public void LoadOptionsMenu()
    {

    }

    public void CloseSaveLoadOptionsMenu()
    {
        // UI Transition Animation here

        // =====

        GameSingleton.instance.cameraManager.DisableCamera(isMain: false, (int)OverlayCameraID.SaveLoadOptionsMenu);
        GameSingleton.instance.cameraManager.DisableOverlayCanvas((int)OverlayCameraID.SaveLoadOptionsMenu);
    }

    public void ToggleNVL(bool toggle)
    {
        if (toggle == true)
        {
            GameSingleton.instance.cameraManager.EnableCamera(isMain: false, (int)OverlayCameraID.NVL);
        }
        else
        {
            GameSingleton.instance.cameraManager.DisableCamera(isMain: false, (int)OverlayCameraID.NVL);
        }

    }



}
