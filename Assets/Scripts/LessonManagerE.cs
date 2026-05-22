using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.XR.Interaction.Toolkit;

public class LessonManagerE : MonoBehaviour
{
    [Header("Timeline")]
    public PlayableDirector questionTimeline;

    [Header("Puzzle Blocks")]
    public GameObject puzzleBlocksParent;

    public HintManager hintManager;

    void Start()
    {
        // Disable puzzle blocks and grabbing at start
        puzzleBlocksParent.SetActive(false);
        DisableGrabbing();

        // Subscribe to timeline event
        questionTimeline.stopped += OnQuestionFinished;

        // Play question timeline immediately
        questionTimeline.Play();
    }

    // -----------------------------
    // AFTER TIMELINE FINISHES
    // -----------------------------
    void OnQuestionFinished(PlayableDirector pd)
    {
        Debug.Log("Question Timeline Finished");

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