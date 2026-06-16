using System;
using System.Collections.Generic;
using UnityEngine;

// Estrutura serializavel de um no (para salvar/carregar em JSON)
// Observacao: aqui salvamos apenas dados "leves" (sem GameObjects ou referencias cíclicas).
[Serializable]
public class SerializableNode
{
    public string id;                 // guid do no
    public string kind;               // tipo do no em string
    public int layer;                 // camada (1..8); boss = 9
    public int column;                // coluna na grade
    public List<string> childrenIds = new(); // ids dos filhos (na camada acima)
}

// Estrutura serializavel do mapa como um todo
[Serializable]
public class SerializableMap
{
    // Parametros para reconstruir o layout
    public int playableLayers;
    public int columns;
    public float columnHorizontalSpacing;
    public float layerVerticalSpacing;
    public int seed;

    // Progresso do jogador
    public string currentNodeId;   // no atual onde o jogador esta
    public string bossId;          // id do boss (opcionalmente informativo)

    // Todos os nos (camadas 1..8 + boss)
    public List<SerializableNode> nodes = new();
}

// Classe estatica para salvar e carregar o JSON em PlayerPrefs
public static class MapPersistence
{
    // Troque a chave se quiser "slots" diferentes
    private const string Key = "PROC_MAP_SAVE_V1";

    // Verifica se existe um save
    public static bool HasSave() => PlayerPrefs.HasKey(Key);

    // Salva o mapa e estado atual em JSON
    public static void Save(SerializableMap data)
    {
        var json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(Key, json);
        PlayerPrefs.Save();
#if UNITY_EDITOR
        Debug.Log($"[MapPersistence] Mapa salvo ({json.Length} chars).");
#endif
    }

    // Carrega o mapa salvo (ou null se nao existir)
    public static SerializableMap Load()
    {
        if (!HasSave()) return null;
        var json = PlayerPrefs.GetString(Key, "{}");
        var data = JsonUtility.FromJson<SerializableMap>(json);
#if UNITY_EDITOR
        Debug.Log("[MapPersistence] Mapa carregado.");
#endif
        return data;
    }

    // Apaga o save atual
    public static void Clear()
    {
        PlayerPrefs.DeleteKey(Key);
#if UNITY_EDITOR
        Debug.Log("[MapPersistence] Save limpo.");
#endif
    }
}
