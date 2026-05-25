using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Manages a sequence of question timelines, each followed by a puzzle round.
/// After the player solves each puzzle (celebration timeline finishes),
/// the next question timeline plays automatically.
///
/// HOW TO SET UP IN THE INSPECTOR:
/// 1. Add this component to a manager GameObject.
/// 2. Fill "Question Timelines" with your ordered PlayableDirectors (Timeline1, Timeline2, ...).
/// 3. Assign "Answer Checker" — this script resets its internal state between rounds.
/// 4. Assign the single shared "Celebration Timeline" PlayableDirector.
/// 5. Assign Demo Blocks Parent, Puzzle Blocks Parent, and Hint Manager.
/// </summary>
public class SequentialLessonManager : MonoBehaviour
{
    [Header("Question Timelines (played in order)")]
    public List<PlayableDirector> questionTimelines = new List<PlayableDirector>();

    [Header("Celebration Timeline (shared, reused each round)")]
    public PlayableDirector celebrationTimeline;

    [Header("Answer Checker reference")]
    [Tooltip("The AnswerChecker whose state this manager resets between rounds.")]
    public AnswerChecker answerChecker;

    [Header("Scene References")]
    public GameObject demoBlocksParent;
    public GameObject puzzleBlocksParent;
    public HintManager hintManager;

    [Header("Settings")]
    [Tooltip("Delay in seconds between celebration ending and the next round starting.")]
    public float delayBetweenRounds = 1.0f;

    // ── private state ────────────────────────────────────────────────
    private int currentIndex = 0;

    // ================================================================
    void Start()
    {
        if (questionTimelines == null || questionTimelines.Count == 0)
        {
            Debug.LogError("[SequentialLessonManager] No question timelines assigned!");
            return;
        }

        foreach (var tl in questionTimelines)
            if (tl != null) tl.playOnAwake = false;

        if (celebrationTimeline != null)
            celebrationTimeline.playOnAwake = false;

        HidePuzzleChildren();
        DisableGrabbing();

        BeginRound(0);
    }

    // ================================================================
    //  ROUND LIFECYCLE
    // ================================================================

    void BeginRound(int index)
    {
        if (index >= questionTimelines.Count)
        {
            OnAllRoundsComplete();
            return;
        }

        currentIndex = index;
        Debug.Log($"[SequentialLessonManager] Starting round {index + 1}/{questionTimelines.Count}");

        // Reset scene state for this round
        HidePuzzleChildren();
        DisableGrabbing();

        // ── Reset AnswerChecker's private fields via reflection ──────
        // This avoids needing to modify AnswerChecker.cs at all.
        if (answerChecker != null)
        {
            // Assign the shared celebration timeline for this round
            answerChecker.celebrationTimeline = celebrationTimeline;

            // Reset puzzleCompleted and isEjecting via reflection
            var type = typeof(AnswerChecker);

            var puzzleCompletedField = type.GetField(
                "puzzleCompleted",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            var isEjectingField = type.GetField(
                "isEjecting",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (puzzleCompletedField != null) puzzleCompletedField.SetValue(answerChecker, false);
            if (isEjectingField != null)      isEjectingField.SetValue(answerChecker, false);

            Debug.Log("[SequentialLessonManager] AnswerChecker reset for new round.");
        }

        // Subscribe to question timeline finish, then play it
        PlayableDirector current = questionTimelines[index];
        current.stopped += OnQuestionTimelineFinished;
        current.Play();
    }

    void OnQuestionTimelineFinished(PlayableDirector pd)
    {
        pd.stopped -= OnQuestionTimelineFinished;

        Debug.Log($"[SequentialLessonManager] Question timeline {currentIndex + 1} finished.");

        if (demoBlocksParent != null)
            demoBlocksParent.SetActive(false);

        ShowPuzzleChildren();
        EnableGrabbing();

        if (hintManager != null)
            hintManager.StartPuzzle();

        // Wait for celebration to finish before advancing to the next round
        if (celebrationTimeline != null)
            celebrationTimeline.stopped += OnCelebrationFinished;
    }

    void OnCelebrationFinished(PlayableDirector pd)
    {
        pd.stopped -= OnCelebrationFinished;

        Debug.Log($"[SequentialLessonManager] Celebration done — advancing to round {currentIndex + 2}.");

        StartCoroutine(AdvanceAfterDelay(delayBetweenRounds));
    }

    IEnumerator AdvanceAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        BeginRound(currentIndex + 1);
    }

    void OnAllRoundsComplete()
    {
        Debug.Log("[SequentialLessonManager] All rounds complete! Lesson finished.");
        // Add your own end-of-lesson logic here (e.g. fade out, show score screen, etc.)
    }

    // ================================================================
    //  SCENE HELPERS
    // ================================================================

    void HidePuzzleChildren()
    {
        if (puzzleBlocksParent == null) return;
        foreach (Transform child in puzzleBlocksParent.transform)
            child.gameObject.SetActive(false);
    }

    void ShowPuzzleChildren()
    {
        if (puzzleBlocksParent == null) return;
        foreach (Transform child in puzzleBlocksParent.transform)
            child.gameObject.SetActive(true);
    }

    void DisableGrabbing()
    {
        if (puzzleBlocksParent == null) return;
        foreach (var grab in puzzleBlocksParent.GetComponentsInChildren<XRGrabInteractable>(true))
            grab.enabled = false;
    }

    void EnableGrabbing()
    {
        if (puzzleBlocksParent == null) return;
        foreach (var grab in puzzleBlocksParent.GetComponentsInChildren<XRGrabInteractable>(true))
            grab.enabled = true;
    }
}