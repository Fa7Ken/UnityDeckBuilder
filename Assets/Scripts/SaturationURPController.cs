using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

// Controla a saturacao via ColorAdjustments (URP)
public class SaturationURPController : MonoBehaviour
{
    [Header("Referencias")]
    public Volume volume; // Volume global com ColorAdjustments

    [Header("Saturacao (0=PB, 1=cor)")]
    [Range(0f, 1f)] public float progress = 0f;

    // mapear 0..1 -> -100..0 (URP usa -100 = PB, 0 = normal)
    public float minSat = -100f;
    public float maxSat = 0f;

    private ColorAdjustments _colorAdj;

    void Awake()
    {
        if (volume == null)
        {
            Debug.LogWarning("[SaturationURPController] Volume nao atribuido.");
            return;
        }

        if (!volume.profile.TryGet(out _colorAdj))
        {
            Debug.LogWarning("[SaturationURPController] ColorAdjustments nao encontrado no Volume Profile.");
        }
    }

    void Update()
    {
        if (_colorAdj == null) return;
        float sat = Mathf.Lerp(minSat, maxSat, Mathf.Clamp01(progress));
        _colorAdj.saturation.Override(sat);
    }

    // opcional: API publica para outros sistemas
    public void SetProgress(float p)
    {
        progress = Mathf.Clamp01(p);
    }
}
