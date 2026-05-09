using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class GameSingleton : MonoBehaviour
{
    public static GameSingleton instance                    { get; private set; }

    public CameraManager cameraManager                      { get; private set; }
    public SceneLoaderManager sceneLoaderManager            { get; private set; }
    public AudioManager audioManager                        { get; private set; }
    public PlayerInput playerInput                          { get; private set; }
    public InputHandler inputHandler                        { get; private set; }
    public EventSystem eventSystem                          { get; private set; }
    public CharacterManager characterManager                { get; private set; }
    public SpriteAnimationManager spriteAnimationManager    { get; private set; }
    public DialogueManager dialogueManager                  { get; private set; }
    public GameStateManager gameStateManager                { get; private set; }
    public ServerManager serverManager { get; private set; }


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
        cameraManager           = GetComponentInChildren<CameraManager>();
        sceneLoaderManager      = GetComponentInChildren<SceneLoaderManager>();
        audioManager            = GetComponentInChildren<AudioManager>();
        playerInput             = GetComponentInChildren<PlayerInput>();
        inputHandler            = GetComponentInChildren<InputHandler>();
        eventSystem             = GetComponentInChildren<EventSystem>();
        characterManager        = GetComponentInChildren<CharacterManager>();
        spriteAnimationManager  = GetComponentInChildren<SpriteAnimationManager>();
        dialogueManager         = GetComponentInChildren<DialogueManager>();
        gameStateManager        = GetComponentInChildren<GameStateManager>();
        serverManager           = GetComponentInChildren<ServerManager>();

    }

}
