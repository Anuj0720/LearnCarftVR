using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.Playables;

public class AnswerChecker : MonoBehaviour
{
    public int correctAnswer = 4;
    public PlayableDirector celebrationTimeline;
    public GameObject puzzleBlocksParent;
    public HintManager hintManager;

    private XRSocketInteractor socket;
    private bool puzzleCompleted = false;

    void Start()
    {
        socket = GetComponent<XRSocketInteractor>();
        socket.selectEntered.AddListener(OnObjectPlaced);
    }

    void OnObjectPlaced(SelectEnterEventArgs args)
    {
        if (puzzleCompleted) return;

        BlockData block = args.interactableObject.transform.GetComponent<BlockData>();

        if (block != null)
        {
            if (block.value == correctAnswer)
            {
                Debug.Log("Correct Answer!");

                puzzleCompleted = true;
                hintManager.PuzzleSolved();

                // Delay celebration slightly to allow proper snapping
                Invoke(nameof(CompletePuzzle), 0.2f);
            }
            else
            {
                Debug.Log("Wrong Answer");
                hintManager.ResetTimer();
            }
        }
    }

    void CompletePuzzle()
    {
        DisablePuzzleGrabbing();
        celebrationTimeline.Play();
    }

    void DisablePuzzleGrabbing()
    {
        XRGrabInteractable[] grabs =
            puzzleBlocksParent.GetComponentsInChildren<XRGrabInteractable>();

        foreach (XRGrabInteractable grab in grabs)
        {
            grab.enabled = false;
        }
    }
}