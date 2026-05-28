using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

public class Teleport : MonoBehaviour
{
    [Header("Teleport Target")]
    public Transform targetPosition;
    public Transform xrRig;

    [Header("XR Camera")]
    public Camera xrCamera;

    [Header("Fade Settings")]
    public Color fadeColor = Color.black;
    public float fadeSpeed = 1.5f;

    [Header("Audio Settings")]
    public AudioClip teleportSound;

    [Header("Timeline (Optional)")]
    public PlayableDirector celebrationTimeline;

    [Header("VFX Settings")]
    public ParticleSystem teleportVFXPrefab;
    public float vfxScale = 1f;

    private ParticleSystem spawnedVFX;
    private AudioSource audioSource;
    private bool teleportTriggered = false;

    // Store the desired teleport destination as soon as teleport is triggered
    private Vector3 pendingPosition;
    private Quaternion pendingRotation;
    private bool hasPendingTarget = false;

    private Canvas fadeCanvas;
    private Image fadeImage;

    void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        CreateFadeOverlay();

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

    private void OnTimelineFinished(PlayableDirector director)
    {
        if (!teleportTriggered)
            TriggerTeleport();
    }

    public void TriggerTeleport()
    {
        if (teleportTriggered) return;
        teleportTriggered = true;

        // Capture the target world position/rotation now so player movement
        // before the actual teleport doesn't change the destination.
        if (targetPosition != null)
        {
            pendingPosition = targetPosition.position;
            pendingRotation = targetPosition.rotation;
            hasPendingTarget = true;
        }
        else
        {
            hasPendingTarget = false;
        }

        if (teleportSound != null)
            audioSource.PlayOneShot(teleportSound);

        PlayVFX();
        StartCoroutine(TeleportSequence());
    }

    private System.Collections.IEnumerator TeleportSequence()
    {
        // Fade out
        yield return StartCoroutine(Fade(0f, 1f));

        // Move directly to target
        MoveXRRig();

        yield return new WaitForSeconds(0.1f);

        // Fade in
        yield return StartCoroutine(Fade(1f, 0f));

        // Destroy VFX
        StopVFX();

        teleportTriggered = false;
    }

    void MoveXRRig()
{
        if (xrRig == null) return;

        // Determine destination: prefer the captured pending target (captured at trigger time),
        // otherwise fall back to the current targetPosition transform.
        Vector3 destination;
        Quaternion destinationRotation;

        if (hasPendingTarget)
        {
            destination = pendingPosition;
            destinationRotation = pendingRotation;
        }
        else if (targetPosition != null)
        {
            destination = targetPosition.position;
            destinationRotation = targetPosition.rotation;
        }
        else
        {
            // Nothing to do
            return;
        }

        // Move xrRig to the computed destination
        xrRig.position = destination;
        xrRig.rotation = destinationRotation;

        // Reset tracking space so camera recenters on rig
        Unity.XR.CoreUtils.XROrigin xrOrigin = xrRig.GetComponent<Unity.XR.CoreUtils.XROrigin>();
        if (xrOrigin != null)
        {
            xrOrigin.MoveCameraToWorldLocation(destination);
        }

        hasPendingTarget = false;

        Debug.Log("Teleported to: " + destination);
}

    void PlayVFX()
    {
        if (teleportVFXPrefab == null) return;

        Vector3 spawnPosition = xrCamera != null
            ? xrCamera.transform.position
            : transform.position;

        spawnedVFX = Instantiate(teleportVFXPrefab, spawnPosition, Quaternion.identity);
        spawnedVFX.transform.localScale = Vector3.one * vfxScale;
        spawnedVFX.Play();

        Debug.Log("Teleport VFX Started");
    }

    void StopVFX()
    {
        if (spawnedVFX == null) return;
        spawnedVFX.Stop();
        Destroy(spawnedVFX.gameObject);
        spawnedVFX = null;
        Debug.Log("Teleport VFX Destroyed");
    }

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
}