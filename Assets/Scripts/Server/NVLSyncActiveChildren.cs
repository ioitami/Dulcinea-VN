using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class NVLSyncActiveChildren : NetworkBehaviour
{
    private readonly SyncList<bool> childActiveStates = new SyncList<bool>();

    private List<GameObject> trackedChildren = new List<GameObject>();

    private void Awake()
    {
        // Collect all children recursively
        foreach (Transform child in GetComponentsInChildren<Transform>(true))
        {
            if (child.gameObject == gameObject) continue;
            trackedChildren.Add(child.gameObject);
        }
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        childActiveStates.Clear();
        foreach (GameObject child in trackedChildren)
            childActiveStates.Add(child.activeSelf);
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        childActiveStates.Callback += OnChildStatesChanged;
        ApplyAllStates();
    }

    private void Update()
    {
        if (!isServer) return;

        for (int i = 0; i < trackedChildren.Count; i++)
        {
            if (trackedChildren[i] == null) continue;

            if (trackedChildren[i].activeSelf != childActiveStates[i])
                childActiveStates[i] = trackedChildren[i].activeSelf;
        }
    }

    private void OnChildStatesChanged(SyncList<bool>.Operation op, int index, bool oldValue, bool newValue)
    {
        if (isServer) return;

        if (index >= 0 && index < trackedChildren.Count)
            trackedChildren[index].SetActive(newValue);
    }

    private void ApplyAllStates()
    {
        if (isServer) return;

        for (int i = 0; i < childActiveStates.Count; i++)
        {
            if (i < trackedChildren.Count)
                trackedChildren[i].SetActive(childActiveStates[i]);
        }
    }
}