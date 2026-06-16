using UnityEngine;
using System;

// Estados possiveis de um no no mapa
public enum NodeState { Locked, Available, Visited }

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(CircleCollider2D))]
public class NodeView : MonoBehaviour
{
    // Identidade e metadados do no (usados pela logica externa)
    public string Id;     // guid do no
    public int Layer;     // camada onde o no esta
    public int Column;    // coluna na grade
    public string Kind;   // tipo do no em string (Nightmare, Dream, etc.)

    // Estado visual e de interacao
    public NodeState State = NodeState.Locked;

    // Callback de clique (o ProceduralMapGenerator se inscreve aqui)
    public Action<NodeView> OnClicked;

    // Componentes cacheados
    private SpriteRenderer _sr;
    private CircleCollider2D _col;

    private void Awake()
    {
        // Garante referencias
        _sr = GetComponent<SpriteRenderer>();
        _col = GetComponent<CircleCollider2D>();

        // Collider como trigger para clicks simples (sem fisica)
        _col.isTrigger = true;

        // Raio padrao (ajuste conforme o tamanho do sprite)
        _col.radius = 0.5f;

        // Aplica visual inicial conforme o estado atual
        ApplyVisual();
    }

    // Define o sprite atual deste no (chamado pelo gerador)
    public void SetSprite(Sprite s)
    {
        if (_sr == null) _sr = GetComponent<SpriteRenderer>();
        _sr.sprite = s;
    }

    // Define o estado do no e atualiza o visual
    public void SetState(NodeState s)
    {
        State = s;
        ApplyVisual();
    }

    // Ativa/desativa a interacao (apenas o collider)
    public void SetInteractable(bool v)
    {
        if (_col == null) _col = GetComponent<CircleCollider2D>();
        _col.enabled = v;
        // Nao alteramos alpha para manter opacidade total.
        // O feedback visual vem do ApplyVisual (cores e escala).
    }

    // Ajusta cor/escala conforme o estado
    private void ApplyVisual()
    {
        if (_sr == null) _sr = GetComponent<SpriteRenderer>();

        switch (State)
        {
            case NodeState.Locked:
                // "Oculto" = escuro opaco
                _sr.color = new Color(0.08f, 0.08f, 0.08f, 1f);
                transform.localScale = Vector3.one;
                break;

            case NodeState.Available:
                // Disponivel = branco opaco e leve destaque de escala
                _sr.color = Color.white;
                transform.localScale = Vector3.one * 1.05f;
                break;

            case NodeState.Visited:
                // Visitado = cinza claro (sem transparencia)
                _sr.color = new Color(0.7f, 0.7f, 0.7f, 1f);
                transform.localScale = Vector3.one;
                break;
        }
    }

    // Clique do mouse (funciona em Desktop; em Mobile, EventSystem + Raycaster tambem responde)
    private void OnMouseUpAsButton()
    {
        if (State != NodeState.Available) return;
        OnClicked?.Invoke(this);
    }
}
