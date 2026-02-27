using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.XR.Interaction.Toolkit;

public class LessonManager : MonoBehaviour
{
    [Header("Timelines")]
    public PlayableDirector teachingTimeline;
    public PlayableDirector questionTimeline;

    [Header("Demo Blocks (Timeline 1)")]
    public GameObject demoBlocksParent;

    [Header("Puzzle Blocks (Timeline 2)")]
    public GameObject puzzleBlocksParent;

    public HintManager hintManager;

    void Start()
    {
        // Make sure puzzle blocks are disabled at start
        puzzleBlocksParent.SetActive(false);

        // Disable grabbing initially
        DisableGrabbing();

        // Subscribe to timeline events
        teachingTimeline.stopped += OnTeachingFinished;
        questionTimeline.stopped += OnQuestionFinished;
    }

    // -----------------------------
    // AFTER TIMELINE 1 FINISHES
    // -----------------------------
    void OnTeachingFinished(PlayableDirector pd)
    {
        Debug.Log("Teaching Timeline Finished");

        // Remove demo cubes
        demoBlocksParent.SetActive(false);

        // Enable puzzle block parent (timeline will activate individual cubes)
        puzzleBlocksParent.SetActive(true);

        // Start second timeline
        questionTimeline.Play();
    }

    // -----------------------------
    // AFTER TIMELINE 2 FINISHES
    // -----------------------------
    void OnQuestionFinished(PlayableDirector pd)
    {
        Debug.Log("Question Timeline Finished");
        hintManager.StartPuzzle();

        EnableGrabbing();
    }

    // -----------------------------
    // GRAB CONTROL
    // -----------------------------
    void DisableGrabbing()
    {
        XRGrabInteractable[] grabObjects =
            puzzleBlocksParent.GetComponentsInChildren<XRGrabInteractable>(true);

        foreach (XRGrabInteractable grab in grabObjects)
        {
            grab.enabled = false;
        }
    }

    void EnableGrabbing()
    {
        XRGrabInteractable[] grabObjects =
            FindObjectsOfType<XRGrabInteractable>(true);

        foreach (XRGrabInteractable grab in grabObjects)
        {
            grab.enabled = true;
        }

        Debug.Log("Grabbing Enabled");
    }
}