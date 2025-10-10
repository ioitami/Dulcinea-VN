using UnityEngine;

public class MainMenu : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        GameSingleton.instance.dialogueManager.AllowStoryClicks = false;
        GameSingleton.instance.nvlCanvas.gameObject?.SetActive(false);
        GameSingleton.instance.worldSpriteCanvas?.SetActive(false);  
        Debug.Log("Cannot story click");
    }


}
