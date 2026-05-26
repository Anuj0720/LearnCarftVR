using UnityEngine;
using UnityEngine.Playables;

public class QuestionsTimeline : MonoBehaviour
{
    [Header("Timelines in Sequence")]
    public PlayableDirector[] timelines;

    private int currentIndex = 0;

    void Start()
    {
        if (timelines.Length == 0)
        {
            Debug.LogWarning("QuestionsTimeline: No timelines assigned!");
            return;
        }

        // Subscribe to all timeline stopped events
        foreach (PlayableDirector timeline in timelines)
        {
            if (timeline != null)
                timeline.stopped += OnTimelineStopped;
        }

        // Play the first timeline
        PlayCurrentTimeline();
    }

    void OnDisable()
    {
        foreach (PlayableDirector timeline in timelines)
        {
            if (timeline != null)
                timeline.stopped -= OnTimelineStopped;
        }
    }

    void OnTimelineStopped(PlayableDirector director)
    {
        // Ignore if this isn't the current timeline
        if (director != timelines[currentIndex]) return;

        Debug.Log("Timeline " + currentIndex + " finished.");
    }

    public void PlayCurrentTimeline()
    {
        if (currentIndex >= timelines.Length)
        {
            Debug.Log("QuestionsTimeline: All timelines completed!");
            return;
        }

        if (timelines[currentIndex] != null)
        {
            Debug.Log("Playing Timeline: " + currentIndex);
            timelines[currentIndex].Play();
        }
    }

    public void OnCorrectAnswerGiven()
    {
        currentIndex++;
        PlayCurrentTimeline();
    }

    public int GetCurrentIndex()
    {
        return currentIndex;
    }

    public bool IsLastTimeline()
    {
        return currentIndex >= timelines.Length;
    }
}