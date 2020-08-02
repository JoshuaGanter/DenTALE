using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemSlot : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    public Item Item;
    private Vector3 _startPosition;
    private Vector2 _clickOffset;

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position - _clickOffset;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        _startPosition = transform.position;
        _clickOffset = eventData.position - new Vector2(_startPosition.x, _startPosition.y);
    }

    public void OnEndDrag(PointerEventData eventData)
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
        transform.position = _startPosition;
    }
}
