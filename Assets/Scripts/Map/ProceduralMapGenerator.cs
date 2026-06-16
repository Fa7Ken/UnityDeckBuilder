using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class ProceduralMapGenerator : MonoBehaviour
{
    // ==========================
    // SPRITES POR TIPO DE NO
    // ==========================
    [Header("Sprites por tipo de no")]
    public Sprite nightmareSprite;  // batalha com pesadelo
    public Sprite dreamSprite;      // batalha com sonho
    public Sprite eliteSprite;      // batalha com elite
    public Sprite eventSprite;      // evento aleatorio
    public Sprite shopSprite;       // loja
    public Sprite restSprite;       // local de descanso
    public Sprite bossSprite;       // chefe final

    // ==========================
    // CONEXOES (LINHAS)
    // ==========================
    [Header("Conexoes")]
    public Material connectionMaterial;
    public float connectionWidth = 0.06f;

    // ==========================
    // LAYOUT DO MAPA
    // ==========================
    [Header("Layout")]
    [Range(5, 11)] public int columns = 7;           // quantidade de colunas na grade
    public float columnHorizontalSpacing = 2.2f;     // espacamento horizontal entre colunas
    public float layerVerticalSpacing = 2.6f;        // espacamento vertical entre camadas
    public Vector2 mapOrigin = new Vector2(0f, 0f);  // ponto de origem do mapa

    // ==========================
    // REGRAS DO MAPA
    // ==========================
    [Header("Regras do mapa")]
    public int playableLayers = 8;                         // 8 camadas jogaveis
    public Vector2Int nodesPerLayerRange = new Vector2Int(3, 5); // minimo e maximo de nos por camada
    public int minEliteOrShopTotal = 2;                    // minimo total de elite+loja nas camadas 4-7
    public int seed = 0;                                   // 0 = aleatorio; >0 reproduzivel

    // ==========================
    // NOMES DE CENA POR TIPO DE NO
    // Preencha no Inspector com os nomes exatos em Build Settings
    // ==========================
    [Header("Scenes por tipo de no")]
    public string nightmareScene = "BattleNightmare";
    public string dreamScene = "BattleDream";
    public string eliteScene = "BattleElite";
    public string eventScene = "Event";
    public string shopScene = "Shop";
    public string restScene = "Rest";
    public string bossScene = "Boss";

    // ==========================
    // TIPO INTERNO DE NO
    // ==========================
    private enum NodeKind { Nightmare, Dream, Elite, Event, Shop, Rest, Boss }

    // Pesos de sorteio por tipo de no (boss nao entra no sorteio)
    private readonly Dictionary<NodeKind, int> weights = new Dictionary<NodeKind, int>
    {
        { NodeKind.Nightmare, 22 },
        { NodeKind.Dream,     18 },
        { NodeKind.Elite,     14 },
        { NodeKind.Event,     20 },
        { NodeKind.Shop,      12 },
        { NodeKind.Rest,      14 },
    };

    // ==========================
    // NO (ESTRUTURA DE RUNTIME)
    // Nao serializamos esta classe para evitar ciclos (pais/filhos)
    // ==========================
    private class Node
    {
        public string id;            // identificador unico (guid)
        public NodeKind kind;        // tipo do no
        public int layerIndex;       // indice da camada (1..8), boss = 9
        public int column;           // coluna na grade
        public Vector3 worldPos;     // posicao no mundo

        [NonSerialized] public GameObject go;    // GameObject visual
        [NonSerialized] public NodeView view;    // componente de interacao

        [NonSerialized] public List<Node> parents = new List<Node>(); // pais (camada abaixo)
        [NonSerialized] public List<Node> children = new List<Node>(); // filhos (camada acima)
    }

    // ==========================
    // CAMPOS DE RUNTIME (NAO SERIALIZADOS)
    // ==========================
    [NonSerialized] private System.Random rng;                 // gerador de numeros aleatorios controlado por seed
    [NonSerialized] private List<List<Node>> layers;           // lista de camadas, cada uma com seus nos
    [NonSerialized] private Node bossNode;                     // no do chefe
    [NonSerialized] private Transform nodesRoot;               // pai dos GOs de nos
    [NonSerialized] private Transform connectionsRoot;         // pai dos GOs de linhas
    [NonSerialized] private Node currentNode;                  // no atual onde o jogador esta

    // ==========================
    // EDITOR (BOTOES NO INSPECTOR)
    // ==========================
#if UNITY_EDITOR
    [CustomEditor(typeof(ProceduralMapGenerator))]
    private class ProceduralMapGeneratorEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            var gen = (ProceduralMapGenerator)target;
            GUILayout.Space(8);
            if (GUILayout.Button("Gerar mapa (novo)"))
            {
                MapPersistence.Clear();
                gen.ClearPrevious();
                gen.GenerateNew();
            }
            if (GUILayout.Button("Recarregar (carregar save se houver)"))
            {
                gen.ClearPrevious();
                gen.GenerateFromSaveOrNew();
            }
            if (GUILayout.Button("Limpar"))
            {
                gen.ClearPrevious();
            }
        }
    }
#endif

    // ==========================
    // CICLO DE VIDA
    // ==========================
    private void Awake()
    {
        ClearPrevious();
        GenerateFromSaveOrNew();
    }

    // Remove objetos anteriores do mapa na cena
    private void ClearPrevious()
    {
        var a = transform.Find("MAP_Nodes");
        if (a) DestroyImmediate(a.gameObject);
        var b = transform.Find("MAP_Connections");
        if (b) DestroyImmediate(b.gameObject);
    }

    // Gera a partir do save (se existir) ou cria um novo mapa
    private void GenerateFromSaveOrNew()
    {
        if (MapPersistence.HasSave())
        {
            BuildFromSave(MapPersistence.Load());
            return;
        }
        GenerateNew();
    }

    // ==========================
    // GERACAO DE NOVO MAPA
    // ==========================
    private void GenerateNew()
    {
        // Cria RNG: seed 0 => usa um valor aleatorio novo; seed > 0 => deterministico
        rng = (seed == 0) ? new System.Random(Guid.NewGuid().GetHashCode()) : new System.Random(seed);

        SetupRoots();

        // Planeja colunas por camada respeitando adjacencia (|delta col| <= 1 possivel)
        var columnPlan = PlanColumnsPerLayer();

        layers = new List<List<Node>>(playableLayers + 1);

        int eliteOrShopCount = 0;

        // Cria nos camada a camada
        for (int layer = 1; layer <= playableLayers; layer++)
        {
            var nodeList = new List<Node>();
            var allowedKinds = GetAllowedKindsForLayer(layer);

            foreach (var col in columnPlan[layer])
            {
                var kind = WeightedPick(allowedKinds);
                var node = CreateNode(kind, layer, col);
                nodeList.Add(node);
                if (kind == NodeKind.Elite || kind == NodeKind.Shop) eliteOrShopCount++;
            }

            // Regras especiais por camada
            if (layer == 8) foreach (var n in nodeList) ForceKind(n, NodeKind.Rest);
            if (layer == 1) foreach (var n in nodeList) ForceKind(n, NodeKind.Nightmare);

            layers.Add(nodeList);
        }

        // Garante minimo de elite/loja no total (nas camadas 4-7)
        if (eliteOrShopCount < minEliteOrShopTotal)
        {
            int need = minEliteOrShopTotal - eliteOrShopCount;
            var candidates = layers[6] // camada 7 => index 6
                .Where(n => n.kind != NodeKind.Elite && n.kind != NodeKind.Shop && n.kind != NodeKind.Rest)
                .OrderBy(_ => rng.Next())
                .ToList();

            for (int i = 0; i < need && i < candidates.Count; i++)
                ForceKind(candidates[i], (i % 2 == 0) ? NodeKind.Elite : NodeKind.Shop);
        }

        // Conecta camadas (bottom-up) respeitando |delta col| <= 1
        ConnectLayers(columnPlan);

        // Cria chefe sem conexoes, alinhado acima de um Rest da camada 8
        CreateBoss(columnPlan);

        // Desenha as linhas do grafo
        DrawConnections();

        // Estado inicial: somente camada 1 clicavel; boss bloqueado
        currentNode = null;
        SetInteractableForStart();

        #if UNITY_2023_1_OR_NEWER
                var camDrag = UnityEngine.Object.FindFirstObjectByType<CameraDragClamp>();
                // ou, se qualquer instancia servir e quiser um pouco mais de perf:
                // var camDrag = UnityEngine.Object.FindAnyObjectByType<CameraDragClamp>();
        #else
        var camDrag = FindObjectOfType<CameraDragClamp>();
        #endif
        if (camDrag != null) camDrag.RecomputeBoundsFromNodes();

        if (camDrag != null) camDrag.RecomputeBoundsFromNodes();

        // Salva em JSON (PlayerPrefs) para persistencia entre cenas
        MapPersistence.Save(ToSerializableMap());
    }

    // Cria GameObjects-pai para organizar cena
    private void SetupRoots()
    {
        nodesRoot = new GameObject("MAP_Nodes").transform;
        nodesRoot.SetParent(transform, false);
        connectionsRoot = new GameObject("MAP_Connections").transform;
        connectionsRoot.SetParent(transform, false);
    }

    // ==========================
    // INTERACAO DE NO (CLIQUES)
    // ==========================
    private void WireNodeView(Node n)
    {
        var nv = n.go.AddComponent<NodeView>();
        nv.Id = n.id;
        nv.Layer = n.layerIndex;
        nv.Column = n.column;
        nv.Kind = n.kind.ToString();
        nv.SetSprite(SpriteFor(n.kind));
        nv.SetState(NodeState.Locked);
        nv.OnClicked = HandleNodeClicked;
        n.view = nv;
    }

    // Dispara ao clicar em um no valido
    private void HandleNodeClicked(NodeView clicked)
    {
        var n = FindNodeById(clicked.Id);
        if (n == null) return;

        if (currentNode != null)
            currentNode.view.SetState(NodeState.Visited);

        currentNode = n;
        currentNode.view.SetState(NodeState.Visited);

        // Apenas filhos do no atual ficam disponiveis
        SetOnlyChildrenInteractable(currentNode);

        // Salva progresso: no atual e o mesmo mapa
        var data = ToSerializableMap();
        data.currentNodeId = currentNode.id;
        MapPersistence.Save(data);

        // Transicao de cena por tipo de no (com fade se SceneTransition existir)
        var sceneName = GetSceneFor(n.kind);
        if (!string.IsNullOrEmpty(sceneName))
        {
            if (SceneTransition.Instance != null)
                SceneTransition.Instance.LoadSceneWithFade(sceneName);
            else
                UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogWarning($"Sem cena configurada para {n.kind}. Preencha no Inspector.");
        }

    }

    // No inicio so a camada 1 e clicavel; boss bloqueado
    private void SetInteractableForStart()
    {
        foreach (var layer in layers)
            foreach (var n in layer)
                n.view.SetInteractable(false);

        foreach (var n in layers[0]) // camada 1
        {
            n.view.SetState(NodeState.Available);
            n.view.SetInteractable(true);
        }

        bossNode.view.SetState(NodeState.Locked);
        bossNode.view.SetInteractable(false);
    }

    // Disponibiliza apenas os filhos do no atual; libera boss apos qualquer Rest da camada 8
    private void SetOnlyChildrenInteractable(Node parent)
    {
        // desabilita tudo
        foreach (var layer in layers)
            foreach (var n in layer)
                n.view.SetInteractable(false);

        // habilita apenas os filhos (proxima camada)
        foreach (var c in parent.children)
        {
            c.view.SetState(NodeState.Available);
            c.view.SetInteractable(true);
        }

        // regra do boss: sem conexoes, mas liberado apos visitar um Rest da camada 8
        bossNode.view.SetInteractable(false);
        if (parent.layerIndex == 8 && parent.kind == NodeKind.Rest)
        {
            bossNode.view.SetState(NodeState.Available);
            bossNode.view.SetInteractable(true);
        }
    }

    // ==========================
    // RECONSTRUCAO A PARTIR DO SAVE
    // ==========================
    private void BuildFromSave(SerializableMap save)
    {
        // Restaura parametros principais (seed aqui e informativo)
        seed = save.seed;
        playableLayers = save.playableLayers;
        columns = save.columns;
        columnHorizontalSpacing = save.columnHorizontalSpacing;
        layerVerticalSpacing = save.layerVerticalSpacing;

        SetupRoots();
        layers = new List<List<Node>>(playableLayers + 1);

        // Cria listas vazias para cada camada
        for (int layer = 1; layer <= playableLayers; layer++)
            layers.Add(new List<Node>());

        // Recria todos os nos (inclusive boss) no lugar certo
        var allNodes = new Dictionary<string, Node>();
        foreach (var sn in save.nodes)
        {
            var nk = (NodeKind)Enum.Parse(typeof(NodeKind), sn.kind);
            var node = CreateNode(nk, sn.layer, sn.column, forceId: sn.id, createView: true);
            allNodes[sn.id] = node;
            if (nk == NodeKind.Boss) bossNode = node;
            else layers[sn.layer - 1].Add(node);
        }

        // Recria ligacoes
        foreach (var sn in save.nodes)
        {
            var parent = allNodes[sn.id];
            foreach (var cid in sn.childrenIds)
            {
                if (allNodes.TryGetValue(cid, out var child))
                    Link(parent, child);
            }
        }

        // Desenha conexoes
        DrawConnections();

        // Restaura disponibilidade conforme no atual
        if (!string.IsNullOrEmpty(save.currentNodeId) && allNodes.TryGetValue(save.currentNodeId, out var cur))
        {
            currentNode = cur;
            currentNode.view.SetState(NodeState.Visited);
            SetOnlyChildrenInteractable(currentNode);
        }
        else
        {
            SetInteractableForStart();
        }
    }

    // ==========================
    // SERIALIZACAO PARA JSON (PERSISTENCIA)
    // ==========================
    private SerializableMap ToSerializableMap()
    {
        var data = new SerializableMap
        {
            playableLayers = playableLayers,
            columns = columns,
            columnHorizontalSpacing = columnHorizontalSpacing,
            layerVerticalSpacing = layerVerticalSpacing,
            seed = seed,
            currentNodeId = (currentNode != null) ? currentNode.id : null,
            bossId = (bossNode != null) ? bossNode.id : null
        };

        // inclui todas as camadas + boss
        var list = new List<Node>();
        foreach (var layer in layers) list.AddRange(layer);
        if (bossNode != null) list.Add(bossNode);

        foreach (var n in list)
        {
            var sn = new SerializableNode
            {
                id = n.id,
                kind = n.kind.ToString(),
                layer = n.layerIndex,
                column = n.column,
                childrenIds = n.children.Select(c => c.id).ToList()
            };
            data.nodes.Add(sn);
        }
        return data;
    }

    // Busca no por id (varre camadas e boss)
    private Node FindNodeById(string id)
        => layers.SelectMany(l => l).Concat(bossNode != null ? new[] { bossNode } : Array.Empty<Node>())
           .FirstOrDefault(n => n.id == id);

    // ==========================
    // PLANEJAMENTO DE COLUNAS E TIPOS
    // ==========================
    private Dictionary<int, List<int>> PlanColumnsPerLayer()
    {
        var plan = new Dictionary<int, List<int>>();

        // Camada 1: escolhe aleatoriamente entre min..max colunas
        int baseCount = rng.Next(nodesPerLayerRange.x, nodesPerLayerRange.y + 1);
        var baseCols = PickDistinctColumns(baseCount);
        plan[1] = baseCols;

        // Demais camadas: derivadas com deslocamento [-1,0,+1] e ajuste de quantidade
        for (int layer = 2; layer <= playableLayers; layer++)
        {
            var prevCols = plan[layer - 1];
            int targetCount = rng.Next(nodesPerLayerRange.x, nodesPerLayerRange.y + 1);

            var provisional = new HashSet<int>();
            foreach (var pc in prevCols)
            {
                int shift = new[] { -1, 0, 1 }[rng.Next(0, 3)];
                int nc = Mathf.Clamp(pc + shift, 0, columns - 1);
                provisional.Add(nc);
            }

            // aumenta ate atingir targetCount
            while (provisional.Count < targetCount)
            {
                var any = provisional.ElementAt(rng.Next(provisional.Count));
                var opts = new List<int>();
                if (any - 1 >= 0) opts.Add(any - 1);
                if (any + 1 <= columns - 1) opts.Add(any + 1);
                if (opts.Count == 0) opts.Add(any);
                provisional.Add(opts[rng.Next(opts.Count)]);
            }

            // reduz se passou
            while (provisional.Count > targetCount)
            {
                var toRemove = provisional.ElementAt(rng.Next(provisional.Count));
                provisional.Remove(toRemove);
            }

            plan[layer] = provisional.OrderBy(c => c).ToList();
        }

        return plan;
    }

    // Escolhe N colunas distintas aleatoriamente
    private List<int> PickDistinctColumns(int count)
    {
        var pool = Enumerable.Range(0, columns).ToList();
        var picked = new List<int>();
        for (int i = 0; i < count && pool.Count > 0; i++)
        {
            int idx = rng.Next(pool.Count);
            picked.Add(pool[idx]);
            pool.RemoveAt(idx);
        }
        picked.Sort();
        return picked;
    }

    // Quais tipos de no sao permitidos em cada camada
    private List<NodeKind> GetAllowedKindsForLayer(int layer)
    {
        if (layer == 1) return new List<NodeKind> { NodeKind.Nightmare };
        if (layer == 8) return new List<NodeKind> { NodeKind.Rest };
        if (layer <= 3) return new List<NodeKind> { NodeKind.Nightmare, NodeKind.Dream, NodeKind.Event, NodeKind.Rest };
        return new List<NodeKind> { NodeKind.Nightmare, NodeKind.Dream, NodeKind.Event, NodeKind.Rest, NodeKind.Elite, NodeKind.Shop };
    }

    // Sorteio ponderado pelo dicionario de pesos
    private NodeKind WeightedPick(List<NodeKind> allowed)
    {
        int total = allowed.Sum(k => weights.ContainsKey(k) ? weights[k] : 0);
        if (total <= 0) return allowed[0];

        int roll = rng.Next(1, total + 1);
        int cum = 0;
        foreach (var k in allowed)
        {
            cum += weights.ContainsKey(k) ? weights[k] : 0;
            if (roll <= cum) return k;
        }
        return allowed.Last();
    }

    // ==========================
    // CRIACAO DE NOS E BOSS
    // ==========================
    private Node CreateNode(NodeKind kind, int layer, int col, string forceId = null, bool createView = true)
    {
        var node = new Node
        {
            id = forceId ?? Guid.NewGuid().ToString("N"),
            kind = kind,
            layerIndex = layer,
            column = col,
            worldPos = GridToWorld(layer, col)
        };

        var go = new GameObject($"Node_L{layer}_C{col}_{kind}");
        go.transform.SetParent(nodesRoot, false);
        go.transform.position = node.worldPos;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteFor(kind);
        sr.sortingOrder = 5 + layer;

        node.go = go;
        if (createView) WireNodeView(node);
        return node;
    }

    private void ForceKind(Node node, NodeKind kind)
    {
        node.kind = kind;
        var s = SpriteFor(kind);
        node.go.GetComponent<SpriteRenderer>().sprite = s;
        if (node.view != null) node.view.SetSprite(s);
    }

    private Sprite SpriteFor(NodeKind kind)
    {
        switch (kind)
        {
            case NodeKind.Nightmare: return nightmareSprite;
            case NodeKind.Dream: return dreamSprite;
            case NodeKind.Elite: return eliteSprite;
            case NodeKind.Event: return eventSprite;
            case NodeKind.Shop: return shopSprite;
            case NodeKind.Rest: return restSprite;
            case NodeKind.Boss: return bossSprite;
            default: return null;
        }
    }

    private Vector3 GridToWorld(int layer, int col)
    {
        float x = mapOrigin.x + (col - (columns - 1) / 2f) * columnHorizontalSpacing;
        float y = mapOrigin.y + (layer - 1) * layerVerticalSpacing;
        return new Vector3(x, y, 0f);
    }

    // Cria o boss sem conexoes, alinhado acima de um Rest da camada 8
    private void CreateBoss(Dictionary<int, List<int>> columnPlan)
    {
        int bossLayer = playableLayers + 1;

        // Escolhe um Rest da camada 8 para alinhar o boss visualmente acima
        var lastLayer = layers[playableLayers - 1]; // todos Rest
        var anchor = lastLayer[rng.Next(lastLayer.Count)];

        bossNode = CreateNode(NodeKind.Boss, bossLayer, anchor.column);
        bossNode.go.name = "Node_BOSS";

        // Boss 2x maior e sorting alto
        bossNode.go.transform.localScale = Vector3.one * 2f;
        var sr = bossNode.go.GetComponent<SpriteRenderer>();
        sr.sortingOrder = 200;

        // Nao cria conexao com a camada 8 (liberamos clique apos Rest da camada 8)
    }

    // ==========================
    // LIGACOES E DESENHO
    // ==========================
    private void ConnectLayers(Dictionary<int, List<int>> columnPlan)
    {
        for (int layer = 2; layer <= playableLayers; layer++)
        {
            var prev = layers[layer - 2];
            var curr = layers[layer - 1];

            // garante pelo menos 1 pai para cada no atual
            foreach (var cn in curr)
            {
                var candidates = prev.Where(p => Mathf.Abs(p.column - cn.column) <= 1).ToList();
                if (candidates.Count == 0) Link(prev.OrderBy(p => Mathf.Abs(p.column - cn.column)).First(), cn);
                else Link(candidates[rng.Next(candidates.Count)], cn);
            }

            // garante pelo menos 1 filho para cada no anterior
            foreach (var pn in prev)
            {
                if (pn.children.Count == 0)
                {
                    var candidates = curr.Where(c => Mathf.Abs(c.column - pn.column) <= 1).ToList();
                    if (candidates.Count == 0) Link(pn, curr.OrderBy(c => Mathf.Abs(c.column - pn.column)).First());
                    else Link(pn, candidates[rng.Next(candidates.Count)]);
                }
            }

            // conexoes extras opcionais para variedade
            foreach (var pn in prev)
            {
                if (rng.NextDouble() < 0.35)
                {
                    var opts = curr.Where(c => Mathf.Abs(c.column - pn.column) <= 1 && !pn.children.Contains(c)).ToList();
                    if (opts.Count > 0) Link(pn, opts[rng.Next(opts.Count)]);
                }
            }
        }
    }

    private void Link(Node a, Node b)
    {
        if (!a.children.Contains(b)) a.children.Add(b);
        if (!b.parents.Contains(a)) b.parents.Add(a);
    }

    private void DrawConnections()
    {
        foreach (var layer in layers)
            foreach (var n in layer)
                foreach (var child in n.children)
                    CreateConnection(n.worldPos, child.worldPos);

        // boss nao tem ligacoes aqui
    }

    private void CreateConnection(Vector3 a, Vector3 b, float widthMultiplier = 1f)
    {
        var go = new GameObject("Conn");
        go.transform.SetParent(connectionsRoot, false);

        var lr = go.AddComponent<LineRenderer>();
        lr.material = connectionMaterial;
        lr.startWidth = lr.endWidth = connectionWidth * widthMultiplier;
        lr.positionCount = 2;
        lr.useWorldSpace = true;
        lr.numCornerVertices = 2;
        lr.numCapVertices = 2;
        lr.sortingOrder = 3;
        lr.SetPosition(0, a);
        lr.SetPosition(1, b);
    }

    // ==========================
    // UTILIDADES
    // ==========================
    private string GetSceneFor(NodeKind kind)
    {
        switch (kind)
        {
            case NodeKind.Nightmare: return nightmareScene;
            case NodeKind.Dream: return dreamScene;
            case NodeKind.Elite: return eliteScene;
            case NodeKind.Event: return eventScene;
            case NodeKind.Shop: return shopScene;
            case NodeKind.Rest: return restScene;
            case NodeKind.Boss: return bossScene;
            default: return null;
        }
    }
}
