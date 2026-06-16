using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CardsController : MonoBehaviour
{
    #region Fields/Properties
    public static CardsController Instance;
    public CardHolder hand;
    public CardHolder deck;
    public CardHolder discardPile;

    #endregion
    #region Card Controls
    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        ShuffleCardHolder(deck);
        StartCoroutine(DrawCard(5));
    }

    public IEnumerator DrawCard(int amount = 1)
    {
        while (amount > 0)
        {
            if (deck.cards.Count == 0)
            {
                yield return StartCoroutine(ShuffleDiscardIntoDeck());
                yield return new WaitForSeconds(0.25f);
                if (deck.cards.Count == 0)
                {
                    Debug.Log("Deu ruim, acabou o baralho");
                    yield break;
                }
            }
            Card card = deck.cards[deck.cards.Count - 1];
            deck.RemoveCard(card);
            hand.AddCard(card);
            yield return new WaitForSeconds(0.1f);
            amount--;
        }
    }

    public void Discard(Card card)
    {
        hand.RemoveCard(card);
        deck.RemoveCard(card);
        discardPile.AddCard(card);
    }

    public IEnumerator ShuffleDiscardIntoDeck()
    {
        List<Card> cards = discardPile.cards;
        System.Random rand = new System.Random();
        List<Card> shufled = new List<Card>(cards.OrderBy(x => rand.Next()).ToList()); //embaralhando e ordenando
        foreach (Card card in shufled)
        {
            discardPile.RemoveCard(card);
            deck.AddCard(card);
            yield return new WaitForSeconds(0.1f);
        }
    }

    public void ShuffleCardHolder(CardHolder cardHolder)
    {
        System.Random rand = new System.Random();

        // Cria uma nova lista embaralhada a partir da lista de cartas
        List<Card> shuffled = cardHolder.cards.OrderBy(x => rand.Next()).ToList();

        // Atualiza a lista de cartas no próprio cardHolder
        cardHolder.cards = shuffled;
    }

    /*public void CurveHand()
    {
        
        int angle = 3 * hand.cards.Count; //calcula metade do angulo para jogar nas cartas e o meio zerar
        int position = 8 * hand.cards.Count;
        int middleHand = 0;
        int curve = 0; //variavel para servir como numero de incremento
        int rangeY = 0;
        if (hand.cards.Count % 2 != 0) //verifica se o numero for impar
        {
            angle -= 3; //retira um dos acrescimos caso for impar, para a carta do meio ficar reta
            position += 8;
        }
        List<Card> cards = hand.cards; //monta uma lista com as cartas da mão 
        foreach (Card card in cards) // cria um loop para alterar o angulo em cada carta individualmente
        {
            int angleZ = angle + curve; //soma o angulo total das cartas com o angulo variavel
            int positionY = position + rangeY;
            if (positionY > 0)
            {
                positionY *= -1;
            }
            RectTransform rect = card.transform as RectTransform; //pega a carta e cria uma variavel só com o rectTransform dela
            Vector3 targetRotation = rect.localEulerAngles; //cria uma variavel com os angulos do rect da carta
            Vector3 targetPosition = rect.position; //cria uma variavel com as posição do rect da carta
            targetRotation.z = angleZ; // altera o eixo z do rect transform
            targetPosition.y = 240 + positionY;
            rect.localEulerAngles = targetRotation; // aplica a alteração na carta
            rect.position = targetPosition;

            rangeY -= 16;
            curve -= 6; //varia o angulo para ser alterado em cada carta
            if (hand.cards.Count % 2 == 0 && positionY == 0 && middleHand == 0)
            {
                rangeY += 16;
                curve += 6;
                middleHand = 1;
                Debug.Log("passei aqui");
            }
        }

    }*/
    #endregion
    #region Cards Events

    public void Play(Card card)
    {
        Transform scriptHolder = card.transform.Find("Effects/Played");
        foreach (ICardEffect effect in scriptHolder.GetComponentsInChildren<ICardEffect>())
        {
            effect.Apply();
        }
    }

    public void AfterPlay(Card card)
    {
        Transform scriptHolder = card.transform.Find("Effects/AfterPlayed");
        foreach (ICardEffect effect in scriptHolder.GetComponentsInChildren<ICardEffect>())
        {
            effect.Apply();
        }
    }
    #endregion
}