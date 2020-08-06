using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PraxisOvenInteractable : Interactable
{
    private bool _isFilled = false;
    public bool IsExploded = false;
    public Animator Animator;

    public override void InteractWith()
    {
        if (!_isFilled)
        {
            GameManager.Instance.ShowHint("Es scheint eine Art Ofen zu sein, ich sollte ihn mit etwas füllen bevor ich ihn anmache.");
        }
        else if (!IsExploded) {
            GameManager.Instance.ShowHint("Das sollte jetzt gleich schön knallen, besser ich gehe etwas auf Entfernung.");
            IsExploded = true;
            Animator.SetBool("Explode", true);
        }
    }

    public override void InteractWith(Item item)
    {
        if (item.title == "Gasbefüller" && !_isFilled)
        {
            GameManager.Instance.ShowHint("Das Gas sollte ausreichen, um eine orderntliche Explosion zu verursachen.");
            _isFilled = true;
        }
        else if (item.title == "Gaskartusche" && !_isFilled)
        {
            GameManager.Instance.ShowHint("Die ganze Kartusche könnte etwas viel sein, vielleicht kann ich etwas basteln, womit ich Gas in den Ofen füllen kann.");
            _isFilled = true;
        }
    }
}
