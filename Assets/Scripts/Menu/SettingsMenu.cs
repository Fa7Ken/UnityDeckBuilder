using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SettingsMenu : MonoBehaviour
{
    [SerializeField] AudioMixer audioMixer; // Audio Mixer que é o objeto para controlar os audios do jogo

    // Atravez do AudioMixer voce controla o volume do jogo
    public void SetVolume(float _volume)
    {
        audioMixer.SetFloat("Volume", _volume);
    }

    // Aqui controla a qualidade do jogo conforme o PreSet do Unity (nao mexi em outros detalhes para fazer manualmente)
    public void SetQuality(int _qualityIndex)
    {
        QualitySettings.SetQualityLevel(_qualityIndex);
    }

    // Ativa/desativa a tela cheia (so funciona quando criar o executavel do jogo, nao tem efeito enquanto na engine do Unity)
    public void SetFullScreen(bool _isFullScreen)
    {
        Screen.fullScreen = _isFullScreen;
    }

    // Fecha o jogo (so funciona quando criar o executavel do jogo, nao tem efeito enquanto na engine do Unity)
    public void Quit()
    {
        Application.Quit();
    }
}
