using System.Collections.Generic;
using UnityEngine;

public class BackgroundManager : MonoBehaviour
{
    [SerializeField]
    public Transform backgroundSpirteParent_MainMenu;
    public Transform backgroundSpriteParent_Window1;
    public Transform backgroundSpriteParent_Window2;

    [Header("Backgrounds List")]
    public List<BackgroundPreset> backgrounds = new List<BackgroundPreset>();

    // Add other functions to manage bg as needed

    private void Awake()
    {
        RemoveChildren(backgroundSpriteParent_Window1);
        RemoveChildren(backgroundSpriteParent_Window2);

        // Set Main Menu BG based on Save?
        SetMainMenuBackground(0);
    }

    void RemoveChildren(Transform parent)
    {
        foreach (Transform child in parent)
        {
            Destroy(child.gameObject);
        }
    }

    // WILL REPLACE PREVIOUS BG IF ANY
    public void SetBackground(string backgroundName, int windowNumber)
    {
        BackgroundPreset preset = backgrounds.Find(b => b.backgroundName == backgroundName);

        if (preset != null)
        {
            Transform parent = windowNumber == 1 ? backgroundSpriteParent_Window1 : backgroundSpriteParent_Window2;

            RemoveChildren(parent);

            GameObject bgInstance = Instantiate(preset.backgroundPrefab);
            bgInstance.transform.SetParent(parent, false);
            bgInstance.transform.localPosition = Vector3.zero;
        }
        else
        {
            Debug.LogWarning($"Background '{backgroundName}' not found!");
        }
    }

    public void SetBackground(int bgIndex, int windowNumber)
    {
        if (bgIndex < 0 || bgIndex >= backgrounds.Count)
        {
            Debug.LogWarning($"Background index '{bgIndex}' is out of range!");
            return;
        }

        BackgroundPreset preset = backgrounds[bgIndex];

        if (preset != null)
        {
            Transform parent = windowNumber == 1 ? backgroundSpriteParent_Window1 : backgroundSpriteParent_Window2;

            RemoveChildren(parent);

            GameObject bgInstance = Instantiate(preset.backgroundPrefab);
            bgInstance.transform.SetParent(parent, false);
            bgInstance.transform.localPosition = Vector3.zero;
        }
        else
        {
            Debug.LogWarning($"Background number '{bgIndex}' not found!");
        }
    }

    public void SetMainMenuBackground(string backgroundName)
    {
        BackgroundPreset preset = backgrounds.Find(b => b.backgroundName == backgroundName);

        if (preset != null)
        {
            Transform parent = backgroundSpirteParent_MainMenu;

            RemoveChildren(parent);

            GameObject bgInstance = Instantiate(preset.backgroundPrefab);
            bgInstance.transform.SetParent(parent, false);
            bgInstance.transform.localPosition = Vector3.zero;
        }
        else
        {
            Debug.LogWarning($"Background '{backgroundName}' not found!");
        }
    }

    public void SetMainMenuBackground(int bgIndex)
    {
        BackgroundPreset preset = backgrounds[bgIndex];

        if (preset != null)
        {
            Transform parent = backgroundSpirteParent_MainMenu;

            RemoveChildren(parent);

            GameObject bgInstance = Instantiate(preset.backgroundPrefab);
            bgInstance.transform.SetParent(parent, false);
            bgInstance.transform.localPosition = Vector3.zero;
        }
        else
        {
            Debug.LogWarning($"Background number '{bgIndex}' not found!");
        }
    }
}

[System.Serializable]
public class BackgroundPreset
{
    public string backgroundName;
    public GameObject backgroundPrefab;
}
