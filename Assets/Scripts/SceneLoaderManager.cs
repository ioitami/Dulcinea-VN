using Ink.Parsed;
using System.Collections.Generic;
using UnityEngine;

public class SceneLoaderManager : MonoBehaviour
{
    public Transform cameraTransform;

    public List<Canvas> currentActiveUI;

    [Header("Main Menu")]
    public Transform mainMenuLocation;
    public Canvas mainMenuCanvas;
    public GameObject mainMenuCharacterSprites;
    public GameObject mainMenuBGSprites;

    [Header("Window1")]
    public Transform window1Location;
    public GameObject window1CharacterSprites;
    public GameObject window1BackgroundSprites;

    [Header("NVL")]
    public Canvas nvlCanvas;
    // TODO: SCENE TRANSITIONS WILL GO HERE (TRANSITION FUNCTIONS SOMEWHERE ELSE)
    public void LoadMainMenu()
    {
        ResetCanvasActiveUIList();
        EnableCanvas(mainMenuCanvas);
        MoveCameraTo(mainMenuLocation.position);
    }

    public void LoadWindow1()
    {
        ResetCanvasActiveUIList();
        EnableCanvas(nvlCanvas);
        MoveCameraTo(window1Location.position);
    }

    public void EnableCanvas(Canvas canvas)
    {
        canvas.gameObject.SetActive(true);

        currentActiveUI.Add(canvas);
    }

    public void DisableCanvas(Canvas canvas)
    {
        canvas.gameObject?.SetActive(false);

        currentActiveUI.Remove(canvas);
    }

    public void ResetCanvasActiveUIList()
    {
        if(currentActiveUI.Count > 0)
        {
            foreach (Canvas canvas in currentActiveUI)
            {
                canvas.gameObject.SetActive(false);
            }

            currentActiveUI.Clear();
        }
    }

    public void MoveCameraTo(Vector3 cameraPosition)
    {
        cameraTransform.localPosition = new Vector3(cameraPosition.x, cameraPosition.y, 0);
    }
}
