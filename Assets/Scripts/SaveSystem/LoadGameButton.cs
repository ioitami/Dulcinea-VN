using UnityEngine;

public class LoadGameButton : MonoBehaviour
{
    public void LoadGame(int saveFileNumber)
    {
        GameSingleton.instance.gameStateManager.LoadGame(saveFileNumber);
    }
}
