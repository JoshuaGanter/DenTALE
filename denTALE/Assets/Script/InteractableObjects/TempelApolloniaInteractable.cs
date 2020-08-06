using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempelApolloniaInteractable : Interactable
{
    public override void InteractWith()
    {
        GameManager.Instance.ShowHint("Apollonia: Hallo Reisender, ich bin Apollonia, die Göttin, der Zahnheilkundigen und habe dich hierher gebracht, damit du mir meine Seele, meinen Orb zurück bringst. Er ist mir kürzlich aus dem Schrein ausgebüchst und kann noch nicht weit sein. Setze ihn wieder dort ein und ich lasse dich zurück in deine Welt.");
    }

    public override void InteractWith(Item item)
    {
        if (item.title == "Waffe")
        {
            GameManager.Instance.ShowHint("Apollonia: Du elendiger Schänder, so mögen deine Zähne in deinem Mund faulen und du eines erbärmlichen Todes sterben, AHGHHH..");
            GameManager.Instance.SwitchScenes((int) GameScene.Archiv);
        }
        else if (item.title == "Schreinsockel")
        {
            GameManager.Instance.ShowHint("Apollonia: Ich danke dir, Reisender. Gehe nun dorthin zurück, wo du herkamst.");
            GameManager.Instance.SwitchScenes((int) GameScene.Archiv);
        }
    }
}
