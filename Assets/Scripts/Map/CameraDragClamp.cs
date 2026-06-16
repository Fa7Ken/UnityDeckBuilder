using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Camera))]
public class CameraDragClamp : MonoBehaviour
{
    [Header("Drag")]
    public float dragSpeed = 1.0f;          // velocidade do arrasto
    public float damping = 10f;             // suavizacao para movimento
    public bool verticalOnly = true;        // arrastar apenas no eixo Y

    [Header("Limites (auto)")]
    public float topMargin = 2.0f;          // margem extra acima do no mais alto
    public float bottomMargin = 2.0f;       // margem extra abaixo do no mais baixo
    public bool computeBoundsOnStart = true; // recalcula bounds no Start

    private Camera cam;
    private float minY, maxY;               // limites absolutos para o centro da camera
    private Vector3 dragOriginScreen;
    private bool dragging;
    private float targetY;

    void Awake()
    {
        cam = GetComponent<Camera>();
    }

    void Start()
    {
        if (computeBoundsOnStart) RecomputeBoundsFromNodes();
        targetY = transform.position.y;
    }

    // Procura todos os NodeView e monta limites com margens
    public void RecomputeBoundsFromNodes()
    {
        // usa API nova quando disponivel; senao, fallback para a antiga
#if UNITY_2023_1_OR_NEWER
        var nodes = Object.FindObjectsByType<NodeView>(FindObjectsSortMode.None);
#else
    var nodes = FindObjectsOfType<NodeView>();
#endif

        if (nodes == null || nodes.Length == 0)
        {
            // fallback: mantém posicao atual como limite
            minY = maxY = transform.position.y;
            return;
        }

        float minNodeY = nodes.Min(n => n.transform.position.y);
        float maxNodeY = nodes.Max(n => n.transform.position.y);

        // altura metade da camera ortografica (assumindo ortho)
        float halfH = cam.orthographic ? cam.orthographicSize : 5f;

        // limites do centro da camera (y) considerando margens e altura da camera
        minY = (minNodeY - bottomMargin) + halfH;
        maxY = (maxNodeY + topMargin) - halfH;

        // se o mapa for menor que a tela, trava no centro do conteudo
        if (minY > maxY)
        {
            float mid = (minNodeY + maxNodeY) * 0.5f;
            minY = maxY = mid;
        }
    }


    void Update()
    {
        // ignore se o pointer esta sobre UI (se houver EventSystem)
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        // inicio do arrasto (mouse ou toque)
        if (!dragging && (Input.GetMouseButtonDown(0) || TouchBegan()))
        {
            dragging = true;
            dragOriginScreen = GetPointerPosition();
        }
        // fim do arrasto
        else if (dragging && (Input.GetMouseButtonUp(0) || TouchEnded()))
        {
            dragging = false;
        }

        // durante arrasto
        if (dragging)
        {
            Vector3 now = GetPointerPosition();
            Vector3 delta = now - dragOriginScreen; // em pixels
            dragOriginScreen = now;

            // converte delta de tela para mundo aproximadamente
            // como e so vertical, basta usar um fator por velocidade
            float dy = delta.y * (dragSpeed * Time.deltaTime);

            // atualiza alvo
            targetY = Mathf.Clamp(targetY - dy, minY, maxY);
        }

        // suaviza movimento
        var pos = transform.position;
        pos.y = Mathf.Lerp(pos.y, targetY, 1f - Mathf.Exp(-damping * Time.deltaTime));
        if (verticalOnly) pos.x = transform.position.x;
        transform.position = pos;
    }

    private Vector3 GetPointerPosition()
    {
        if (Input.touchCount > 0) return (Vector3)Input.touches[0].position;
        return Input.mousePosition;
    }

    private bool TouchBegan()
    {
        return Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Began;
    }

    private bool TouchEnded()
    {
        return Input.touchCount > 0 && (Input.touches[0].phase == TouchPhase.Ended || Input.touches[0].phase == TouchPhase.Canceled);
    }
}
