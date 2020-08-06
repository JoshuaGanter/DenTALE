using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PraxisGebissInteractable : Interactable
{
    private bool _isWet = false;
    public bool Cleaned = false;
    public Animator Animator;

    public override void InteractWith()
    {
        if (!_isWet)
        {
            GameManager.Instance.ShowHint("Sehr eklig, aber unter all dem Schmodder scheint etwas in das Modell geritzt zu sein..");
        }
        else if (!Cleaned) {
            GameManager.Instance.ShowHint("Jetzt ist der Dreck eingeweicht, ich muss ihn nur noch irgendwie runterkratzen..");
        }
    }

    public override void InteractWith(Item item)
    {
        if (item.title == "Mundspülung" && !_isWet)
        {
            GameManager.Instance.ShowHint("In der Flasche ist nicht mehr viel drin. Wenn ich das Modell ordentlich einweichen will, muss ich eine Möglichkeit finden die Mundspülung gezielter aufzutragen..");
        }
        else if (item.title == "Sprühflasche" && !_isWet)
        {
            GameManager.Instance.ShowHint("Das funktioniert gut, nun brauche ich nur noch etwas Stabiles um den Dreck vorsichtig runter zu kratzen..");
            _isWet = true;
            Animator.SetBool("Spray", true);
        }
        else if (item.title == "Pelikan" && !Cleaned)
        {
            Debug.Log("clean gebiss");
            GameManager.Instance.ShowHint("Damit sollte ich den Dreck runter bekommen.. Perfekt und da steht auch tatsächlich ein Code auf dem Modell. Den merk ich mir!");
            Debug.Log("gebiss cleaned");
            Cleaned = true;
            Debug.Log("start animation");
            Animator.SetBool("Kratzen", true);
            Debug.Log("animation started");
        }
    }
}
