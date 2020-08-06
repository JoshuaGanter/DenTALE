using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArchiveDoorInteractable : Interactable
{
    private bool _isOpened = false;
    public Animator Animator;
    public override void InteractWith()
    {
        if (!_isOpened)
        {
            GameManager.Instance.ShowHint("Scheint abgeschlossen.. Wo ist den nur der Schlüssel?");
        }
    }

    public override void InteractWith(Item item)
    {
        if(item.title == "Archivraumschlüssel" && !_isOpened)
        {
            GameManager.Instance.ShowHint("Der passt.. Sesam öffne dich!");
            Animator.SetBool("Door_Oben", true);
            _isOpened = true;
        }
    }
}
