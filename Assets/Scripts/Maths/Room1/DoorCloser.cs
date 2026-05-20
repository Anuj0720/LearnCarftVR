using System.Collections;
using UnityEngine;

public class DoorCloser : MonoBehaviour
{
    [Header("Door Parts")]
    public Transform doorLeft;
    public Transform doorRight;

    [Header("Target Local Positions (SET IN INSPECTOR)")]
    public Vector3 doorLeftClosedPosition;
    public Vector3 doorRightClosedPosition;

    [Header("Animation Settings")]
    [Tooltip("Time in SECONDS to fully close the door")]
    public float closeDuration = 2f;  // <-- Now this means actual seconds, not speed

    [Header("Audio")]
    public AudioSource doorAudioSource;
    public AudioClip doorCloseClip;

    private Vector3 leftStartPos;
    private Vector3 rightStartPos;

    private bool hasClosed = false;
    private Animator doorAnimator;

    void Start()
    {
        leftStartPos = doorLeft.localPosition;
        rightStartPos = doorRight.localPosition;

        doorAnimator = GetComponent<Animator>();
        if (doorAnimator != null)
        {
            doorAnimator.enabled = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasClosed) return;
        CloseDoor();
    }

    void CloseDoor()
    {
        if (hasClosed) return;
        hasClosed = true;

        Debug.Log("DOOR CLOSE TRIGGERED");

        if (doorAudioSource != null && doorCloseClip != null)
        {
            doorAudioSource.PlayOneShot(doorCloseClip);
        }

        StartCoroutine(AnimateDoor());
    }

    IEnumerator AnimateDoor()
    {
        float elapsed = 0f;

        while (elapsed < closeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / closeDuration); // 0 → 1 over closeDuration seconds
            float smoothT = Mathf.SmoothStep(0f, 1f, t);     // smooth ease in/out

            doorLeft.localPosition = Vector3.Lerp(leftStartPos, doorLeftClosedPosition, smoothT);
            doorRight.localPosition = Vector3.Lerp(rightStartPos, doorRightClosedPosition, smoothT);

            yield return null;
        }

        doorLeft.localPosition = doorLeftClosedPosition;
        doorRight.localPosition = doorRightClosedPosition;

        Debug.Log("DOOR CLOSE COMPLETE");
    }
}