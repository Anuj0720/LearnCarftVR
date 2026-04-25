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
    public Animator robotAnimator;
    public string disappointedTriggerName = "Disappointed";

    // ✅ OPTION 1: AudioSource
    public AudioSource wrongAnswerAudioSource;

    // ✅ OPTION 2: AudioClip
    public AudioClip wrongAnswerClip;

    // ✅ NEW: Motivation AudioClip
    public AudioClip motivationAudioClip;

    [Header("Correct Answer Feedback")]

    // ✅ OPTION 1: AudioSource
    public AudioSource correctAnswerAudioSource;

    // ✅ OPTION 2: AudioClip
    public AudioClip correctAnswerClip;

    // Shared AudioSource (used if only clips are provided)
    public AudioSource fallbackAudioSource;

    private XRSocketInteractor socket;
    private bool puzzleCompleted = false;
    private bool isEjecting = false;

    void Start()
    {
        socket = GetComponent<XRSocketInteractor>();
        socket.selectEntered.AddListener(OnObjectPlaced);
    }

    void OnObjectPlaced(SelectEnterEventArgs args)
    {
        if (puzzleCompleted || isEjecting) return;

        BlockData block = args.interactableObject.transform.GetComponent<BlockData>();

        if (block != null)
        {
            if (block.value == correctAnswer)
            {
                Debug.Log("Correct Answer!");

                PlayCorrectSound();

                puzzleCompleted = true;
                hintManager.PuzzleSolved();
                Invoke(nameof(CompletePuzzle), 0.2f);
            }
            else
            {
                Debug.Log("Wrong Answer placed: " + block.value);

                isEjecting = true;

                TriggerDisappointedAnimation();
                PlayWrongSound();

                StartCoroutine(EjectNextFrame(args.interactableObject));

                hintManager.ResetTimer();
            }
        }
    }

    IEnumerator EjectNextFrame(IXRSelectInteractable interactable)
    {
        yield return null;

        if (interactable == null)
        {
            isEjecting = false;
            yield break;
        }

        if (socket.hasSelection)
        {
            socket.interactionManager.SelectExit(socket, interactable);
        }

        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();

        XRGrabInteractable grab = interactable.transform.GetComponent<XRGrabInteractable>();
        if (grab != null)
        {
            grab.enabled = true;
        }

        Rigidbody rb = interactable.transform.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            Vector3 ejectDirection = (interactable.transform.position - transform.position).normalized;
            if (ejectDirection == Vector3.zero) ejectDirection = Vector3.up;

            rb.AddForce(ejectDirection * 3f, ForceMode.Impulse);
        }

        yield return new WaitForSeconds(0.5f);

        isEjecting = false;
    }

    void TriggerDisappointedAnimation()
    {
        if (robotAnimator == null) return;

        foreach (AnimatorControllerParameter param in robotAnimator.parameters)
        {
            if (param.name == disappointedTriggerName &&
                param.type == AnimatorControllerParameterType.Trigger)
            {
                robotAnimator.SetTrigger(disappointedTriggerName);
                return;
            }
        }
    }

    // 🔊 WRONG SOUND + MOTIVATION
    void PlayWrongSound()
    {
        // Priority 1: AudioSource
        if (wrongAnswerAudioSource != null && wrongAnswerAudioSource.clip != null)
        {
            wrongAnswerAudioSource.Stop();
            wrongAnswerAudioSource.Play();

            StartCoroutine(PlayMotivationAfterDelay(wrongAnswerAudioSource.clip.length));
            return;
        }

        // Priority 2: Clip + fallback
        if (wrongAnswerClip != null && fallbackAudioSource != null)
        {
            fallbackAudioSource.PlayOneShot(wrongAnswerClip);

            StartCoroutine(PlayMotivationAfterDelay(wrongAnswerClip.length));
            return;
        }

        Debug.LogWarning("No wrong answer audio assigned!");
    }

    IEnumerator PlayMotivationAfterDelay(float delay)
    {
        if (motivationAudioClip == null || fallbackAudioSource == null)
            yield break;

        yield return new WaitForSeconds(delay);

        fallbackAudioSource.PlayOneShot(motivationAudioClip);
    }

    // 🔊 CORRECT SOUND
    void PlayCorrectSound()
    {
        if (correctAnswerAudioSource != null && correctAnswerAudioSource.clip != null)
        {
            correctAnswerAudioSource.Stop();
            correctAnswerAudioSource.Play();
            return;
        }

        if (correctAnswerClip != null && fallbackAudioSource != null)
        {
            fallbackAudioSource.PlayOneShot(correctAnswerClip);
            return;
        }

        Debug.LogWarning("No correct answer audio assigned!");
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