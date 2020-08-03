using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameScene {
    Praxis = 0,
    Archiv = 1
}

public delegate void AddItemToInventory(Item item);
public delegate void RemoveItemFromInventory(Item item);

public class GameManager : MonoBehaviour
{
    private static GameManager instance;
    private Player _player;
    private GameState currentGameState = GameState.Paused;

    public delegate void GameStateChange(GameState newState);
    public event GameStateChange OnGameStateChange;
    public event AddItemToInventory OnAddItemToInventory;
    public event RemoveItemFromInventory OnRemoveItemFromInventory;
    public Animator transition;
    public float transitionDuration = 1f;
    public DisplayInventory DisplayInventory;

    public Item Target { get; private set; }
    public GameObject TargetObject { get; private set; }
    private Animator AnimatorObj;
    private List<GameObject> _inspectObjects = new List<GameObject>();
    private List<Item> _inspectItems = new List<Item>();
    Dictionary<Item[], Item> recipes = new Dictionary<Item[], Item>();
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

    private void OnInventoryOpened(bool isOpen)
    {
        if (isOpen)
        {
            setCurrentGameState(GameState.Inventory);
        }
        else
        {
            setCurrentGameState(GameState.Adventure);
        }
    }

    private void OnItemClicked(Item item)
    {
        if (!_inspectItems.Contains(item))
        {
            foreach (GameObject gameObject in _inspectObjects)
            {
                Destroy(gameObject);
            }
            _inspectObjects.Clear();
            _inspectItems.Clear();

            GameObject prev = Instantiate(item.prefab, Vector3.zero, Quaternion.identity);
            prev.transform.parent = Camera.main.transform;
            prev.transform.localPosition = new Vector3(1.5f + item.prefab.transform.position.x, 1.0f + item.prefab.transform.position.y, 6.0f + item.prefab.transform.position.z);
            prev.transform.localRotation = Quaternion.Euler(-90, 0, 0);
            //prev.transform.Rotate(-90, 0, 0);
            _inspectObjects.Add(prev);
            _inspectItems.Add(item);
            TargetObject = prev;

            if (recipes.TryGetValue(_inspectItems.ToArray(), out Item craftResult))
            {
                DisplayInventory.EnableCrafting(true);
            }
        }

        if (currentGameState != GameState.Inspect)
        {
            setCurrentGameState(GameState.Inspect);

        }
    }

    private void OnCraftingOpened(bool isOpen)
    {
        if (isOpen == false)
        {
            foreach (GameObject gameObject in _inspectObjects)
            {
                Destroy(gameObject);
            }
            _inspectObjects.Clear();
            _inspectItems.Clear();
            setCurrentGameState(GameState.Inventory);
        }
    }

    void OnGameObjectClicked(GameObject gameObj)
    {
        if (gameObj.tag == "Artifact" && (currentGameState == GameState.Adventure || currentGameState == GameState.Inventory))
        {
            Artifact artifact = gameObj.GetComponent<Artifact>();
            if (artifact.isCursed)
            {
                StartCoroutine(SwitchScenes((int) artifact.toScene));
            }
            else
            {
                if (OnAddItemToInventory != null)
                {
                    OnAddItemToInventory(artifact.Item);
                }
                _player.Inventory.AddItem(artifact.Item);
                Destroy(gameObj);
            }
        }
        /*else if (gameObj.tag == "Artifact" && currentGameState == GameState.Inspect)
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
        }*/
        else if (gameObj.tag == "InteractableObject")
        {
            gameObj.GetComponent<Interactable>()?.InteractWith();
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

        recipes = new Dictionary<Item[], Item>();
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
        DisplayInventory.OnInventoryOpened += OnInventoryOpened;
        DisplayInventory.OnCraftingOpened += OnCraftingOpened;
        ItemSlot.OnItemClicked += OnItemClicked;
        
        setCurrentGameState(GameState.Adventure);

        _player = GetComponentInChildren<Player>();
    }

    void OnGUI()
    {
        /*if (currentGameState == GameState.Inspect)
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
        }*/
        GUI.Label(new Rect(500, 10, 150, 100), "Game State: " + currentGameState.ToString());
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