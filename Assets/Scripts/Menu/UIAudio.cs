using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIAudio : MonoBehaviour
{
    [SerializeField] AudioClip hover, click;
    AudioSource audiosource;

    // Inicia o objeto de audio para tocar os efeitos sonoros nos botoes
    void Start()
    {
        audiosource = GetComponent<AudioSource>();
    }

    // Som para ser executado quando passar pelo botao
    public void SoundOnHover()
    {
        audiosource.PlayOneShot(hover);
    }

    // Som para ser executado quando clicar no botao
    public void SoundOnClick()
    {
        audiosource.PlayOneShot(click);
    }
}
