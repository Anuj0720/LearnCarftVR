using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

public class Teleport : MonoBehaviour
{
    [Header("Teleport Target")]
    public Transform targetPosition;
    public Transform xrRig;

    [Header("XR Camera (assign your Main Camera / CenterEyeAnchor)")]
    public Camera xrCamera;

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

    void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        CreateFadeOverlay();

        // Auto-find the XR camera if not assigned
        if (xrCamera == null)
            xrCamera = Camera.main;
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
        GameObject canvasGO = new GameObject("TeleportFadeCanvas");
        DontDestroyOnLoad(canvasGO);
        fadeCanvas = canvasGO.AddComponent<Canvas>();
        fadeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        fadeCanvas.sortingOrder = 999;

        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        GameObject imgGO = new GameObject("FadeImage");
        imgGO.transform.SetParent(canvasGO.transform, false);
        fadeImage = imgGO.AddComponent<Image>();

        RectTransform rect = fadeImage.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

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

        fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, fromAlpha);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(fromAlpha, toAlpha, elapsed / duration);
            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, alpha);
            yield return null;
        }

        fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, toAlpha);
    }

    // ── Moves the XR Rig to target with camera offset compensation ────────────
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

        // --- CAMERA OFFSET COMPENSATION FIX ---
        // In XR, the camera drifts from the rig origin due to real-world head tracking.
        // We must subtract the horizontal (X/Z) camera-to-rig offset so the camera
        // lands exactly at the target instead of being displaced outside the area.

        if (xrCamera != null)
        {
            // Horizontal offset between rig root and the actual camera position
            Vector3 cameraWorldPos = xrCamera.transform.position;
            Vector3 rigWorldPos    = xrRig.position;

            float offsetX = cameraWorldPos.x - rigWorldPos.x;
            float offsetZ = cameraWorldPos.z - rigWorldPos.z;

            // Place the rig so the camera ends up exactly at the target XZ position.
            // Use the target's Y directly for the rig so the floor stays correct.
            xrRig.position = new Vector3(
                targetPosition.position.x - offsetX,
                targetPosition.position.y,          // target Y = floor level for the rig
                targetPosition.position.z - offsetZ
            );
        }
        else
        {
            // Fallback: no camera reference, place rig directly (original behaviour)
            xrRig.position = targetPosition.position;
        }

        // Only rotate on Y axis to avoid tilting the rig
        Vector3 currentRotation = xrRig.eulerAngles;
        xrRig.eulerAngles = new Vector3(
            currentRotation.x,
            targetPosition.eulerAngles.y,
            currentRotation.z
        );

        Debug.Log($"XR Rig teleported to {xrRig.position} (camera now at ~{targetPosition.position})");
    }
}