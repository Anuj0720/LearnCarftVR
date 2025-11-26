using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;

public class TutorialManager : MonoBehaviour
{
    [Header("Deskbot System")]
    public AudioSource deskbotVoice;
    public AudioClip introClip;
    public AudioClip instructGrabClip;
    public AudioClip successClip;
    public DeskbotAnimationController deskbotAnimation;

    [Header("Interactable References")]
    public XRGrabInteractable cube;
    public XRSocketInteractor socket;

    [Header("Highlight Objects")]
    public HighlightObject cubeGlow;
    public HighlightObject socketGlow;

    private bool cubeGrabbed = false;
    private bool cubePlaced = false;

    void Awake()
    {
        // Ensure listeners exist BEFORE tutorial runs
        cube.selectEntered.AddListener(OnCubeGrabbed);
        socket.selectEntered.AddListener(OnCubePlaced);
    }

    void Start()
    {
        StartCoroutine(RunTutorial());
    }

    IEnumerator RunTutorial()
    {
        yield return new WaitForSeconds(1f);

        // 🔹 Intro Step
        deskbotAnimation.PlayIntro();
        deskbotVoice.PlayOneShot(introClip);

        yield return new WaitForSeconds(introClip.length + 1f);

        // 🔹 Teach Grab Step
        deskbotAnimation.PlayHint();
        deskbotVoice.PlayOneShot(instructGrabClip);

        cubeGlow.Highlight(true);
    }

    void OnCubeGrabbed(SelectEnterEventArgs args)
    {
        if (cubeGrabbed) return;
        cubeGrabbed = true;

        cubeGlow.Highlight(false);
        socketGlow.Highlight(true);

        // If you want a hint line here later, add it
    }

    void OnCubePlaced(SelectEnterEventArgs args)
    {
        if (cubePlaced) return;
        cubePlaced = true;

        socketGlow.Highlight(false);

        // 🔹 Success
        deskbotAnimation.PlaySuccess();
        deskbotVoice.PlayOneShot(successClip);

        StartCoroutine(ReturnToIdle(successClip.length));
    }

    IEnumerator ReturnToIdle(float delay)
    {
        yield return new WaitForSeconds(delay);
        deskbotAnimation.PlayIdle();
    }
}
