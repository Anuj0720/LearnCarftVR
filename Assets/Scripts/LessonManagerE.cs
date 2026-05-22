using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.XR.Interaction.Toolkit;

public class LessonManagerE : MonoBehaviour
{
    [Header("Timeline")]
    public PlayableDirector questionTimeline;

    [Header("Demo Blocks")]
    public GameObject demoBlocksParent;

    [Header("Puzzle Blocks")]
    public GameObject puzzleBlocksParent;

    public HintManager hintManager;

    void Start()
    {
        // Prevent auto play
        questionTimeline.playOnAwake = false;

        // Disable puzzle blocks and grabbing at start
        puzzleBlocksParent.SetActive(false);
        DisableGrabbing();

        // Subscribe to timeline stopped event
        questionTimeline.stopped += OnQuestionFinished;
    }

    // -----------------------------
    // AFTER TIMELINE FINISHES
    // (TimelineTrigger plays it, we just react when it stops)
    // -----------------------------
    void OnQuestionFinished(PlayableDirector pd)
    {
        Debug.Log("Question Timeline Finished");

        // Hide demo blocks
        if (demoBlocksParent != null)
            demoBlocksParent.SetActive(false);

        // Enable puzzle blocks
        puzzleBlocksParent.SetActive(true);

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