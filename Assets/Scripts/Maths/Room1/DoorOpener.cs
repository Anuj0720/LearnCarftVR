using System.Collections;
using UnityEngine;
using UnityEngine.Playables;

public class DoorOpener : MonoBehaviour
{
    [Header("Door Parts")]
    public Transform doorLeft;
    public Transform doorRight;

    [Header("Target Local Positions (SET IN INSPECTOR)")]
    public Vector3 doorLeftOpenPosition;
    public Vector3 doorRightOpenPosition;

    [Header("Animation Settings")]
    public float openSpeed = 2f;

    [Header("Timeline")]
    public PlayableDirector celebrationTimeline;

    [Header("Audio")]
    public AudioSource doorAudioSource;
    public AudioClip doorOpenClip;

    private Vector3 leftStartPos;
    private Vector3 rightStartPos;

    private bool hasOpened = false;

    private Animator doorAnimator;

    void Start()
    {
        // ✅ Store LOCAL positions (IMPORTANT)
        leftStartPos = doorLeft.localPosition;
        rightStartPos = doorRight.localPosition;

        // ✅ Disable animator (THIS WAS YOUR MAIN ISSUE)
        doorAnimator = GetComponent<Animator>();
        if (doorAnimator != null)
        {
            doorAnimator.enabled = false;
        }

        StartCoroutine(WaitForTimelineEnd());
    }

    IEnumerator WaitForTimelineEnd()
    {
        // Wait until timeline starts
        while (celebrationTimeline != null && celebrationTimeline.state != PlayState.Playing)
            yield return null;

        // Wait until timeline finishes
        while (celebrationTimeline != null && celebrationTimeline.state == PlayState.Playing)
            yield return null;

        // Small safety delay (optional but helps)
        yield return new WaitForSeconds(0.1f);

        OpenDoor();
    }

    void OpenDoor()
    {
        if (hasOpened) return;
        hasOpened = true;

        Debug.Log("DOOR OPEN TRIGGERED");

        // 🔊 Play sound
        if (doorAudioSource != null && doorOpenClip != null)
        {
            doorAudioSource.PlayOneShot(doorOpenClip);
        }

        StartCoroutine(AnimateDoor());
    }

    IEnumerator AnimateDoor()
    {
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * openSpeed;

            // ✅ Use LOCAL POSITION (VERY IMPORTANT)
            doorLeft.localPosition = Vector3.Lerp(leftStartPos, doorLeftOpenPosition, t);
            doorRight.localPosition = Vector3.Lerp(rightStartPos, doorRightOpenPosition, t);

            yield return null;
        }

        // Ensure exact final positions
        doorLeft.localPosition = doorLeftOpenPosition;
        doorRight.localPosition = doorRightOpenPosition;

        Debug.Log("DOOR OPEN COMPLETE");
    }
}