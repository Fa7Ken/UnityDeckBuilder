using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DialogueLine
{
    // Elementos presentes no objeto de armazenamento do dialogos
    public string characterName;  // Nome do personagem
    public Sprite backGround; // Imagem do fundo
    public Sprite characterLeft; // Imagem do personagem a direita da tela
    public Sprite characterRight; // Imagem do personagem a esquerda da tela
    [TextArea(3, 10)] public string text; // Texto do di�logo
}