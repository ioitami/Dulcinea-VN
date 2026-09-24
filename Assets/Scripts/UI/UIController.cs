using System.Collections.Generic;
using UnityEngine;

public class UIController : MonoBehaviour
{
    [Header("Main Menu")]
    public MainMenu mainMenu;

    [Header("Options Menu")]
    public OptionsMenu optionsMenu;

    [Header("AVL")]
    public AVL avl;
    public NVL nvl;

    [Header("Window1")]
    public Window1 window1;
    public Window2 window2;

    [Header("ConvoLog")]
    public DialogueLogHistory dialogueLogHistory;

    //Generic screen access
    public UIScreenBase GetScreen(string screenName)
    {
        return UIScreenBase.Get(screenName);
    }

    public T GetScreen<T>(string screenName) where T : UIScreenBase
    {
        return UIScreenBase.Get<T>(screenName);
    }

    public void EnableScreen(string screenName)
    {
        UIScreenBase.Get(screenName)?.SetScreenActive(true);
    }

    public void DisableScreen(string screenName)
    {
        UIScreenBase.Get(screenName)?.SetScreenActive(false);
    }

    public void EnableAllScreens()
    {
        foreach (UIScreenBase screen in UIScreenBase.All)
        {
            screen.SetScreenActive(true);
        }
    }

    public void DisableAllScreens()
    {
        foreach (UIScreenBase screen in UIScreenBase.All)
        {
            screen.SetScreenActive(false);
        }
    }
}