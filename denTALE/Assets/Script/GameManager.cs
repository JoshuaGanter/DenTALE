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
    private Player _player;
    private GameState currentGameState = GameState.Paused;

    public delegate void GameStateChange(GameState newState);
    public event GameStateChange OnGameStateChange;
    public Animator transition;
    public float transitionDuration = 1f;

    public Item Target { get; private set; }
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
        if (gameObj.tag == "Artifact" && currentGameState == GameState.Adventure)
        {
            Artifact artifact = gameObj.GetComponent<Artifact>();
            if (artifact.isCursed)
            {
                StartCoroutine(SwitchScenes((int) artifact.toScene));
            }
            else
            {
                _player.Inventory.AddItem(artifact.Item);
                Destroy(gameObj);
            }
        }
        else if (gameObj.tag == "Artifact" && currentGameState == GameState.Inspect)
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

    void OnShakeStarted()
    {

    }

    void OnShakeEnded()
    {
        // TODO: play breaking sound

        if (currentGameState == GameState.Inspect && Target.consistsOf.Length != 0)
        {
            _player.Inventory.RemoveItem(Target);
            _player.Inventory.AddItems(Target.consistsOf);
            // TODO: play some kind of animation
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

        Dictionary<Item[], Item> recipes = new Dictionary<Item[], Item>();
        foreach (Item item in Resources.LoadAll<Item>("Items"))
        {
            if (item.consistsOf.Length > 0)
            {
                List<Item> components = new List<Item>(item.consistsOf);
                components.Sort((x, y) => string.Compare(x.title, y.title));
                recipes.Add(components.ToArray(), item);
            }
        }
    }

    void Start()
    {
        // Subscribe to events:
        CameraController.OnGameObjectClicked += OnGameObjectClicked;
        AccelerationManager.ShakeStarted += OnShakeStarted;
        AccelerationManager.ShakeEnded += OnShakeEnded;
        
        setCurrentGameState(GameState.Adventure);

        _player = GetComponentInChildren<Player>();
    }

    void Update()
    {
        
    }

    void OnGUI()
    {
        if (currentGameState == GameState.Inspect)
        {
            if (GUI.Button(new Rect(10, 10, 150, 100), "Back"))
            {
                setCurrentGameState(GameState.Adventure);
                var sceneObjects = GameObject.FindGameObjectsWithTag("Scene");
                foreach( var sceneObject in sceneObjects)
                {
                    sceneObject.GetComponent<Renderer>().enabled = true;
                }
                Target = null;
            }
        }
        GUI.Label(new Rect(10, 120, 150, 100), "Items in inventory: " + _player.Inventory.Count);
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
    Paused,
    Adventure,
    Inventory,
    Inspect
}