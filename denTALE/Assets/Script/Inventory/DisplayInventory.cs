using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DisplayInventory : MonoBehaviour
{
    public GameObject InventorySlotPrefab;
    private Dictionary<string, GameObject> _inventorySlots = new Dictionary<string, GameObject>();
    private int _currentScrollPosition = 0;

    void Start()
    {
        GameManager.Instance.OnAddItemToInventory += OnItemAddedToInventory;
        GameManager.Instance.OnRemoveItemFromInventory += OnItemRemovedFromInventory;
        GameManager.Instance.OnGameStateChange += OnGameStateChange;
        // TODO: subscribe to scroll event

        this.enabled = false;
    }

    private void OnGameStateChange(GameState newGameState)
    {
        Debug.Log("Game State Changed (in inventory) to " + newGameState.ToString());
        if (newGameState == GameState.Inventory || newGameState == GameState.Inspect)
        {
            this.enabled = true;
        }
        else
        {
            this.enabled = false;
        }
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
        inventorySlot.GetComponent<RectTransform>().localPosition = GetPosition(_inventorySlots.Count, _currentScrollPosition);
        inventorySlot.GetComponent<Image>().sprite = item.icon;
        inventorySlot.GetComponent<Image>().color = Color.green;
        inventorySlot.GetComponent<ItemSlot>().Item = item;
        _inventorySlots.Add(item.title, inventorySlot);
    }

    private void OnItemRemovedFromInventory(Item item)
    {
        GameObject inventorySlot;
        if (_inventorySlots.TryGetValue(item.title, out inventorySlot))
        {
            Destroy(inventorySlot);
        }
        _inventorySlots.Remove(item.title);
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
}
