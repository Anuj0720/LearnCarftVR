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
        questionTimeline.playOnAwake = false;

        // Keep puzzleBlocksParent ACTIVE so Timeline can control its children
        // But hide each child individually at start
        HidePuzzleChildren();

        DisableGrabbing();

        questionTimeline.stopped += OnQuestionFinished;
    }

    // Hide all children of puzzleBlocksParent individually
    // so the parent stays active for Timeline to work
    void HidePuzzleChildren()
    {
        foreach (Transform child in puzzleBlocksParent.transform)
        {
            child.gameObject.SetActive(false);
        }

        Debug.Log("Puzzle children hidden individually, parent stays active.");
    }

    // -----------------------------
    // AFTER TIMELINE FINISHES
    // -----------------------------
    void OnQuestionFinished(PlayableDirector pd)
    {
        Debug.Log("Question Timeline Finished");

        // Hide demo blocks
        if (demoBlocksParent != null)
            demoBlocksParent.SetActive(false);

        // Make sure all puzzle children are visible after timeline ends
        ShowPuzzleChildren();

        hintManager.StartPuzzle();
        EnableGrabbing();
    }

    // Enable all children of puzzleBlocksParent
    void ShowPuzzleChildren()
    {
        foreach (Transform child in puzzleBlocksParent.transform)
        {
            child.gameObject.SetActive(true);
        }

        Debug.Log("All puzzle children shown.");
    }

    // -----------------------------
    // GRAB CONTROL
    // -----------------------------
    void DisableGrabbing()
    {
        // Use true to include inactive children
        XRGrabInteractable[] grabObjects =
            puzzleBlocksParent.GetComponentsInChildren<XRGrabInteractable>(true);

        foreach (XRGrabInteractable grab in grabObjects)
        {
            grab.enabled = false;
        }

        Debug.Log("Grabbing Disabled");
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