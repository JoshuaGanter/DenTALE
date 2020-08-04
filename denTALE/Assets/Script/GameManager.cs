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
public delegate void TargetChanged(GameObject newTarget);

public class GameManager : MonoBehaviour
{
    private static GameManager instance;
    private Player _player;
    private GameState currentGameState = GameState.Paused;

    public delegate void GameStateChange(GameState newState);
    public event GameStateChange OnGameStateChange;
    public event AddItemToInventory OnAddItemToInventory;
    public event RemoveItemFromInventory OnRemoveItemFromInventory;
    public event TargetChanged OnTargetChanged;
    public Animator transition;
    public float transitionDuration = 1f;
    public DisplayInventory DisplayInventory;

    public Item Target { get; private set; }
    public GameObject TargetObject { get; private set; }
    private List<GameObject> _inspectObjects = new List<GameObject>();
    private List<Item> _inspectItems = new List<Item>();
    private Dictionary<Item[], Item> recipes;
    private bool _isShaking;
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

    private void OnItemDragged(Item item)
    {
        if (!_inspectItems.Contains(item) && _inspectItems.Count < 3)
        {
            GameObject prev = Instantiate(item.prefab, Vector3.zero, Quaternion.identity, Camera.main.transform);
            prev.transform.localRotation = Quaternion.Euler(-90, 0, 0);
            _inspectObjects.Add(prev);
            _inspectItems.Add(item);

            RearrangeInspectItems();
            CheckIfRecipeIsCraftable();
        }
    }

    private void RearrangeInspectItems()
    {
        float translationX = -(_inspectObjects.Count - 1.0f) / 2.0f;
        for (int i = 0; i < _inspectObjects.Count; i++)
        {
            GameObject displayedObject = _inspectObjects[i];
            Item displayedItem = _inspectItems[i];
            displayedObject.transform.localPosition = GetItemPosition(displayedItem.prefab.transform.position, (translationX + i) * 2);
        }
    }

    private void CheckIfRecipeIsCraftable()
    {
        List<Item> inspectItemsSorted = new List<Item>(_inspectItems);
        inspectItemsSorted.Sort((x, y) => string.Compare(x.title, y.title));
        if (recipes.TryGetValue(inspectItemsSorted.ToArray(), out Item craftResult))
        {
            DisplayInventory.EnableCrafting(true);
        }
        else
        {
            DisplayInventory.EnableCrafting(false);
        }
    }

    private Vector3 GetItemPosition(Vector3 prefabPosition)
    {
        return GetItemPosition(prefabPosition, 0);
    }
    
    private Vector3 GetItemPosition(Vector3 prefabPosition, float translationX)
    {
        return new Vector3(1.5f + prefabPosition.x + translationX, 1.0f + prefabPosition.y, 6.0f + prefabPosition.z);
    }

    private void OnItemClicked(Item item)
    {
        if (!_inspectItems.Contains(item) || _inspectItems.Count > 1)
        {
            ClearTarget();

            GameObject prev = Instantiate(item.prefab, Vector3.zero, Quaternion.identity, Camera.main.transform);
            prev.transform.localPosition = new Vector3(1.5f + item.prefab.transform.position.x, 1.0f + item.prefab.transform.position.y, 6.0f + item.prefab.transform.position.z);
            prev.transform.localRotation = Quaternion.Euler(-90, 0, 0);
            _inspectObjects.Add(prev);
            _inspectItems.Add(item);
            TargetObject = prev;
            Target = item;

            if (OnTargetChanged != null)
            {
                OnTargetChanged(prev);
            }
            DisplayInventory.EnableCrafting(false);
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
            ClearTarget();
            setCurrentGameState(GameState.Inventory);
        }
    }

    private void OnCraft()
    {
        if (recipes.TryGetValue(_inspectItems.ToArray(), out Item craftResult))
        {
            foreach (Item item in _inspectItems)
            {
                _player.Inventory.RemoveItem(item);
                if (OnRemoveItemFromInventory != null)
                {
                    OnRemoveItemFromInventory(item);
                }
            }

            _player.Inventory.AddItem(craftResult);
            if (OnAddItemToInventory != null)
            {
                OnAddItemToInventory(craftResult);
            }

            ClearTarget();
            OnItemClicked(craftResult);
        }
    }

    private void ClearTarget()
    {
        Target = null;
        TargetObject = null;
        if (OnTargetChanged != null)
        {
            OnTargetChanged(null);
        }
        foreach (GameObject gameObject in _inspectObjects)
        {
            Destroy(gameObject);
        }
        _inspectObjects.Clear();
        _inspectItems.Clear();
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
        else if (currentGameState == GameState.Inspect && _inspectObjects.Count > 1)
        {
            int index = _inspectObjects.IndexOf(gameObj);
            Destroy(gameObj);
            _inspectObjects.RemoveAt(index);
            _inspectItems.RemoveAt(index);
            RearrangeInspectItems();
            CheckIfRecipeIsCraftable();
        }
        else if (gameObj.tag == "InteractableObject")
        {
            gameObj.GetComponent<Interactable>()?.InteractWith();
        }
    }

    void OnShakeStarted()
    {
        _isShaking = true;
    }

    void OnShakeEnded()
    {
        _isShaking = false;
        // TODO: play breaking sound

        if (currentGameState == GameState.Inspect && Target.consistsOf.Length != 0)
        {
            _player.Inventory.RemoveItem(Target);
            _player.Inventory.AddItems(Target.consistsOf);

            if (OnRemoveItemFromInventory != null)
            {
                OnRemoveItemFromInventory(Target);
            }
            if (OnAddItemToInventory != null)
            {
                foreach (Item item in Target.consistsOf)
                {
                    OnAddItemToInventory(item);
                }
            }
            Item[] newTarget = Target.consistsOf;

            ClearTarget();
            OnItemClicked(newTarget[0]);
            for (int i = 1; i < newTarget.Length; i++)
            {
                OnItemDragged(newTarget[i]);
            }
            
            // TODO: play some kind of animation
        }
    }

    public void OnRotateItemInInspector(float amount)
    {
        TargetObject.transform.Rotate(0, 0, -amount);
    }

    public void OnClickInScene(Vector2 position)
    {
        Ray ray = Camera.main.ScreenPointToRay(position);
        Debug.DrawRay(ray.origin, ray.direction*4, Color.red, 10);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            OnGameObjectClicked(hit.collider.gameObject);
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

        recipes = new Dictionary<Item[], Item>(new RecipeComparer());
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
        DisplayInventory.OnCraft += OnCraft;
        ItemSlot.OnItemClicked += OnItemClicked;
        ItemSlot.OnItemDragged += OnItemDragged;
        ClickPlane.OnClickInScene += OnClickInScene;
        ClickPlane.OnRotateItemInInspector += OnRotateItemInInspector;
        
        setCurrentGameState(GameState.Adventure);

        _player = GetComponentInChildren<Player>();
    }

    void OnGUI()
    {
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