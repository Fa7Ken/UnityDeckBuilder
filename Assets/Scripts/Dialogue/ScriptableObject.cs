using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewDialogue", menuName = "Dialogue System/New Dialogue")]
public class Dialogue : ScriptableObject
{
    // esse código é para permitir que seja possivel criar os objetos dos dialogos
    // (o arquivo onde coloca as linhas)
    public DialogueLine[] lines; // Array de falas
}
