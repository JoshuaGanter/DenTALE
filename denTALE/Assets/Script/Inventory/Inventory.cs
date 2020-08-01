using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Inventory", menuName = "Inventory/Inventory")]
public class Inventory : ScriptableObject
{
    private List<Item> _container = new List<Item>();

    public int Count
    {
        get
        {
            return _container.Count;
        }
    }

    public void AddItem(Item item)
    {
        _container.Add(item);
    }

    public void AddItems(Item[] items)
    {
        _container.AddRange(items);
    }

    public void RemoveItem(Item item)
    {
        _container.Remove(item);
    }
}
