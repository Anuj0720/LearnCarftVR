using System.Collections;
using UnityEngine;
using UnityEngine.Playables;

public class DoorOpener2 : MonoBehaviour
{
    [Header("Door")]
    public Transform door;

    [Header("Target Local Rotation (SET IN INSPECTOR)")]
    public Vector3 doorOpenRotation;

    [Header("Animation Settings")]
    public float openSpeed = 2f;

    [Header("Timeline")]
    public PlayableDirector celebrationTimeline;

    [Header("Audio")]
    public AudioSource doorAudioSource;
    public AudioClip doorOpenClip;

    private Quaternion startRotation;
    private Quaternion openRotation;

    private bool hasOpened = false;

    private Animator doorAnimator;

    void Start()
    {
        // Store the door's starting local rotation
        startRotation = door.localRotation;
        openRotation  = Quaternion.Euler(doorOpenRotation);

        // Disable animator so it doesn't fight the script
        doorAnimator = GetComponent<Animator>();
        if (doorAnimator != null)
            doorAnimator.enabled = false;

        StartCoroutine(WaitForTimelineEnd());
    }

    IEnumerator WaitForTimelineEnd()
    {
        // Wait until the timeline starts playing
        while (celebrationTimeline != null && celebrationTimeline.state != PlayState.Playing)
            yield return null;

        // Wait until the timeline finishes
        while (celebrationTimeline != null && celebrationTimeline.state == PlayState.Playing)
            yield return null;

        yield return new WaitForSeconds(0.1f);

        OpenDoor();
    }

    void OpenDoor()
    {
        if (hasOpened) return;
        hasOpened = true;

        Debug.Log("DOOR 2 OPEN TRIGGERED");

        if (doorAudioSource != null && doorOpenClip != null)
            doorAudioSource.PlayOneShot(doorOpenClip);

        StartCoroutine(AnimateDoor());
    }

    IEnumerator AnimateDoor()
    {
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * openSpeed;
            door.localRotation = Quaternion.Lerp(startRotation, openRotation, t);
            yield return null;
        }

        // Ensure exact final rotation
        door.localRotation = openRotation;

        Debug.Log("DOOR 2 OPEN COMPLETE");
    }
}