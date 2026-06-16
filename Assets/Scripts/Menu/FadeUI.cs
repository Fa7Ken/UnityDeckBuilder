using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeUI : MonoBehaviour
{
    CanvasGroup canvasGroup; //Pega o CanvasGroup (componente) no objeto que tem esse script

    // Inicia o CanvasGroup no Awake para garantir que quando outros códigos procurem ele ja esteja iniciado
    // Awake inicia antes dos Starts dos outros codigos
    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    //Funcao chamando a corrotina (com proposito de adicionar no botao, ja que so da para usar funcao em botao)
    public void FadeUIOut(float _seconds)
    {
        StartCoroutine(FadeOut(_seconds));
    }

    //Funcao chamando a corrotina (com proposito de adicionar no botao, ja que so da para usar funcao em botao)
    public void FadeUIIn(float _seconds)
    {
        StartCoroutine(FadeIn(_seconds));
    }

    // Corrotina para fazer o efeito de Fade OUT no menu
    IEnumerator FadeOut(float _seconds)
    {
        canvasGroup.interactable = false; // Desativa a interacao do CanvasGroup
        canvasGroup.blocksRaycasts = false; // Desativa o RayCast (nao sei explicar bem, mas tem a ver com efeito de fisica, colisao e afins)
        canvasGroup.alpha = 1; // Coloca o Canvas visivel no 100%
        while (canvasGroup.alpha > 0) //entra no loop ate o canvas ficar com visibilidade no 0
        {
            canvasGroup.alpha -= Time.unscaledDeltaTime / _seconds; // cada vez que entra no loop, diminui a visibilidade do canvas
            yield return null; //quando o alpha chega no 0 encerra o while
        }
        yield return null; // encerra a corrotina
    }

    // Corrotina para fazer o efeito de Fade IN no menu
    IEnumerator FadeIn(float _seconds)
    {

        canvasGroup.alpha = 0; // Coloca o Canvas visivel no 0%
        while (canvasGroup.alpha < 1) //entra no loop ate o canvas ficar com visibilidade no 1 (100%)
        {
            canvasGroup.alpha += Time.unscaledDeltaTime / _seconds; // cada vez que entra no loop, aumenta a visibilidade do canvas
            yield return null; //quando o alpha chega no 1 encerra o while
        }
        canvasGroup.interactable = true; // Ativa a interacao do CanvasGroup
        canvasGroup.blocksRaycasts = true; // Ativa o RayCast
        yield return null; // encerra a corrotina
    }
}
