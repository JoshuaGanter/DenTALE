using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void OnInteraction();

public class Interactable : MonoBehaviour
{
    public string Name;
    public event OnInteraction InteractWithEvent;

    public virtual void InteractWith()
    {
        Debug.Log($"[{Name}] interaction without item.");
        // TODO: Find a way to implement an interaction based on someth
    }

    public virtual void InteractWith(Item item)
    {
        Debug.Log($"[{Name}] interaction with {item.title}");
    }
}
