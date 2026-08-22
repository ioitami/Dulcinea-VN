using Ink.Parsed;
using System.Collections.Generic;
using UnityEngine;

public enum MainCameraID
{
    Window1     = 0, 
    Window2     = 1,
}

public enum MainCameraLocations
{
    MainMenu = 0,
    Window1 = 1,
    Window2 = 2
}

public enum ScreenID
{
    MainMenu                = 0,
    AVL                     = 1,
    NVL                     = 2,
    SaveLoadOptionsMenu     = 3,
    PreferencesOptionsMenu  = 4,
    DialogueLogHistory      = 5
}

public class CameraManager : MonoBehaviour
{
    public List<Transform> mainCameraList;
    public List<Transform> mainCameraLocations;
    public List<Transform> screenList;


    public void MoveCameraToLocation(int cameraID, int mainCamLocation)
    {
        Transform cam = mainCameraList[cameraID].transform;

        cam.position = mainCameraLocations[mainCamLocation].position;
    }

    public void EnableMainCamera(int id)
    {
        DisableAllCameras(); // only one main cam should be active at a time
        mainCameraList[id].gameObject.SetActive(true);
    }

    public void DisableMainCamera(int id)
    {
        mainCameraList[id].gameObject.SetActive(false);
    }

    public void DisableAllCameras()
    {
        foreach (Transform t in mainCameraList)
        {
            t.gameObject.SetActive(false);
        }
    }

    public void EnableAllOverlay()
    {
        foreach (Transform c in screenList)
        {
            c.gameObject.SetActive(true);
        }
    }

    public void DisableAllOverlay()
    {
        foreach (Transform c in screenList)
        {
            c.gameObject.SetActive(false);
        }
    }

    public void EnableOverlay(int id)
    {
        screenList[id].gameObject.SetActive(true);
    }
    public void DisableOverlay(int id)
    {
        screenList[id].gameObject.SetActive(false);
    }

}


