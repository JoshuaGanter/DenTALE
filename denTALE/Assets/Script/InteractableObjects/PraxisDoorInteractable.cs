using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PraxisDoorInteractable : Interactable
{
    public GameObject Ofen;
    public GameObject Gebiss;
    public override void InteractWith()
    {
        if (Ofen.GetComponent<PraxisOvenInteractable>().IsExploded || Gebiss.GetComponent<PraxisGebissInteractable>().Cleaned)
        {
            GameManager.ScenesDone[0] = true;
            GameManager.Instance.ShowHint("Na dann stelle ich den Code mal am Zahlenschloss ein.. Es öffnet sich tatsächlich. Nichts wie raus hier.");
            StartCoroutine(GameManager.Instance.SwitchScenes((int) GameScene.Archiv));
        }
        else
        {
            GameManager.Instance.ShowHint("Das muss die Tür hier raus sein, aber das Zahlenschloss sieht stabil aus. Ich muss wohl einen Weg finden es zu öffnen. Vielleicht finde ich ja den Code in der Nähe.");
        }
    }
}
