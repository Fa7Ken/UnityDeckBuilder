using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tester : MonoBehaviour
{
    public Card card;

    public CardHolder cardHolder;
    [ContextMenu("Draw")]

    public void DrawCard()
    {
        StartCoroutine(CardsController.Instance.DrawCard(5));
    }

    [ContextMenu("Rmv")]
    public void RemoveCard()
    {
        CardsController.Instance.Discard(card);
    }

    [ContextMenu("Shuffled")]
    public void ShuffleDiscard()
    {
        StartCoroutine(CardsController.Instance.ShuffleDiscardIntoDeck());
    }

    [ContextMenu("Play Card")]
    public void PlayCard()
    {
        CardsController.Instance.Play(card);
    }

    [ContextMenu("After Play Card")]
    public void AfterPlay()
    {
        CardsController.Instance.AfterPlay(card);
    }
}
