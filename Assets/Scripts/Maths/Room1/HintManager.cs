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
    }

    public void ResetTimer()
    {
        timer = 0f;
    }

    public void PuzzleSolved()
    {
        puzzleSolved = true;
    }

    void PlayHint1()
    {
        hint1Played = true;
        hint1Timeline.Play();
    }

    void PlayHint2()
    {
        hint2Played = true;
        hint2Timeline.Play();

        correctCubeRenderer.material.EnableKeyword("_EMISSION");
        correctCubeRenderer.material.SetColor("_EmissionColor", Color.yellow * 3f);
    }
}