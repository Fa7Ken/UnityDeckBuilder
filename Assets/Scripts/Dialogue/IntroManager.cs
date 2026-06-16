using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntroManager : MonoBehaviour
{
    public DialogueManager dialogueManager; // O controle do dialogo 
    public Dialogue dialogo0;
    public bool active = false;
    // Start is called before the first frame update
    public void Start()
    {
        if (dialogueManager == null)
        {
            dialogueManager = GameObject.Find("DialogueManager")?.GetComponent<DialogueManager>();
        }
    }

    // Update is called once per frame
    public void Update()
    {
        if (!active) // Se o activate estiver negativo, acessa aqui para iniciar o dialogo, com o proposito de iniciar no momento que iniciar a cena
        {
            dialogueManager.StartDialogue(dialogo0);
            active = true;
        }

    }
}
