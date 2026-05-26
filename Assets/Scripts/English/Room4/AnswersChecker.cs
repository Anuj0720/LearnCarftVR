using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.Playables;

public class AnswersChecker : MonoBehaviour
{
    [Header("Questions Timeline Controller")]
    public QuestionsTimeline questionsTimeline;

    [Header("Correct Answers Per Timeline (match order)")]
    public int[] correctAnswers;

    [Header("Puzzle Blocks Parent")]
    public GameObject puzzleBlocksParent;

    [Header("Hint Manager")]
    public HintManager hintManager;

    [Header("Wrong Answer Feedback")]
    public Animator robotAnimator;
    public string disappointedTriggerName = "Disappointed";

    public AudioSource wrongAnswerAudioSource;
    public AudioClip wrongAnswerClip;
    public AudioClip motivationAudioClip;

    [Header("Correct Answer Feedback")]
    public AudioSource correctAnswerAudioSource;
    public AudioClip correctAnswerClip;

    public AudioSource fallbackAudioSource;

    [Header("Celebration Timeline (Final)")]
    public PlayableDirector finalCelebrationTimeline;

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

        if (block == null) return;

        int currentIndex  = questionsTimeline.GetCurrentIndex();

        // Safety check
        if (currentIndex >= correctAnswers.Length)
        {
            Debug.LogWarning("AnswersChecker: No correct answer defined for index " + currentIndex);
            return;
        }

        int expectedAnswer = correctAnswers[currentIndex];

        if (block.value == expectedAnswer)
        {
            Debug.Log("Correct Answer for Timeline " + currentIndex + "!");

            PlayCorrectSound();

            // If this is the last question
            if (questionsTimeline.IsLastTimeline())
            {
                puzzleCompleted = true;
                hintManager.PuzzleSolved();
                Invoke(nameof(CompleteFinalPuzzle), 0.2f);
            }
            else
            {
                // Move to the next timeline
                Invoke(nameof(GoToNextTimeline), 0.2f);
            }
        }
        else
        {
            Debug.Log("Wrong Answer: " + block.value + " | Expected: " + expectedAnswer);

            isEjecting = true;

            TriggerDisappointedAnimation();
            PlayWrongSound();

            StartCoroutine(EjectNextFrame(args.interactableObject));

            hintManager.ResetTimer();
        }
    }

    void GoToNextTimeline()
    {
        DisablePuzzleGrabbing();
        questionsTimeline.OnCorrectAnswerGiven();
        EnablePuzzleGrabbing();
    }

    void CompleteFinalPuzzle()
    {
        DisablePuzzleGrabbing();

        if (finalCelebrationTimeline != null)
            finalCelebrationTimeline.Play();
        else
            Debug.LogWarning("AnswersChecker: No final celebration timeline assigned!");
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
            socket.interactionManager.SelectExit(socket, interactable);

        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();

        XRGrabInteractable grab = interactable.transform.GetComponent<XRGrabInteractable>();
        if (grab != null)
            grab.enabled = true;

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

    void PlayWrongSound()
    {
        if (wrongAnswerAudioSource != null && wrongAnswerAudioSource.clip != null)
        {
            wrongAnswerAudioSource.Stop();
            wrongAnswerAudioSource.Play();
            StartCoroutine(PlayMotivationAfterDelay(wrongAnswerAudioSource.clip.length));
            return;
        }

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

    void DisablePuzzleGrabbing()
    {
        if (puzzleBlocksParent == null) return;
        XRGrabInteractable[] grabs = puzzleBlocksParent.GetComponentsInChildren<XRGrabInteractable>();
        foreach (XRGrabInteractable grab in grabs)
            grab.enabled = false;
    }

    void EnablePuzzleGrabbing()
    {
        if (puzzleBlocksParent == null) return;
        XRGrabInteractable[] grabs = puzzleBlocksParent.GetComponentsInChildren<XRGrabInteractable>();
        foreach (XRGrabInteractable grab in grabs)
            grab.enabled = true;
    }
}