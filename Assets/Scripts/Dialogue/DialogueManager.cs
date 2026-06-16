using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;


public class DialogueManager : MonoBehaviour
{
    [Header("Variaveis do Dialogo")]
    public RectTransform panel; // Arraste o painel de dialogo no Inspector
    public TextMeshProUGUI nameText;  // Nome do personagem
    public TextMeshProUGUI dialogueText; // Texto do dialogo
    public Image backGround; //fundo da tela
    public Image charLeft; //personagem que fica do lado esquerdo da tela
    public Image charRigth; //personagem que fica do lado direito da tela
    public Image nextImage; // Imagem para avancar o texto
    public float typingSpeed = 0.05f; // Velocidade do efeito de digitacao

    public float currentTimeNextImage = 0f;
    public bool increasingNextImage = true;




    [Space(5)]
    [Header("Variaveis De Controle")]
    private Dialogue currentDialogue; //grupo de falas atual
    private int currentLineIndex; // indice da linha atual
    private bool isTyping = false; //se esta digitando a linha ainda
    private Coroutine typingCoroutine; //iniciar a co-rotina de digitacao
    Animator anim;



    void Awake()
    {
        // iniciando todas as variaveis de forma forcada para evitar que elas nao sejam chamadas,
        // nao e obrigatorio, mas e bom quando for chamar o dialogue manager para outros codigos
        if (panel == null)
        {
            panel = GameObject.Find("Painel de Dialogo")?.GetComponent<RectTransform>();
        }
        if (nameText == null)
        {
            nameText = GameObject.Find("Name")?.GetComponent<TextMeshProUGUI>();
        }
        if (dialogueText == null)
        {
            dialogueText = GameObject.Find("Dialogue")?.GetComponent<TextMeshProUGUI>();
        }
    }

    void Start()
    {
        panel.localScale = Vector3.zero; // Comeca invisivel
        nextImage.gameObject.SetActive(false); //desativa o botao de avancar
    }

    void Update()
    {
        
        if (panel.localScale != Vector3.zero) //so ativa se o painel estiver ativo, para evitar bugs de
                                              //chamar a acao da tecla mesmo nao estando em dialogo
        {
            // Verifica se qualquer tecla foi pressionada
            FadeNext();
            if (Input.anyKeyDown)
            {
                if (isTyping) //se estiver digitando completa o texto
                {
                    CompleteTextInstantly();
                }
                else
                {
                    nextImage.gameObject.SetActive(false);
                    DisplayNextLine();
                }
            }
        }
    }

    public void StartDialogue(Dialogue dialogue) //funcao usada principalmente por outros scripts
    {
        //caso nao tenha dialogo anexado no script que chamou essa funcao, aparece este erro no console
        if (dialogue == null)
        {
            Debug.LogError("StartDialogue recebeu um dialogo nulo!");
            return;
        }
        currentDialogue = dialogue; //comeca alimentar as variaveis no DialogueManager
        currentLineIndex = 0; //zera o index para comecar o dialogo desde o inicio
        panel.localScale = Vector3.one; //ativa o painel do dialogo que foi desativado no inicio do script
        DisplayNextLine(); //funcao de avancar o dialogo
    }

    public void DisplayNextLine()
    {
        if (currentDialogue == null || currentDialogue.lines == null || currentDialogue.lines.Length == 0)
        {
            Debug.LogWarning("Nenhum dialogo ativo. Ignorando o clique.");
            return; //Impede que o codigo continue e evita o erro caso nao tenha dialogo ativa
        }

        if (currentLineIndex < currentDialogue.lines.Length) //verifica se o tamanho das linha atual do
                                                             //dialogo e menor que o numero maximo de dialogos
        {
            //puxa todas as informacoes da linha de dialogo, nome, texto, quadro da imagem falando e comeca digitar
            DialogueLine line = currentDialogue.lines[currentLineIndex];

            nameText.text = line.characterName;
            backGround.sprite = line.backGround;
            charLeft.sprite = line.characterLeft;
            charRigth.sprite = line.characterRight;
            nextImage.gameObject.SetActive(false);

            typingCoroutine = StartCoroutine(TypeText(line.text));

            currentLineIndex++;

        }
        else //caso o numero de linhas seja igual ou maior (nao provavel) ele chama a funcao para encerrar o dialogo
        {
            EndDialogue();
        }
    }

    IEnumerator TypeText(string text) // co-rotina de digitacao da fala, caso tenha botoes ativa eles
    {
        isTyping = true; // coloca a variavel booleana em verdade para informa que esta digitando
        dialogueText.text = ""; // inicia a variavel para escrever texto

        foreach (char letter in text)// verifica a quantidade de letras no texto da linha do dialogo
        {
            dialogueText.text += letter; // adiciona uma letra por vez na variavel iniciada
            yield return new WaitForSeconds(typingSpeed); // aguarda o tempo da variavel para chamar a proxima letra
        }

        isTyping = false; // quando termina o loop de repeticao transforma a booleana em falsa

        nextImage.gameObject.SetActive(true);
    }

    void CompleteTextInstantly()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine); // Para a corrotina da digitacao
        }

        dialogueText.text = currentDialogue.lines[currentLineIndex - 1].text; // Mostra o texto inteiro
        isTyping = false;

        nextImage.gameObject.SetActive(true);
    }
    public void FadeNext()
    {

        currentTimeNextImage += (increasingNextImage ? 1 : -1) * 1f * Time.deltaTime;     
        currentTimeNextImage = Mathf.Clamp01(currentTimeNextImage);
                
        float alpha = Mathf.Lerp(0f, 1f, (Mathf.Sin(currentTimeNextImage * Mathf.PI * 2 - Mathf.PI/2) + 1) / 2);

        Color color = nextImage.color;
        color.a = alpha;
        nextImage.color = color;
        if (currentTimeNextImage >= 1f) increasingNextImage = false;
        if (currentTimeNextImage <= 0f) increasingNextImage = true;
    }
    public void EndDialogue()
    {
        panel.localScale = Vector3.zero; //desativa o painel de dialogo
        nextImage.gameObject.SetActive(false); //desativa a imagem  de avancar texto
    }
}