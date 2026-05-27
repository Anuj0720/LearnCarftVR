using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Controls the two-phase English lesson.
/// Phase 1 – Teaching Timeline  : demo blocks show correct sentence order.
/// Phase 2 – Question Timeline  : shuffled word blocks appear via activation tracks.
/// After Phase 2 ends grabbing is enabled and the hint countdown starts.
/// </summary>
public class LessonsManager : MonoBehaviour
{
    [Header("Timelines")]
    public PlayableDirector teachingTimeline;
    public PlayableDirector questionTimeline;

    [Header("Demo Blocks  (Phase 1)")]
    public GameObject demoBlocksParent;

    [Header("Puzzle Blocks  (Phase 2)")]
    [Tooltip("Parent that holds ALL puzzle word blocks. " +
             "Individual blocks are shown/hidden by Timeline activation tracks inside questionTimeline.")]
    public GameObject puzzleBlocksParent;

    [Header("References")]
    public HintsManager hintsManager;

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    void Start()
    {
        // Validate
        if (teachingTimeline == null) Debug.LogError("[LessonsManager] teachingTimeline not assigned!", this);
        if (questionTimeline == null) Debug.LogError("[LessonsManager] questionTimeline not assigned!", this);
        if (hintsManager     == null) Debug.LogError("[LessonsManager] hintsManager not assigned!", this);

        // Puzzle blocks parent stays ACTIVE so Timeline activation tracks work,
        // but no individual block should be active yet — that's handled by the timeline.
        // Only disable grabbing at the start.
        if (puzzleBlocksParent != null)
            DisablePuzzleGrabbing();

        if (teachingTimeline != null) teachingTimeline.stopped += OnTeachingFinished;
        if (questionTimeline != null) questionTimeline.stopped += OnQuestionFinished;
    }

    void OnDestroy()
    {
        if (teachingTimeline != null) teachingTimeline.stopped -= OnTeachingFinished;
        if (questionTimeline != null) questionTimeline.stopped -= OnQuestionFinished;
    }

    // ── Timeline callbacks ────────────────────────────────────────────────────

    void OnTeachingFinished(PlayableDirector pd)
    {
        Debug.Log("[LessonsManager] Teaching finished → starting question phase.");
        if (demoBlocksParent != null) demoBlocksParent.SetActive(false);
        if (questionTimeline != null) questionTimeline.Play();
    }

    void OnQuestionFinished(PlayableDirector pd)
    {
        Debug.Log("[LessonsManager] Question timeline finished → enabling grabbing.");
        EnablePuzzleGrabbing();
        if (hintsManager != null) hintsManager.StartPuzzle();
    }

    // ── Grab helpers ──────────────────────────────────────────────────────────

    void DisablePuzzleGrabbing()
    {
        foreach (XRGrabInteractable g in
                 puzzleBlocksParent.GetComponentsInChildren<XRGrabInteractable>(true))
            g.enabled = false;
    }

    void EnablePuzzleGrabbing()
    {
        if (puzzleBlocksParent == null) return;
        foreach (XRGrabInteractable g in
                 puzzleBlocksParent.GetComponentsInChildren<XRGrabInteractable>(true))
            g.enabled = true;

        Debug.Log("[LessonsManager] Grabbing enabled.");
    }
}