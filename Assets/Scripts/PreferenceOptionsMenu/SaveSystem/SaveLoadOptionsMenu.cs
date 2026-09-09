using Ink.Parsed;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SaveLoadOptionsMenu : MonoBehaviour
{
    public Canvas saveMenu;
    public Canvas loadMenu;
    public Button returnBtn;

    public List<SaveGameButton> saveGameButtonSlots;
    public List<LoadGameButton> loadGameButtonSlots;

    private void Start()
    {
        UpdateSaveLoadSlots();
    }

    public void UpdateSaveLoadSlots()
    {
        for (int i = 0; i < saveGameButtonSlots.Count; i++)
        {
            //saveGameButtonSlots[i].saveSpritePreview.sprite = GameSingleton.instance.gameStateManager.LoadScreenshotSprite(i + 1);
        }

        for (int i = 0; i < loadGameButtonSlots.Count; i++)
        {
            //loadGameButtonSlots[i].loadSpritePreview.sprite = GameSingleton.instance.gameStateManager.LoadScreenshotSprite(i + 1);
        }
    }
}
