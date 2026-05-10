using System;
using System.Collections.Generic;
using UnityEngine;

public class UnityMainThreadDispatcher : MonoBehaviour
{
    public static UnityMainThreadDispatcher instance { get; private set; }

    private Queue<Action> actionQueue = new Queue<Action>();

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        lock (actionQueue)
        {
            while (actionQueue.Count > 0)
                actionQueue.Dequeue().Invoke();
        }
    }

    public void Enqueue(Action action)
    {
        if (instance == null)
        {
            Debug.LogError("[UnityMainThreadDispatcher] Instance is null. Cannot enqueue action.");
            return;
        }

        lock (actionQueue)
        {
            actionQueue.Enqueue(action);
        }
    }
}