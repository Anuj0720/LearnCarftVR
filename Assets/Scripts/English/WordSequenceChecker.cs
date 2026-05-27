using System.Collections;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Central puzzle tracker.
/// Each WordSlot calls OnSlotFilledCorrectly() or OnWrongPlacement().
/// When _correctCount reaches totalSlots → plays the celebration timeline.
///
/// FIX: Celebration logic has zero dependency on optional refs (HintsManager,
/// HintNumberSpawner). Both are called only if assigned. The celebration can
/// never be blocked by a missing reference.
/// </summary>
public class WordSequenceChecker : MonoBehaviour
{
    [Header("Puzzle Setup")]
    [Tooltip("Must equal the number of WordSlot sockets in your scene.")]
    public int totalSlots = 4;

    [Header("Celebration  ← REQUIRED")]
    public PlayableDirector celebrationTimeline;

    [Header("Optional References")]
    public HintNumberSpawner hintNumberSpawner;
    public HintsManager      hintsManager;
    public GameObject        puzzleBlocksParent;

    [Header("Victory Audio")]
    public AudioSource correctAllAudioSource;
    public AudioClip   correctAllClip;
    public AudioSource fallbackAudioSource;

    // ── Private ───────────────────────────────────────────────────────────────
    private int  _correctCount   = 0;
    private bool _puzzleComplete = false;

    // ── Validate on Start ─────────────────────────────────────────────────────
    void Start()
    {
        if (celebrationTimeline == null)
            Debug.LogError("[WordSequenceChecker] ❌ 'Celebration Timeline' is NOT assigned! " +
                           "Drag your celebration PlayableDirector into this field.", this);
    }

    // ── Called by WordSlot ────────────────────────────────────────────────────

    public void OnSlotFilledCorrectly(int slotIndex)
    {
        if (_puzzleComplete) return;

        _correctCount++;
        Debug.Log($"[WordSequenceChecker] Slot {slotIndex} ✅  Progress {_correctCount}/{totalSlots}");

        if (_correctCount >= totalSlots)
            StartCoroutine(TriggerVictory());
    }

    public void OnWrongPlacement()
    {
        if (_puzzleComplete) return;
        if (hintsManager != null) hintsManager.ResetTimer();
    }

    // ── Victory sequence ──────────────────────────────────────────────────────

    IEnumerator TriggerVictory()
    {
        _puzzleComplete = true;
        Debug.Log("[WordSequenceChecker] 🎉 All correct! Starting victory sequence.");

        // 1. Victory sound — fire immediately
        PlayVictorySound();

        // 2. Optional cleanup
        if (hintsManager      != null) hintsManager.PuzzleSolved();
        if (hintNumberSpawner != null) hintNumberSpawner.HideAllHints();

        // 3. Disable grabbing
        DisableAllGrabbing();

        // 4. Small delay so victory sound starts before timeline
        yield return new WaitForSeconds(0.3f);

        // 5. Play celebration — the only thing that truly matters
        if (celebrationTimeline != null)
        {
            celebrationTimeline.Play();
            Debug.Log("[WordSequenceChecker] 🎬 Celebration timeline started.");
        }
        else
        {
            Debug.LogError("[WordSequenceChecker] celebrationTimeline is NULL — cannot play celebration!", this);
        }
    }

    void DisableAllGrabbing()
    {
        if (puzzleBlocksParent == null) return;
        foreach (var g in puzzleBlocksParent.GetComponentsInChildren<XRGrabInteractable>(true))
            g.enabled = false;
    }

    void PlayVictorySound()
    {
        if (correctAllAudioSource != null && correctAllAudioSource.clip != null)
        {
            correctAllAudioSource.Stop();
            correctAllAudioSource.Play();
            return;
        }
        if (correctAllClip != null && fallbackAudioSource != null)
        {
            fallbackAudioSource.PlayOneShot(correctAllClip);
            return;
        }
        Debug.LogWarning("[WordSequenceChecker] No victory audio configured (optional).", this);
    }

    // ── Replay support ────────────────────────────────────────────────────────
    public void ResetPuzzle()
    {
        _correctCount   = 0;
        _puzzleComplete = false;
    }
}