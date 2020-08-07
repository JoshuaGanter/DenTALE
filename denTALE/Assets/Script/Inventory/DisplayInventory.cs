using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public delegate void OpenInventory(bool isOpen);
public delegate void OpenCrafting(bool isOpen);
public delegate void Craft();

public class DisplayInventory : MonoBehaviour, IPointerClickHandler, IDragHandler, IEndDragHandler
{
    public static event OpenInventory OnInventoryOpened;
    public static event OpenCrafting OnCraftingOpened;
    public static event Craft OnCraft;
    public GameObject InventorySlotPrefab;
    public GameObject EmptyInventorySlotPrefab;
    public Canvas Canvas;
    public Button MenuButton;
    public Button CraftButton;
    public Button CraftCloseButton;
    public Sprite OpenedSprite;
    public Sprite ClosedSprite;
    public Button NextButton;
    public Button PreviousButton;
    public Text PageText;
    private Image _image;
    private Dictionary<string, GameObject> _inventorySlots = new Dictionary<string, GameObject>();
    private int _currentScrollPosition = 0;
    private bool _isOpen = false;
    private RectTransform _rectTransform;
    private int _numberOfRows = 3;
    private int _currentPage = 0;
    private int _numberOfPages = 3;

    void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        GameManager.Instance.OnAddItemToInventory += OnItemAddedToInventory;
        GameManager.Instance.OnRemoveItemFromInventory += OnItemRemovedFromInventory;
        // TODO: subscribe to scroll event

        GameManager.Instance.OnGameStateChange += OnGameStateChanged;
        _image = GetComponent<Image>();
    }

    void Start()
    {
        for (int i = 0; i < 15; i++)
        {
            GameObject o = Instantiate<GameObject>(EmptyInventorySlotPrefab, Vector3.zero, Quaternion.identity, transform);
            o.transform.localPosition = new Vector3(10 + ((64 + 8) * (i % _numberOfRows)), -10 - ((64 + 8) * (i / _numberOfRows)));
        }
        CraftButton.gameObject.SetActive(false);
        CraftCloseButton.gameObject.SetActive(false);
    }

    private void OnGameStateChanged(GameState newState)
    {
        if (newState == GameState.Inspect)
        {
            CraftButton.gameObject.SetActive(true);
            CraftCloseButton.gameObject.SetActive(true);
            EnableCrafting(false);
        }
        else
        {
            CraftButton.gameObject.SetActive(false);
            CraftCloseButton.gameObject.SetActive(false);
        }

        if (newState == GameState.Adventure)
        {
            MenuButton.gameObject.SetActive(true);
        }
        else
        {
            MenuButton.gameObject.SetActive(false);
        }
    }

    public void EnableCrafting(bool enabled)
    {
        CraftButton.interactable = enabled;
    }

    public void OpenInventory(Inventory inventory)
    {
        OnInventoryCleared();
        foreach (Item item in inventory)
        {
            OnItemAddedToInventory(item);
        }
    }

    private void OnItemAddedToInventory(Item item)
    {
        GameObject inventorySlot = Instantiate<GameObject>(InventorySlotPrefab, Vector3.zero, Quaternion.identity, transform);
        //inventorySlot.GetComponent<RectTransform>().localPosition = GetPosition(_inventorySlots.Count, _currentScrollPosition);
        inventorySlot.GetComponent<Image>().sprite = item.icon;
        ItemSlot itemSlot = inventorySlot.GetComponent<ItemSlot>();
        itemSlot.Item = item;
        itemSlot.Canvas = Canvas;
        _inventorySlots.Add(item.title, inventorySlot);
        _numberOfPages = ((_inventorySlots.Count - 1) / 15) + 1;
        PageText.text = $"{_currentPage + 1}/{_numberOfPages}";
        if (_currentPage < _numberOfPages - 1)
        {
            NextButton.interactable = true;
        }
        ReorderInventory();

        if (item.title == "Klemmbrett")
        {
            GameManager.Instance.ShowHint("Irgendetwas scheint zwischen dem Brett und der Liste zu sein, ich sollte es auseinanderbauen und es mir genauer anschauen. Dazu muss ich das Inventar öffnen, das Klemmbrett antippen und dann kräftig schütteln..");
        }
        else if (item.title == "Stift")
        {
            GameManager.Instance.ShowHint("Ich sollte mir den wohl am besten direkt zusammen mit der List am Klemmbrett befestigen. Dazu muss ich eines dieser drei Dinge im Inventar antippen und dann die anderen beiden dazu legen..");
        }
        else if (item.title == "Archivraumschlüssel")
        {
            GameManager.Instance.ShowHint("Ein alter Schlüssel, passt sicherlich zur Archivtür. Ich sollte ihn aus dem Inventar nehmen und in das Schloss stecken..");
        }
    }

    private void OnItemRemovedFromInventory(Item item)
    {
        GameObject inventorySlot;
        if (_inventorySlots.TryGetValue(item.title, out inventorySlot))
        {
            Destroy(inventorySlot);
        }
        _inventorySlots.Remove(item.title);
        _numberOfPages = ((_inventorySlots.Count - 1) / 15) + 1;
        PageText.text = $"{_currentPage + 1}/{_numberOfPages}";
        if(_currentPage >= _numberOfPages)
        {
            _currentPage = _numberOfPages - 1;
            NextButton.interactable = false;
            if (_currentPage == 0)
            {
                PreviousButton.interactable = false;
            }
        }
        ReorderInventory();
    }

    private void ReorderInventory()
    {
        int i = 0;
        foreach (GameObject item in _inventorySlots.Values)
        {
            item.GetComponent<RectTransform>().localPosition = GetPosition(i);
            i++;
        }
    }

    private void OnInventoryCleared()
    {
        _inventorySlots.Clear();
        _currentPage = 0;
        NextButton.interactable = false;
        PreviousButton.interactable = false;
    }

    private Vector3 GetPosition(int i)
    {
        if (i / 15 != _currentPage)
        {
            return new Vector3(-200, -200);
        }
        i = i - (15 * _currentPage);
        return new Vector3(10 + ((64 + 8) * (i % _numberOfRows)), -10 - ((64 + 8) * (i / _numberOfRows)));
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!_isOpen)
        {
            if (OnInventoryOpened != null)
            {
                OnInventoryOpened(true);
            }
            _isOpen = true;
            _rectTransform.anchoredPosition = new Vector2(0, 0);
            _image.sprite = OpenedSprite;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 newPosition = new Vector2(_rectTransform.anchoredPosition.x + (eventData.delta.x / Canvas.scaleFactor), _rectTransform.anchoredPosition.y);
        if (newPosition.x < -220)
        {  
            newPosition.x = -220;
        }
        else if (newPosition.x > 0)
        {
            newPosition.x = 0;
        }
        _rectTransform.anchoredPosition = newPosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!_isOpen && _rectTransform.anchoredPosition.x > -215)
        {
            if (OnInventoryOpened != null)
            {
                OnInventoryOpened(true);
            }
            _isOpen = true;
            _image.sprite = OpenedSprite;
        }
        else if (_isOpen && _rectTransform.anchoredPosition.x < -35)
        {
            if (GameManager.Instance.CurrentGameState == GameState.Inspect)
            {
                CloseCrafting();
            }
            if (OnInventoryOpened != null)
            {
                OnInventoryOpened(false);
            }
            _isOpen = false;
            _image.sprite = ClosedSprite;
        }
        
        if (_isOpen)
        {
            _rectTransform.anchoredPosition = new Vector2(0, 0);
        }
        else
        {
            _rectTransform.anchoredPosition = new Vector2(-220, 0);
        }
    }

    public void CloseCrafting()
    {
        if (OnCraftingOpened != null)
        {
            OnCraftingOpened(false);
        }
    }

    public void Craft()
    {
        if (OnCraft != null)
        {
            OnCraft();
        }
    }

    public void OpenMenu()
    {

    }

    public void NextPage()
    {
        _currentPage++;
        ReorderInventory();
        PageText.text = $"{_currentPage + 1}/{_numberOfPages}";
        if (_currentPage == _numberOfPages - 1)
        {
            NextButton.interactable = false;
        }
        if (_currentPage != 0)
        {
            PreviousButton.interactable = true;
        }
    }

    public void PreviousPage()
    {
        _currentPage--;
        ReorderInventory();
        PageText.text = $"{_currentPage + 1}/{_numberOfPages}";
        if (_currentPage == 0)
        {
            PreviousButton.interactable = false;
        }
        if (_currentPage != _numberOfPages - 1)
        {
            NextButton.interactable = true;
        }
    }
}
