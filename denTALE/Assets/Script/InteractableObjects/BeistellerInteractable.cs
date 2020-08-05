using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeistellerInteractable : Interactable
{
    public override void InteractWith()
    {
        GameManager.Instance.ShowHint("Da stimmt was nicht..");
    }

    public override void InteractWith(Item item)
    {
        Debug.Log($"[{Name}] interaction with {item.title}");
    }
}
