using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Artifact : MonoBehaviour
{
    public bool isCursed = true;
    public GameScene toScene = GameScene.Archiv;
    public Item Item;

    public void Start()
    {
        if (isCursed && GameManager.ScenesDone[0] && toScene == GameScene.Praxis)
        {
            Destroy(gameObject);
        }
        else if (isCursed && GameManager.ScenesDone[1] && toScene == GameScene.Tempel)
        {
            Destroy(gameObject);
        }
    }
}
