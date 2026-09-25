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
    public List<SerializableVector3> charactersScale;
    public List<SerializableQuaternion> charactersRotation;

    public string window1Background;
    public string window2Background;
    public string window1Text;
    public string window2Text;

    public bool splitActive;
    public string splitWindow1GroupID;
    public string splitWindow1BlockID;
    public string splitWindow2GroupID;
    public string splitWindow2BlockID;
    public bool linkedSplitContinue;
    public bool toggleBothWindowClicksAllow;

    public bool awaitingSplitConvergenceClick;
    public string convergenceGroupID;
    public string convergenceBlockID;

    public int theRandomNumber;
}