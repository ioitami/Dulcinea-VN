using Ink.Parsed;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

[Serializable]
public class OptionsTabButton
{
    public GameObject optionsTabBtn;
    public GameObject linkedRightPage;
    public UnityEvent onClick = new UnityEvent();
}

public class OptionsTab : MonoBehaviour
{
    public List<OptionsTabButton> optionTabButtonList;

    public void SetOptionsTabButtonListeners()
    {
        for (int i = 0; i < optionTabButtonList.Count; i++)
        {
            int index = i;

            if (optionTabButtonList[i].optionsTabBtn != null) 
                optionTabButtonList[i].optionsTabBtn.GetComponentInChildren<Button>().onClick.AddListener(() => SelectTabButton(index));
        }
    }

    public void SelectTabButton(int index)
    {
        for(int i = 0; i < optionTabButtonList.Count; i++)
        {
            optionTabButtonList[i].optionsTabBtn.GetComponentInChildren<Button>().interactable = true;
            optionTabButtonList[i].linkedRightPage.SetActive(false);
        }

        optionTabButtonList[index].optionsTabBtn.GetComponentInChildren<Button>().interactable = false;

        if (optionTabButtonList[index].linkedRightPage != null) optionTabButtonList[index].linkedRightPage.SetActive(true);

        optionTabButtonList[index].onClick.Invoke();
    }
}
