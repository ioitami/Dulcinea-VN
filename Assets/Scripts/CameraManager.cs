using Ink.Parsed;
using System.Collections.Generic;
using UnityEngine;

public enum MainCameraID
{
    MainMenu    = 0,
    Window1     = 1, 
    Window2     = 2,
}

public enum OverlayCameraID
{
    SaveLoadOptionsMenu     = 0,
    PreferencesOptionsMenu  = 1,
    AVL                     = 2,
    NVL                     = 3,
    CharacterScreen         = 4,
    DialogueLogHistory      = 5
}

public class CameraManager : MonoBehaviour
{
    public List<Transform> mainCameraList;
    public List<Transform> overlayCameraList;
    public List<Canvas> overlayCanvasList;

    public List<Transform> savedOverlayCameraList;


    public void EnableMainCamera(int id)
    {
        DisableAllMainCameras(); // only one main cam should be active at a time
        mainCameraList[id].gameObject.SetActive(true);
    }

    public void EnableOverlayCamera(int id)
    {
        overlayCameraList[id].gameObject.SetActive(true);
    }

    public void DisableMainCamera(int id)
    {
        mainCameraList[id].gameObject.SetActive(false);
    }

    public void DisableOverlayCamera(int id)
    {
        overlayCameraList[id].gameObject.SetActive(false);
    }

    public void DisableAllCameras()
    {
        foreach (Transform t in mainCameraList)
        {
            t.gameObject.SetActive(false);
        }

        foreach (Transform t in overlayCameraList)
        {
            t.gameObject.SetActive(false);
        }
    }

    public void DisableAllMainCameras()
    {
        foreach (Transform t in mainCameraList)
        {
            t.gameObject.SetActive(false);
        }
    }

    public void DisableAllOverlayCameras()
    {
        foreach (Transform t in overlayCameraList)
        {
            if (t.gameObject.activeSelf == true)
            {
                savedOverlayCameraList.Add(t.gameObject.transform);
                t.gameObject.SetActive(false);
            }
        }
    }

    public void EnablePreviouslyDisabledOverlayCameras()
    {
        foreach (Transform t in savedOverlayCameraList)
        {
            t.gameObject.SetActive(true);
        }

        savedOverlayCameraList.Clear();
    }


    public void DisableAllOverlayCanvas()
    {
        foreach (Canvas c in overlayCanvasList)
        {
            c.gameObject.SetActive(false);
        }
    }

    public void EnableOverlayCanvas(int id)
    {
        overlayCanvasList[id].gameObject.SetActive(true);
    }
    public void DisableOverlayCanvas(int id)
    {
        overlayCanvasList[id].gameObject.SetActive(false);
    }


}


