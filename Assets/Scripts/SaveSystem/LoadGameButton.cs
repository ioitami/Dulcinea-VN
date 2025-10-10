using UnityEngine;

public class LoadGameButton : MonoBehaviour
{
    public void LoadGame()
    {
        GameSingleton.instance.gameStateManager.LoadGame();
    }
}
