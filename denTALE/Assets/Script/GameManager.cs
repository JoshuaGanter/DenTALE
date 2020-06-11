using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;
    private GameState currentGameState = GameState.Default;

    public delegate void GameStateChange(GameState newState);
    public event GameStateChange OnGameStateChange;

    public GameObject Target { get; private set; }

    public static GameManager Instance
    {
        get { return instance; }
    }

    public GameState CurrentGameState
    {
        get { return currentGameState; }
    }

    void setCurrentGameState(GameState newGameState)
    {
        currentGameState = newGameState;
        if (OnGameStateChange != null)
        {
            OnGameStateChange(currentGameState);
        }
    }

    void OnGameObjectClicked(GameObject gameObj)
    {
        if (gameObj.tag == "Artifact" && currentGameState == GameState.LookAround)
        {
            Target = gameObj;
            setCurrentGameState(GameState.InspectObject);
        }
        else if (gameObj.tag == "Artifact" && currentGameState == GameState.InspectObject)
        {
            // ToDo: Play Animation
            //gameObj.GetComponent<Animator>().enabled = true;
        }
    }

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        // Subscribe to events:
        CameraController.OnGameObjectClicked += OnGameObjectClicked;
        
        setCurrentGameState(GameState.LookAround);
    }

    void Update()
    {
        
    }

    void OnGUI()
    {
        if (currentGameState == GameState.InspectObject)
        {
            if (GUI.Button(new Rect(10, 10, 150, 100), "Back"))
            {
                setCurrentGameState(GameState.LookAround);
                Target = null;
            }
        }
    }
}

public enum GameState
{
    Default,
    LookAround,
    InspectObject
}