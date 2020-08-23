using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum GameScene {
    Archiv = 0,
    Praxis = 1,
    Tempel = 2
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
    public GameObject _hint;

    public Item Target { get; private set; }
    public GameObject TargetObject { get; private set; }
    public static bool[] ScenesDone = new bool[]{ false, false };
    public static bool[] HintsShown = new bool[]{ false, false, false };
    public GameObject Curator;
    private List<GameObject> _inspectObjects = new List<GameObject>();
    private List<Item> _inspectItems = new List<Item>();
    private Dictionary<Item[], Item> recipes;
    private bool _isShaking;
    private Vector3 _savedPlayedCoordinates;
    private List<Item> _allItems = new List<Item>();
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
            float rotateZ = 0;
            if (_inspectObjects.Count >= 1)
            {
                rotateZ = _inspectObjects[_inspectObjects.Count - 1].transform.rotation.eulerAngles.z;
            }
            GameObject prev = Instantiate(item.prefab, Vector3.zero, Quaternion.identity, Camera.main.transform);
            prev.transform.localRotation = Quaternion.Euler(-90, 0, 0);
            prev.transform.Rotate(0, 0, rotateZ);
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
        _inspectItems.Sort((x, y) => string.Compare(x.title, y.title));
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
            Target = _inspectItems[0];
            TargetObject = _inspectObjects[0];
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
        
        if (currentGameState == GameState.Inspect && _inspectObjects.Count == 1 && Target.consistsOf.Length != 0)
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
        }
    }

    public void OnRotateItemInInspector(float amount)
    {
        foreach (GameObject gameObject in _inspectObjects)
        {
            gameObject.transform.Rotate(0, 0, -amount / 2);
        }
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

    public void ShowHint(string message)
    {
        _hint.GetComponentInChildren<Text>().text = message;
        _hint.SetActive(true);
        StartCoroutine(CloseHint(7));
    }

    private IEnumerator CloseHint(int waitForSeconds)
    {
        yield return new WaitForSeconds(waitForSeconds);
        _hint.SetActive(false);
    }

    private Item GetItemByTitle(string title)
    {
        foreach (Item item in _allItems)
        {
            if (item.title == title)
            {
                return item;
            }
        }
        return null;
    }

    void Awake()
    {
        if (instance != null && instance != this)
        {
            DestroyImmediate(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        _player = GetComponentInChildren<Player>();
        _player.Inventory.Clear();

        recipes = new Dictionary<Item[], Item>(new RecipeComparer());
        foreach (Item item in Resources.LoadAll<Item>("Items"))
        {
            item.PickedUp = false;
            if (item.consistsOf.Length > 0)
            {
                List<Item> components = new List<Item>(item.consistsOf);
                components.Sort((x, y) => string.Compare(x.title, y.title));
                recipes.Add(components.ToArray(), item);
            }
            _allItems.Add(item);
        }
    }

    void Start()
    {
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
    }

    public IEnumerator SwitchScenes(int toSceneIndex)
    {
        if (SceneManager.GetActiveScene().buildIndex == (int) GameScene.Archiv)
        {
            _savedPlayedCoordinates = _player.transform.localPosition;
        }
        transition.SetTrigger("Start");
        
        yield return new WaitForSeconds(transitionDuration);
        SceneManager.LoadScene(toSceneIndex);
        if (toSceneIndex == (int) GameScene.Archiv)
        {
            _player.transform.localPosition = _savedPlayedCoordinates;
            foreach (Item item in _player.Inventory)
            {
                OnRemoveItemFromInventory(item);
            }
            _player.Inventory.Clear();
            _player.Inventory.AddItem(GetItemByTitle("Klemmbrett mit Stift"));
            _player.Inventory.AddItem(GetItemByTitle("Archivraumschlüssel"));
            if (ScenesDone[0])
            {
                _player.Inventory.AddItem(GetItemByTitle("Füllpistole"));
                _player.Inventory.AddItem(GetItemByTitle("Pelikan"));
                _player.Inventory.AddItem(GetItemByTitle("Pumpe"));
            }
            if (ScenesDone[1])
            {
                _player.Inventory.AddItem(GetItemByTitle("Gusspresse"));
                _player.Inventory.AddItem(GetItemByTitle("Mundhammer"));
                _player.Inventory.AddItem(GetItemByTitle("Zahnschlüssel"));
            }
            foreach (Item item in _player.Inventory)
            {
                OnAddItemToInventory(item);
            }
            if (ScenesDone[0] && ScenesDone[1])
            {
                Curator.GetComponentInChildren<Text>().text = "Oh, mein lieber Freund, Sie haben es ja tatsächlich geschafft alle Gegenstände, die ich Ihnen aufgeschrieben habe, zu sammeln. Damit sind Sie der erste und das will etwas heißen, ich habe diese Liste schon vielen Leuten zugemutet. Nun kann ich Ihnen ja auch die Wahrheit sagen: Ich selbst habe die Gegenstände verflucht um einen Nachfolger für die Leitung des Museums zu finden. Wie mir scheint ist das wohl nun geglückt und ich kann mich in meine wohlverdiente Rente zurückziehen, hahaha. Viel Spaß mit dem Museum, Herr Direktor! Hahaha!";
                Curator.SetActive(true);
            }
        }
        else if (toSceneIndex == (int) GameScene.Praxis)
        {
            _player.transform.localPosition = new Vector3(5.0f, -12.0f, 11.5f);
        }
        else if (toSceneIndex == (int) GameScene.Tempel)
        {
            _player.transform.localPosition = new Vector3(30.0f, -12.0f, -63.0f);
        }

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