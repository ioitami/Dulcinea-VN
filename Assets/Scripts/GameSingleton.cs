using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class GameSingleton : MonoBehaviour
{
    public static GameSingleton instance                    { get; private set; }

    public Camera mainCamera                                { get; private set; }
    public PlayerInput playerInput                          { get; private set; }
    public InputHandler inputHandler                        { get; private set; }
    public EventSystem eventSystem                          { get; private set; }
    public DialogueManager dialogueManager                  { get; private set; }
    public CharacterManager characterManager                { get; private set; }
    public GameStateManager gameStateManager                { get; private set; }
    public Canvas nvlCanvas                                  { get; private set; }
    public GameObject worldSpriteCanvas                    { get; private set; }


    void Awake()
    {

        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        instance = this;

        DontDestroyOnLoad(this.gameObject);


        // Load all the connected scripts
        mainCamera              = GetComponentInChildren<Camera>();
        playerInput             = GetComponentInChildren<PlayerInput>();
        inputHandler            = GetComponentInChildren<InputHandler>();
        eventSystem             = GetComponentInChildren<EventSystem>();
        dialogueManager         = GetComponentInChildren<DialogueManager>();
        characterManager        = GetComponentInChildren<CharacterManager>();
        gameStateManager        = GetComponentInChildren<GameStateManager>();
        nvlCanvas               = GetComponentInChildren<Canvas>();
        worldSpriteCanvas       = GameObject.Find("WorldSpriteCanvas");

    }

}
