using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Joystick : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler
{
    private Image _bgImage;
    private Image _joystickImage;
    private Vector3 _inputVector;

    public float Horizontal
    {
        get
        {
            return _inputVector.x;
        }
    }

    public float Vertical
    {
        get
        {
            return _inputVector.z;
        }
    }

    public void Start()
    {
        _bgImage = GetComponent<Image>();
        _joystickImage = transform.GetChild(0).GetComponent<Image>();
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 pos;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_bgImage.rectTransform, eventData.position, eventData.pressEventCamera, out pos))
        {
            pos.x = (pos.x / _bgImage.rectTransform.sizeDelta.x);
            pos.y = (pos.y / _bgImage.rectTransform.sizeDelta.y);

            _inputVector = new Vector3(pos.x * 2 + 1, 0, pos.y * 2 - 1);
            _inputVector = (_inputVector.magnitude > 1) ? _inputVector.normalized : _inputVector;

            _joystickImage.rectTransform.anchoredPosition = new Vector3(_inputVector.x * (_bgImage.rectTransform.sizeDelta.x/3), _inputVector.z * (_bgImage.rectTransform.sizeDelta.y/3));
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OnDrag(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _inputVector = Vector3.zero;
        _joystickImage.rectTransform.anchoredPosition = Vector3.zero;
    }
}
