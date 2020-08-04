using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public delegate void ItemClicked(Item item);
public delegate void ItemDragged(Item item);

public class ItemSlot : MonoBehaviour, IPointerClickHandler, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    public static event ItemClicked OnItemClicked;
    public static event ItemDragged OnItemDragged;
    public Item Item;
    public Canvas Canvas;
    private Vector2 _startPosition;
    private RectTransform _rectTransform;

    public void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
    }

    public void OnDrag(PointerEventData eventData)
    {
        _rectTransform.anchoredPosition += eventData.delta / Canvas.scaleFactor;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        _startPosition = _rectTransform.anchoredPosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (GameManager.Instance.CurrentGameState == GameState.Inventory)
        {
            Ray ray = Camera.main.ScreenPointToRay(eventData.position);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                GameObject gameObject = hit.transform.gameObject;
                if (gameObject.tag == "InteractableObject")
                {
                    gameObject.GetComponent<Interactable>()?.InteractWith(Item);
                }
            }
        }
        else if (GameManager.Instance.CurrentGameState == GameState.Inspect)
        {
            if (!RectTransformUtility.RectangleContainsScreenPoint(gameObject.GetComponentInParent<RectTransform>(), eventData.pressPosition))
            {
                if (OnItemDragged != null)
                {
                    OnItemDragged(Item);
                }
            }
        }
        _rectTransform.anchoredPosition = _startPosition;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!eventData.dragging && OnItemClicked != null)
        {
            OnItemClicked(Item);
        } 
    }
}
