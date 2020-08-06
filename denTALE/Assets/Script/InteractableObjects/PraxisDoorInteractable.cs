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
            StartCoroutine(GameManager.Instance.SwitchScenes((int) GameScene.Archiv));
        }
    }
}
