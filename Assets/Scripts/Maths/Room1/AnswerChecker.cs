using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.Playables;

public class AnswerChecker : MonoBehaviour
{
    public int correctAnswer = 4;
    public PlayableDirector celebrationTimeline;
    public GameObject puzzleBlocksParent;
    public HintManager hintManager;

    [Header("Wrong Answer Feedback")]
    [Tooltip("Drag your Room1_Bot GameObject here")]
    public Animator robotAnimator;
    [Tooltip("Must match the Trigger parameter name in the Animator exactly")]
    public string disappointedTriggerName = "Disappointed";
    [Tooltip("AudioSource with your wrong answer clip assigned")]
    public AudioSource wrongAnswerAudioSource;

    private XRSocketInteractor socket;
    private bool puzzleCompleted = false;

    // Prevents the socket from re-accepting the same block while it's being ejected
    private bool isEjecting = false;

    void Start()
    {
        socket = GetComponent<XRSocketInteractor>();
        socket.selectEntered.AddListener(OnObjectPlaced);

        if (robotAnimator == null)
            Debug.LogWarning("AnswerChecker: Robot Animator is NOT assigned in the Inspector!", this);
        if (wrongAnswerAudioSource == null)
            Debug.LogWarning("AnswerChecker: Wrong Answer AudioSource is NOT assigned in the Inspector!", this);
    }

    void OnObjectPlaced(SelectEnterEventArgs args)
    {
        // Ignore if puzzle is done OR we're already in the middle of ejecting a block
        if (puzzleCompleted || isEjecting) return;

        BlockData block = args.interactableObject.transform.GetComponent<BlockData>();

        if (block != null)
        {
            if (block.value == correctAnswer)
            {
                Debug.Log("Correct Answer!");
                puzzleCompleted = true;
                hintManager.PuzzleSolved();
                Invoke(nameof(CompletePuzzle), 0.2f);
            }
            else
            {
                Debug.Log("Wrong Answer placed: " + block.value);

                // Lock immediately so no further selectEntered events are processed
                // until this block is fully ejected and away from the socket
                isEjecting = true;

                // Fire animation and sound right now before touching the XR system
                TriggerDisappointedAnimation();
                PlayWrongSound();

                StartCoroutine(EjectNextFrame(args.interactableObject));

                hintManager.ResetTimer();
            }
        }
    }

    IEnumerator EjectNextFrame(IXRSelectInteractable interactable)
    {
        // Wait one frame for XR selectEntered event to fully finish
        yield return null;

        if (interactable == null)
        {
            isEjecting = false;
            yield break;
        }

        // Release the block from the socket
        if (socket.hasSelection)
        {
            socket.interactionManager.SelectExit(socket, interactable);
        }

        // Wait for physics to settle after SelectExit
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();

        // Re-enable grabbing so the player can pick it up again
        XRGrabInteractable grab = interactable.transform.GetComponent<XRGrabInteractable>();
        if (grab != null)
        {
            grab.enabled = true;
        }

        // Push block away from socket so it physically moves out of range
        Rigidbody rb = interactable.transform.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            Vector3 ejectDirection = (interactable.transform.position - transform.position).normalized;
            if (ejectDirection == Vector3.zero) ejectDirection = Vector3.up;
            // Strong enough push to move it well clear of the socket trigger zone
            rb.AddForce(ejectDirection * 3f, ForceMode.Impulse);
        }

        // Wait long enough for the block to physically travel away from the socket
        // before we allow the socket to accept anything again
        yield return new WaitForSeconds(0.5f);

        isEjecting = false;
        Debug.Log("AnswerChecker: Ejection complete, socket ready again.");
    }

    void TriggerDisappointedAnimation()
    {
        if (robotAnimator == null)
        {
            Debug.LogError("AnswerChecker: Robot Animator is null! Assign it in the Inspector.", this);
            return;
        }

        bool paramExists = false;
        foreach (AnimatorControllerParameter param in robotAnimator.parameters)
        {
            if (param.name == disappointedTriggerName && param.type == AnimatorControllerParameterType.Trigger)
            {
                paramExists = true;
                break;
            }
        }

        if (paramExists)
        {
            robotAnimator.SetTrigger(disappointedTriggerName);
            Debug.Log("AnswerChecker: Disappointed trigger fired.");
        }
        else
        {
            Debug.LogError($"AnswerChecker: Trigger '{disappointedTriggerName}' not found in Animator! " +
                           $"Add it via Animator → Parameters → '+' → Trigger → name it '{disappointedTriggerName}'.", this);
        }
    }

    void PlayWrongSound()
    {
        if (wrongAnswerAudioSource == null)
        {
            Debug.LogError("AnswerChecker: AudioSource is null! Assign it in the Inspector.", this);
            return;
        }

        if (wrongAnswerAudioSource.clip == null)
        {
            Debug.LogError("AnswerChecker: AudioSource has no clip! Drag your audio file onto the AudioSource component.", this);
            return;
        }

        // Stop any currently playing instance before playing again
        // so it never overlaps with itself even if somehow called twice
        wrongAnswerAudioSource.Stop();
        wrongAnswerAudioSource.Play();
        Debug.Log("AnswerChecker: Wrong answer sound played.");
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