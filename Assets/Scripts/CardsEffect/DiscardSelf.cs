using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiscardSelf : MonoBehaviour, ICardEffect
{
    public void Apply()
    {
        CardsController.Instance.Discard(GetComponentInParent<Card>());
    }
}
