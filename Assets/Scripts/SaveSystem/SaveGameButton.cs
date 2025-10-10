using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SaveGameButton : MonoBehaviour
{

    public void SaveGame()
    {
        GameSingleton.instance.gameStateManager?.SaveGame();
    }
}
