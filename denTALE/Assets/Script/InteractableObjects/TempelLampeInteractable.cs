using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempelLampeInteractable : Interactable
{
    public Animator Animator;
    private bool _destroyed = false;
    public override void InteractWith()
    {
        if (!_destroyed)
        {
            GameManager.Instance.ShowHint("Diese Lampe sieht anders aus als die restlichen. Etwas scheint darin zu sein, vielleicht finde ich eine Möglichkeit es raus zu bekommen..");
        }
    }

    public override void InteractWith(Item item)
    {
        if (!_destroyed)
        {
            GameManager.Instance.ShowHint("Das hat ordentlich geknallt, jetzt muss ich dieses fliegende Ding nur noch irgendwie in die Finger bekommen..");
            _destroyed = true;
            Animator.SetBool("Flying_Orb", true);
        }
    }
}
