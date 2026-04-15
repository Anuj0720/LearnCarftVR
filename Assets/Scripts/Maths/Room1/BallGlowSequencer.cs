using System.Collections;
using UnityEngine;

/// <summary>
/// Glows tennis balls one by one in sequence, once.
/// Drag your tennis ball Renderers in order into the Inspector.
/// Call PlaySequence() to start.
/// </summary>
public class BallGlowSequencer : MonoBehaviour
{
    [Header("Balls — drag them in ORDER (Ball 1 first, last ball last)")]
    public Renderer[] balls;

    [Header("Glow Settings")]
    public Color glowColor = Color.yellow;
    public float glowIntensity = 3f;

    [Tooltip("How long each ball glows before the next one joins (seconds)")]
    public float perBallInterval = 0.5f;

    [Tooltip("How long the full sequence stays glowing at the end before fading out")]
    public float holdAtEndDuration = 1f;

    [Tooltip("How long the fade-out takes after the sequence finishes")]
    public float fadeOutDuration = 0.5f;

    [Tooltip("Play automatically when the scene starts")]
    public bool playOnStart = false;

    private Coroutine activeCoroutine;

    void Start()
    {
        // Pre-enable emission keyword on all ball materials
        foreach (Renderer r in balls)
        {
            if (r != null)
            {
                r.material.EnableKeyword("_EMISSION");
                r.material.SetColor("_EmissionColor", Color.black);
            }
        }

        if (playOnStart)
            PlaySequence();
    }

    /// <summary>Call this to start the glow sequence.</summary>
    public void PlaySequence()
    {
        if (activeCoroutine != null)
            StopCoroutine(activeCoroutine);

        // Reset all to off first
        foreach (Renderer r in balls)
            if (r != null) r.material.SetColor("_EmissionColor", Color.black);

        activeCoroutine = StartCoroutine(RunSequence());
    }

    /// <summary>Call this to stop and turn off all glow immediately.</summary>
    public void StopSequence()
    {
        if (activeCoroutine != null)
        {
            StopCoroutine(activeCoroutine);
            activeCoroutine = null;
        }

        foreach (Renderer r in balls)
            if (r != null) r.material.SetColor("_EmissionColor", Color.black);
    }

    IEnumerator RunSequence()
    {
        int activeBallCount = 0;
        float elapsed = 0f;

        // ── Phase 1: Add balls one by one ──────────────────────────
        // Every perBallInterval seconds the next ball joins the pulse.
        // All currently active balls pulse together the whole time.

        float totalIntroTime = perBallInterval * balls.Length;

        while (elapsed < totalIntroTime)
        {
            elapsed += Time.deltaTime;

            // How many balls should be glowing by now
            int shouldBeActive = Mathf.Min(
                Mathf.FloorToInt(elapsed / perBallInterval) + 1,
                balls.Length
            );
            activeBallCount = shouldBeActive;

            // Shared sine pulse for all active balls
            float pulse = (Mathf.Sin(elapsed * Mathf.PI * 2f) + 1f) / 2f;
            Color glow = glowColor * (pulse * glowIntensity);

            for (int i = 0; i < activeBallCount; i++)
                if (balls[i] != null) balls[i].material.SetColor("_EmissionColor", glow);

            yield return null;
        }

        // ── Phase 2: Hold — all balls glowing together ─────────────
        elapsed = 0f;
        while (elapsed < holdAtEndDuration)
        {
            elapsed += Time.deltaTime;
            float pulse = (Mathf.Sin(elapsed * Mathf.PI * 2f) + 1f) / 2f;
            Color glow = glowColor * (pulse * glowIntensity);

            foreach (Renderer r in balls)
                if (r != null) r.material.SetColor("_EmissionColor", glow);

            yield return null;
        }

        // ── Phase 3: Fade out ───────────────────────────────────────
        elapsed = 0f;
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float t = 1f - (elapsed / fadeOutDuration);
            Color glow = glowColor * (t * glowIntensity);

            foreach (Renderer r in balls)
                if (r != null) r.material.SetColor("_EmissionColor", glow);

            yield return null;
        }

        // ── Done — turn everything off ──────────────────────────────
        foreach (Renderer r in balls)
            if (r != null) r.material.SetColor("_EmissionColor", Color.black);

        activeCoroutine = null;
    }
}