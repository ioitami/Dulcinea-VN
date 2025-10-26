using UnityEngine;

public class ServerManager : MonoBehaviour
{
    [SerializeField]
    private bool isMainWindow = true;

    public bool IsMainWindow
    {
        get => isMainWindow;
        private set
        {
            isMainWindow = value;
        }
    }
}
