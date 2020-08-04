using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public delegate void RotateItemInInspector(float amount);
public delegate void ClickInScene(Vector2 position);

public class ClickPlane : MonoBehaviour, IPointerClickHandler, IDragHandler
{
    public static event RotateItemInInspector OnRotateItemInInspector;
    public static event ClickInScene OnClickInScene;

    public void OnDrag(PointerEventData eventData)
    {
        if (GameManager.Instance.CurrentGameState == GameState.Inspect)
        {
            if (OnRotateItemInInspector != null)
            {
                OnRotateItemInInspector(eventData.delta.x);
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!eventData.dragging)
        {
            if (OnClickInScene != null)
            {
                OnClickInScene(eventData.position);
            }
        }
    }
}
