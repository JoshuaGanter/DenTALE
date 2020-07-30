using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameScene {
    Praxis = 0,
    Archiv = 1
}

public class GameManager : MonoBehaviour
{
    private static GameManager instance;
    private GameState currentGameState = GameState.Default;

    public delegate void GameStateChange(GameState newState);
    public event GameStateChange OnGameStateChange;
    public Animator transition;
    public float transitionDuration = 1f;

    public GameObject Target { get; private set; }
    private Animator AnimatorObj;
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
            Artifact artifact = gameObj.GetComponent<Artifact>();
            if (artifact.isCursed)
            {
                StartCoroutine(SwitchScenes((int) artifact.toScene));
            }
        }
        else if (gameObj.tag == "Artifact" && currentGameState == GameState.InspectObject)
        {

            AnimatorObj = gameObj.GetComponentInChildren<Animator>();
            AnimatorObj.enabled = true;
            
            if (AnimatorObj.GetBool("Auf") == false)
            {
                AnimatorObj.SetBool("Auf", true);

            }

            else if (AnimatorObj.GetBool("Auf") == true)
            {
                AnimatorObj.SetBool("Auf", false);
            }
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
                var sceneObjects = GameObject.FindGameObjectsWithTag("Scene");
                foreach( var sceneObject in sceneObjects)
                {
                    sceneObject.GetComponent<Renderer>().enabled = true;
                }
                Target = null;
            }
        }
    }

    IEnumerator SwitchScenes(int toSceneIndex)
    {
        transition.SetTrigger("Start");
        
        yield return new WaitForSeconds(transitionDuration);
        SceneManager.LoadScene(toSceneIndex);
        transition.SetTrigger("Reset");
    }
}

public enum GameState
{
    Default,
    LookAround,
    InspectObject
}