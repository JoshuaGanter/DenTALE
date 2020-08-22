using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class Item : ScriptableObject
{
    public GameObject prefab;
    public Sprite icon;
    public Item[] consistsOf;
    public string title = "";
    [TextArea(15, 20)]
    public string description;
    public bool PickedUp = false;

    public override bool Equals(object other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        if (other.GetType() != typeof (Item)) return false;
        return this.title == ((Item) other).title;
    }

    public override int GetHashCode()
    {
        return this.title.GetHashCode();
    }
}

public class RecipeComparer : IEqualityComparer<Item[]>
{
    public bool Equals(Item[] x, Item[] y)
    {
        if (x.Length != y.Length) return false;
        for(int i = 0; i < x.Length; i++)
        {
            if (!x[i].Equals(y[i])) return false;
        }
        return true;
    }

    public int GetHashCode(Item[] items)
    {
        int hash = 0;
        foreach(Item item in items)
        {
            hash += item.GetHashCode();
        }
        return hash;
    }
}