using UnityEngine;
using UnityEngine.Playables;

public class HintManager : MonoBehaviour
{
    public float hintDelay = 20f;

    public PlayableDirector hintTimeline;

    public Renderer correctCubeRenderer;

    private float timer = 0f;
    private bool puzzleActive = false;
    private bool puzzleSolved = false;
    private bool hintPlayed = false;

    void Update()
    {
        if (!puzzleActive || puzzleSolved) return;

        timer += Time.deltaTime;

        if (!hintPlayed && timer >= hintDelay)
        {
            PlayHint();
        }
    }

    public void StartPuzzle()
    {
        puzzleActive = true;
        timer = 0f;
        puzzleSolved = false;
        hintPlayed = false;
    }

    public void ResetTimer()
    {
        if (!puzzleSolved)
            timer = 0f;
    }

    public void PuzzleSolved()
    {
        puzzleSolved = true;

        if (hintTimeline != null && hintTimeline.state == PlayState.Playing)
            hintTimeline.Stop();

        if (correctCubeRenderer != null)
            correctCubeRenderer.material.DisableKeyword("_EMISSION");
    }

    void PlayHint()
    {
        if (puzzleSolved) return;

        hintPlayed = true;
        hintTimeline.Play();

        if (correctCubeRenderer != null)
        {
            correctCubeRenderer.material.EnableKeyword("_EMISSION");
            correctCubeRenderer.material.SetColor("_EmissionColor", Color.yellow * 3f);
        }
    }
}