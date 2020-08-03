using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public delegate void OpenInventory(bool isOpen);
public delegate void OpenCrafting(bool isOpen);

public class DisplayInventory : MonoBehaviour, IPointerClickHandler, IDragHandler, IEndDragHandler
{
    public static event OpenInventory OnInventoryOpened;
    public static event OpenCrafting OnCraftingOpened;
    public GameObject InventorySlotPrefab;
    public Canvas Canvas;
    public Button MenuButton;
    public Button CraftButton;
    public Button CraftCloseButton;
    private Dictionary<string, GameObject> _inventorySlots = new Dictionary<string, GameObject>();
    private int _currentScrollPosition = 0;
    private bool _isOpen = false;
    private RectTransform _rectTransform;

    void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
    }

    void Start()
    {
        GameManager.Instance.OnAddItemToInventory += OnItemAddedToInventory;
        GameManager.Instance.OnRemoveItemFromInventory += OnItemRemovedFromInventory;
        // TODO: subscribe to scroll event

        GameManager.Instance.OnGameStateChange += OnGameStateChanged;

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
        ReorderInventory();
    }

    private void OnItemRemovedFromInventory(Item item)
    {
        GameObject inventorySlot;
        if (_inventorySlots.TryGetValue(item.title, out inventorySlot))
        {
            Destroy(inventorySlot);
        }
        _inventorySlots.Remove(item.title);
        ReorderInventory();
    }

    private void ReorderInventory()
    {
        int i = 0;
        foreach (GameObject item in _inventorySlots.Values)
        {
            item.GetComponent<RectTransform>().localPosition = GetPosition(i, _currentScrollPosition);
            i++;
        }
    }

    private void OnInventoryCleared()
    {
        _inventorySlots.Clear();
    }
    
    private void OnInventoryScrolled(int newScrollPosition)
    {
        int i = 0;
        foreach(GameObject inventorySlot in _inventorySlots.Values)
        {
            inventorySlot.GetComponent<RectTransform>().localPosition = GetPosition(i, newScrollPosition);
            i++;
        }
    }

    private Vector3 GetPosition(int i, int scrollPosition)
    {
        return new Vector3(10 + (110 * (i % 2)), -10 - (110 * (i / 2)) - scrollPosition);
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
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 newPosition = new Vector2(_rectTransform.anchoredPosition.x + (eventData.delta.x / Canvas.scaleFactor), _rectTransform.anchoredPosition.y);
        if (newPosition.x < -235)
        {  
            newPosition.x = -235;
        }
        else if (newPosition.x > 0)
        {
            newPosition.x = 0;
        }
        _rectTransform.anchoredPosition = newPosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!_isOpen && _rectTransform.anchoredPosition.x > -200)
        {
            if (OnInventoryOpened != null)
            {
                OnInventoryOpened(true);
            }
            _isOpen = true;
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
        }
        
        if (_isOpen)
        {
            _rectTransform.anchoredPosition = new Vector2(0, 0);
        }
        else
        {
            _rectTransform.anchoredPosition = new Vector2(-235, 0);
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

    }

    public void OpenMenu()
    {

    }
}
