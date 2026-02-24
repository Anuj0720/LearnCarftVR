using UnityEngine;
using UnityEngine.Playables;

public class TimelineTrigger : MonoBehaviour
{
    public PlayableDirector timeline;
    private bool hasPlayed = false;

    private void Start()
    {
        Debug.Log("TimelineTrigger script started.");
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Something entered trigger: " + other.name);

        if (hasPlayed)
        {
            Debug.Log("Already played. Ignoring.");
            return;
        }

        if (other.CompareTag("Player"))
        {
            Debug.Log("PLAYER detected. Playing timeline.");

            if (timeline == null)
            {
                Debug.LogError("Timeline reference is NULL!");
                return;
            }

            hasPlayed = true;
            timeline.Play();
        }
        else
        {
            Debug.Log("Not Player. Tag was: " + other.tag);
        }
    }
}