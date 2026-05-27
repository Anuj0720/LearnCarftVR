using UnityEngine;
using UnityEngine.Playables;

/// <summary>
/// Manages the hint countdown for the word-sequence puzzle.
/// After 'hintDelay' seconds of no correct placement → shows 3D number hints.
/// </summary>
public class HintsManager : MonoBehaviour
{
    [Header("Timing")]
    [Tooltip("Seconds before the hint fires after the puzzle becomes active (or after a wrong placement).")]
    public float hintDelay = 20f;

    [Header("References")]
    [Tooltip("Optional robot/spotlight animation that plays with the hint.")]
    public PlayableDirector  hintTimeline;

    [Tooltip("Spawns 3D sequence numbers above each unsolved block.")]
    public HintNumberSpawner hintNumberSpawner;

    // ── State ─────────────────────────────────────────────────────────────────
    private float _timer        = 0f;
    private bool  _puzzleActive = false;
    private bool  _puzzleSolved = false;
    private bool  _hintPlayed   = false;

    // ── Update ────────────────────────────────────────────────────────────────

    void Update()
    {
        if (!_puzzleActive || _puzzleSolved) return;

        _timer += Time.deltaTime;

        if (!_hintPlayed && _timer >= hintDelay)
            PlayHint();
    }

    // ── Public API ────────────────────────────────────────────────────────────

    public void StartPuzzle()
    {
        _puzzleActive = true;
        _timer        = 0f;
        _puzzleSolved = false;
        _hintPlayed   = false;
        Debug.Log($"[HintsManager] Puzzle started. Hint fires in {hintDelay}s.");
    }

    public void ResetTimer()
    {
        if (_puzzleSolved) return;

        _timer      = 0f;
        _hintPlayed = false;

        if (hintNumberSpawner != null) hintNumberSpawner.HideAllHints();
        if (hintTimeline      != null && hintTimeline.state == PlayState.Playing)
            hintTimeline.Stop();

        Debug.Log("[HintsManager] Timer reset (wrong placement).");
    }

    public void PuzzleSolved()
    {
        _puzzleSolved = true;

        if (hintTimeline      != null && hintTimeline.state == PlayState.Playing)
            hintTimeline.Stop();
        if (hintNumberSpawner != null) hintNumberSpawner.HideAllHints();

        Debug.Log("[HintsManager] Puzzle solved — hints cleared.");
    }

    // ── Private ───────────────────────────────────────────────────────────────

    void PlayHint()
    {
        if (_puzzleSolved) return;

        _hintPlayed = true;
        Debug.Log("[HintsManager] Hint delay reached — showing sequence numbers.");

        if (hintTimeline      != null) hintTimeline.Play();
        if (hintNumberSpawner != null) hintNumberSpawner.ShowHints();
        else Debug.LogWarning("[HintsManager] HintNumberSpawner not assigned!", this);
    }
}