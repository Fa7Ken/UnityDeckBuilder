using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card : MonoBehaviour
{
    #region Fields/Properties
    public RectTransform Rect{get{ return _rect; } }
    RectTransform _rect;
    Transform _back;
    Transform _front;
    int _movementTween;
    int _rotationTween;
    #endregion
    #region Unity Events
    void Awake()
    {
        _rect = GetComponent<RectTransform>();
        _back = transform.Find("BackCard");
        _front = transform.Find("FrontCard");
    }
    void Update()
    {
        if (_rect.rotation.eulerAngles.y > 90 && _rect.rotation.eulerAngles.y < 270)
        {
            _back.SetAsLastSibling();
        }
        else
        {
            _front.SetAsLastSibling();
        }
    }

    #endregion
    #region Methods

    public void Move(Vector3 position, float duration, System.Action onComplete)
    {
        if (LeanTween.isTweening(this.gameObject) && LeanTween.isTweening(_movementTween))
        {
            LeanTween.cancel(_movementTween);
        }

        _movementTween = _rect.LeanMove(position, duration).setOnComplete(onComplete).id;

    }

    public void Rotate( float amount, float duration)
    {
        if (LeanTween.isTweening(this.gameObject) && LeanTween.isTweening(_rotationTween))
        {
            LeanTween.cancel(_rotationTween);
        }

        _rotationTween = _rect.LeanRotateAroundLocal(Vector3.up, amount, duration).id;

    }
    #endregion
}