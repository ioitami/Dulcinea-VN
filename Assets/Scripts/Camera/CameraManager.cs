using System.Collections.Generic;
using UnityEngine;

public enum MainCameraID
{
    Window1 = 0,
    Window2 = 1,
}

public enum MainCameraLocations
{
    MainMenu = 0,
    Window1 = 1,
    Window2 = 2
}

public class CameraManager : MonoBehaviour
{
    public List<Transform> mainCameraList;
    public List<Transform> mainCameraLocations;

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
}