using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuFadeController : MonoBehaviour
{
    private FadeUI fadeUI; // Codigo para fazer o efeito de fade
    [SerializeField] private float fadeTime; // Tempo para fazer o fade (esmaecer)

    //  Inicia o codigo do FadeUI e ativa a funcao do efeito de Fade OUT
    void Start()
    {
        fadeUI = GetComponent<FadeUI>();
        fadeUI.FadeUIOut(fadeTime);
    }

    // Funcao que chama a corrotina com o proposito de ativar por um botao (so da para chamar funcao em botao) 
    public void CallFadeAndStartGame(string _sceneToLoad)
    {
        StartCoroutine(FadeAndStartGame(_sceneToLoad));
    }

    // Corrotina para ativar o efeito de Fade IN e mudar de tela saindo do menu para a cena do jogo
    public IEnumerator FadeAndStartGame(string _sceneToLoad)
    {
        fadeUI.FadeUIIn(fadeTime); // Ativa o efeito de Fade IN com o tempo definido no fadeTime
        yield return new WaitForSeconds(fadeTime); // Aguarda o tempo do fadeTime
        SceneManager.LoadScene(_sceneToLoad); // Muda pra cena do jogo
    }
}
