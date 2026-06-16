using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class CardDrag : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{

    Card _card;
    bool _dragging;
    Transform _objectToDrag;
    Vector2 _oldPosition;
    Vector3 _savedPosition;
    void Awake()
    {
        _card = GetComponentInParent<Card>();
        _objectToDrag = this.transform.parent.parent;
    }

    void Update()
    {
        if (_dragging)
        {
            _objectToDrag.position = Mouse.current.position.ReadValue();
        }
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        _dragging = true;
        _oldPosition = eventData.position = new Vector2(_objectToDrag.position.x, _objectToDrag.position.y);
        _savedPosition = _card.Rect.anchoredPosition3D;
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        _dragging = false;
        EventSystem.current.SetSelectedGameObject(null);
        if (_oldPosition.y < _objectToDrag.position.y - (_objectToDrag as RectTransform).rect.height * _objectToDrag.localScale.y)
        {
            CardsController.Instance.Play(_card);
            CardsController.Instance.AfterPlay(_card);
        }
        else
        {
            //_objectToDrag.position = _oldPosition;
            _card.Move(_savedPosition, 0.2f, () => {});
        }
    }
}
