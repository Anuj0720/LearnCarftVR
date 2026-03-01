using UnityEngine;
using UnityEngine.Playables;

public class HintManager : MonoBehaviour
{
    public float hint1Delay = 15f;
    public float hint2Delay = 30f;

    public PlayableDirector hint1Timeline;
    public PlayableDirector hint2Timeline;

    public Renderer correctCubeRenderer;

    private float timer = 0f;
    private bool puzzleActive = false;
    private bool puzzleSolved = false;
    private bool hint1Played = false;
    private bool hint2Played = false;

    void Update()
    {
        // Stop processing completely if puzzle is solved
        if (!puzzleActive || puzzleSolved) return;

        timer += Time.deltaTime;

        if (!hint1Played && timer >= hint1Delay)
        {
            PlayHint1();
        }

        if (!hint2Played && timer >= hint2Delay)
        {
            PlayHint2();
        }
    }

    public void StartPuzzle()
    {
        puzzleActive = true;
        timer = 0f;
        puzzleSolved = false;
        hint1Played = false;
        hint2Played = false;
    }

    public void ResetTimer()
    {
        // Only reset if puzzle not solved yet
        if (!puzzleSolved)
        {
            timer = 0f;
        }
    }

    public void PuzzleSolved()
    {
        puzzleSolved = true;

        // Stop any currently playing hint timelines immediately
        if (hint1Timeline != null && hint1Timeline.state == PlayState.Playing)
        {
            hint1Timeline.Stop();
        }

        if (hint2Timeline != null && hint2Timeline.state == PlayState.Playing)
        {
            hint2Timeline.Stop();
        }

        // Turn off the cube highlight emission if hint 2 had activated it
        if (correctCubeRenderer != null)
        {
            correctCubeRenderer.material.DisableKeyword("_EMISSION");
        }
    }

    void PlayHint1()
    {
        // Double-check puzzle isn't solved before playing
        if (puzzleSolved) return;

        hint1Played = true;
        hint1Timeline.Play();
    }

    void PlayHint2()
    {
        // Double-check puzzle isn't solved before playing
        if (puzzleSolved) return;

        hint2Played = true;
        hint2Timeline.Play();

        if (correctCubeRenderer != null)
        {
            correctCubeRenderer.material.EnableKeyword("_EMISSION");
            correctCubeRenderer.material.SetColor("_EmissionColor", Color.yellow * 3f);
        }
    }
}