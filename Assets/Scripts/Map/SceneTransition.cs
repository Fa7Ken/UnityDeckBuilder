using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneTransition : MonoBehaviour
{
    public static SceneTransition Instance { get; private set; }

    [Header("Fade")]
    public float fadeDuration = 0.4f; // tempo do fade
    public AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Overlay (opcional)")]
    public CanvasGroup canvasGroup;   // se nao setar, criaremos um automaticamente
    public Image overlayImage;        // imagem preta

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        EnsureOverlay();
        // fade-in inicial ao abrir a cena
        StartCoroutine(FadeIn());
    }

    // Carrega cena com fade-out -> load -> fade-in
    public void LoadSceneWithFade(string sceneName)
    {
        StartCoroutine(FadeAndLoad(sceneName));
    }

    private IEnumerator FadeAndLoad(string sceneName)
    {
        yield return FadeOut();
        yield return SceneManager.LoadSceneAsync(sceneName);
        yield return FadeIn();
    }

    public IEnumerator FadeIn()
    {
        if (canvasGroup == null) yield break;
        canvasGroup.blocksRaycasts = true;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / fadeDuration;
            float a = 1f - fadeCurve.Evaluate(Mathf.Clamp01(t));
            canvasGroup.alpha = a;
            yield return null;
        }
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
    }

    public IEnumerator FadeOut()
    {
        if (canvasGroup == null) yield break;
        canvasGroup.blocksRaycasts = true;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / fadeDuration;
            float a = fadeCurve.Evaluate(Mathf.Clamp01(t));
            canvasGroup.alpha = a;
            yield return null;
        }
        canvasGroup.alpha = 1f;
    }

    // Se nao houver overlay no inspector, cria um Canvas + Image preta no runtime
    private void EnsureOverlay()
    {
        if (canvasGroup != null && overlayImage != null) return;

        var canvasGO = new GameObject("FadeCanvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999;

        var cg = canvasGO.AddComponent<CanvasGroup>();
        cg.alpha = 1f; // comecar preto, depois FadeIn no Awake
        cg.blocksRaycasts = true;

        var imgGO = new GameObject("FadeOverlay");
        imgGO.transform.SetParent(canvasGO.transform, false);
        var img = imgGO.AddComponent<Image>();
        img.color = Color.black;
        img.raycastTarget = true;

        var rt = img.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        canvasGroup = cg;
        overlayImage = img;
    }
}