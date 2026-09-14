using System;
using System.Collections.Generic;

[Serializable]
public class SaveData
{
    public int saveSlotNumber;
    public string chapterName;
    public string description;
    public string saveTimeStamp;
    public string screenshotBase64;
    public string dialogueGroupID;
    public string dialogueBlockID;
    public bool requiresServer;
    public List<string> charactersOnScreen;
    public List<string> charactersMood;
    public List<SerializableVector3> charactersPosition;
}