using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void OnInteraction();

public class Interactable : MonoBehaviour
{
    public string Name;
    public event OnInteraction InteractWithEvent;

    public virtual void InteractWith() { }

    public virtual void InteractWith(Item item) { }
}
