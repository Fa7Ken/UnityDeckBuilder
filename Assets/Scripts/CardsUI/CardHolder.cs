using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CardHolder : MonoBehaviour
{
    public List<Card> cards;
    public TextMeshProUGUI cardAmount;
    public RectTransform holder;
    public Vector3 cardRotation; // agora é Vector3 (X, Y, Z) no Inspector

    void Awake()
    {
        cards = new List<Card>(GetComponentsInChildren<Card>());
        cardAmount.text = cards.Count.ToString();
        SetInitialRotation();
    }

    public void AddCard(Card card)
    {
        RectTransform rect = card.transform as RectTransform;

        // posição alvo em world space
        Vector3 targetWorld = holder.position;

        // referencia do holder atual da carta (pode ser null se não estiver em cena)
        CardHolder oldHolder = rect.GetComponentInParent<CardHolder>();

        // ------------------------------
        // ROTACAO: maneira recomendada (absoluta)
        // anima para a rotação alvo (cardRotation) em local space
        LeanTween.rotateLocal(rect.gameObject, cardRotation, 0.6f);
        // ------------------------------

        // movimento em world space — NÃO reparenta antes para evitar "teleporte"
        LeanTween.move(rect.gameObject, targetWorld, 0.4f).setOnComplete(() =>
        {
            // ao terminar o movimento, reparenta e zera posição local
            cards.Add(card);
            rect.SetParent(holder, false);
            rect.localPosition = Vector3.zero;

            // garante rotação final exatamente como configurada
            rect.localRotation = Quaternion.Euler(cardRotation);

            cardAmount.text = cards.Count.ToString();
        });
    }

    public void RemoveCard(Card card)
    {
        cards.Remove(card);
        cardAmount.text = cards.Count.ToString();
    }

    void SetInitialRotation()
    {
        foreach (Card card in cards)
        {
            RectTransform rect = card.transform as RectTransform;
            rect.localRotation = Quaternion.Euler(cardRotation);
        }
    }
}