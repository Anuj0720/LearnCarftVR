using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

public class Teleport : MonoBehaviour
{
    [Header("Teleport Target")]
    public Transform targetPosition;
    public Transform xrRig;

    [Header("Fade Settings")]
    public Color fadeColor = Color.black;
    public float fadeSpeed = 1.5f;

    [Header("Audio Settings")]
    public AudioClip teleportSound;

    [Header("Timeline (Optional)")]
    public PlayableDirector celebrationTimeline;

    private AudioSource audioSource;
    private bool teleportTriggered = false;

    // Fade overlay
    private Canvas fadeCanvas;
    private Image fadeImage;
    private bool isFading = false;

    void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        CreateFadeOverlay();
    }

    void Start()
    {
        if (celebrationTimeline != null)
            celebrationTimeline.stopped += OnTimelineFinished;
    }

    void OnDestroy()
    {
        if (celebrationTimeline != null)
            celebrationTimeline.stopped -= OnTimelineFinished;
    }

    // ── Creates a full-screen black UI overlay ────────────────────────────────
    void CreateFadeOverlay()
    {
        // Canvas
        GameObject canvasGO = new GameObject("TeleportFadeCanvas");
        DontDestroyOnLoad(canvasGO);
        fadeCanvas = canvasGO.AddComponent<Canvas>();
        fadeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        fadeCanvas.sortingOrder = 999; // Always on top

        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        // Full-screen Image
        GameObject imgGO = new GameObject("FadeImage");
        imgGO.transform.SetParent(canvasGO.transform, false);
        fadeImage = imgGO.AddComponent<Image>();

        RectTransform rect = fadeImage.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        // Start fully transparent
        fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f);
    }

    // ── Timeline callback ─────────────────────────────────────────────────────
    private void OnTimelineFinished(PlayableDirector director)
    {
        if (!teleportTriggered)
            TriggerTeleport();
    }

    // ── Main entry point (also callable from Signal Receiver) ─────────────────
    public void TriggerTeleport()
    {
        if (teleportTriggered) return;
        teleportTriggered = true;

        if (teleportSound != null)
            audioSource.PlayOneShot(teleportSound);

        StartCoroutine(TeleportSequence());
    }

    // ── Full fade-out → teleport → fade-in sequence ───────────────────────────
    private System.Collections.IEnumerator TeleportSequence()
    {
        // 1. Fade OUT to black
        yield return StartCoroutine(Fade(0f, 1f));

        // 2. Move XR Rig while screen is black
        MoveXRRig();

        // Small pause so the new location renders before revealing
        yield return new WaitForSeconds(0.1f);

        // 3. Fade IN back to clear
        yield return StartCoroutine(Fade(1f, 0f));

        teleportTriggered = false;
    }

    // ── Smooth alpha fade coroutine ───────────────────────────────────────────
    private System.Collections.IEnumerator Fade(float fromAlpha, float toAlpha)
    {
        float elapsed = 0f;
        float duration = 1f / fadeSpeed;

        Color c = new Color(fadeColor.r, fadeColor.g, fadeColor.b, fromAlpha);
        fadeImage.color = c;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(fromAlpha, toAlpha, elapsed / duration);
            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, alpha);
            yield return null;
        }

        fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, toAlpha);
    }

    // ── Moves the XR Rig to target ────────────────────────────────────────────
    void MoveXRRig()
    {
        if (xrRig == null)
        {
            Debug.LogWarning("Teleport: XR Rig is not assigned!");
            return;
        }

        if (targetPosition == null)
        {
            Debug.LogWarning("Teleport: Target Position is not assigned!");
            return;
        }

        xrRig.position = targetPosition.position;

        // Only rotate on Y axis to avoid tilting the rig
        Vector3 currentRotation = xrRig.eulerAngles;
        xrRig.eulerAngles = new Vector3(
            currentRotation.x,
            targetPosition.eulerAngles.y,
            currentRotation.z
        );

        Debug.Log($"XR Rig teleported to {targetPosition.position}");
    }
}