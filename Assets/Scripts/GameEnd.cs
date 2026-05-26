using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

public class GameEnd : MonoBehaviour
{
    // ─────────────────────────────────────────────────────────────────────────
    //  INSPECTOR FIELDS
    // ─────────────────────────────────────────────────────────────────────────

    [Header("Scene Names (must match Build Settings exactly)")]
    public string mathsSceneName   = "Maths";
    public string englishSceneName = "Scene_2";

    [Header("Celebration Timeline (Room4 of THIS scene)")]
    public PlayableDirector celebrationTimeline;

    [Header("Game Ending Timeline (assign in BOTH Maths and English scenes)")]
    public PlayableDirector gameEndingTimeline;

    // ─────────────────────────────────────────────────────────────────────────
    //  PRIVATE
    // ─────────────────────────────────────────────────────────────────────────

    private SceneLoader sceneLoader;
    private bool        sequenceStarted      = false;
    private bool        celebrationHasPlayed = false; // true once timeline enters Playing state
    private bool        watchingCelebration  = false; // true while we are polling Update

    private const string VISIT_COUNT_KEY  = "GameVisitCount";
    private const string FIRST_CHOICE_KEY = "FirstSceneChoice";

    // ─────────────────────────────────────────────────────────────────────────
    //  UNITY LIFECYCLE
    // ─────────────────────────────────────────────────────────────────────────

    void Start()
    {
        sceneLoader = FindObjectOfType<SceneLoader>();

        if (sceneLoader == null)
            Debug.LogWarning("GameEnd: No SceneLoader found in scene!");

        if (celebrationTimeline == null)
        {
            Debug.LogWarning("GameEnd: celebrationTimeline is NOT assigned!");
            return;
        }

        // Make sure gameEndingTimeline is stopped at scene start
        if (gameEndingTimeline != null)
            gameEndingTimeline.Stop();

        // Start watching the celebration timeline every frame
        watchingCelebration = true;

        Debug.Log($"GameEnd: Ready. Watching celebrationTimeline → {celebrationTimeline.name}");
    }

    void Update()
    {
        if (!watchingCelebration || celebrationTimeline == null) return;

        PlayableDirector pd = celebrationTimeline;

        // Step 1 — Detect that it has actually started playing
        if (!celebrationHasPlayed)
        {
            if (pd.state == PlayState.Playing)
            {
                celebrationHasPlayed = true;
                Debug.Log("GameEnd: Celebration timeline is NOW PLAYING.");
            }
            return; // wait until it starts before we check for finish
        }

        // Step 2 — It has played before; now detect it stopped/paused = finished
        if (celebrationHasPlayed && pd.state != PlayState.Playing)
        {
            watchingCelebration = false; // stop polling
            Debug.Log("GameEnd: Celebration timeline FINISHED.");
            OnCelebrationFinished();
        }
    }

    void OnDestroy()
    {
        watchingCelebration = false;
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  PUBLIC — called from Menu buttons
    // ─────────────────────────────────────────────────────────────────────────

    public void OnMenuChooseMaths()
    {
        ResetProgress();
        PlayerPrefs.SetString(FIRST_CHOICE_KEY, "Maths");
        PlayerPrefs.Save();
        sceneLoader.LoadSceneByName(mathsSceneName);
    }

    public void OnMenuChooseEnglish()
    {
        ResetProgress();
        PlayerPrefs.SetString(FIRST_CHOICE_KEY, "English");
        PlayerPrefs.Save();
        sceneLoader.LoadSceneByName(englishSceneName);
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  CELEBRATION FINISHED — called from Update once state leaves Playing
    // ─────────────────────────────────────────────────────────────────────────

    private void OnCelebrationFinished()
    {
        if (sequenceStarted) return;
        sequenceStarted = true;

        string firstChoice  = PlayerPrefs.GetString(FIRST_CHOICE_KEY, "");
        string currentScene = SceneManager.GetActiveScene().name;
        int    visitCount   = PlayerPrefs.GetInt(VISIT_COUNT_KEY, 0);

        Debug.Log($"GameEnd: Processing end | firstChoice={firstChoice} | currentScene={currentScene} | visitCount={visitCount}");

        if (IsLastScene(firstChoice, currentScene))
        {
            // ── Second scene completed → play game ending timeline ────────────
            Debug.Log("GameEnd: LAST SCENE → Starting game ending timeline.");
            StartCoroutine(PlayGameEndingAfterDelay());
        }
        else
        {
            // ── First scene completed → set count to 1 → go to next scene ────
            PlayerPrefs.SetInt(VISIT_COUNT_KEY, 1);
            PlayerPrefs.Save();

            string nextScene = (currentScene == mathsSceneName)
                ? englishSceneName
                : mathsSceneName;

            Debug.Log($"GameEnd: FIRST SCENE done. Count = 1. Transitioning to → {nextScene}");
            sceneLoader.LoadSceneByName(nextScene);
        }
    }

    // Small delay to let any celebrationTimeline cleanup finish
    // before game ending timeline starts
    private System.Collections.IEnumerator PlayGameEndingAfterDelay()
    {
        yield return new WaitForSeconds(0.5f);

        if (gameEndingTimeline != null)
        {
            gameEndingTimeline.Play();
            Debug.Log("GameEnd: Game ending timeline is now PLAYING.");

            // Wait for game ending timeline to finish, then go back to menu
            yield return StartCoroutine(WaitForTimelineAndGoToMenu());
        }
        else
        {
            Debug.LogWarning("GameEnd: gameEndingTimeline is NOT assigned!");
        }
    }

    // Polls game ending timeline until it finishes, then loads menu
    private System.Collections.IEnumerator WaitForTimelineAndGoToMenu()
    {
        // Wait for it to start playing first
        yield return new WaitUntil(() =>
            gameEndingTimeline != null &&
            gameEndingTimeline.state == PlayState.Playing);

        Debug.Log("GameEnd: Waiting for game ending timeline to finish...");

        // Wait until it stops
        yield return new WaitUntil(() =>
            gameEndingTimeline == null ||
            gameEndingTimeline.state != PlayState.Playing);

        Debug.Log("GameEnd: Game ending timeline FINISHED. Returning to menu.");

        ResetProgress();
        sceneLoader.LoadSceneByName("Menu");
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  LAST SCENE DETECTION
    //  Maths first  → last scene is English
    //  English first → last scene is Maths
    // ─────────────────────────────────────────────────────────────────────────

    private bool IsLastScene(string firstChoice, string currentScene)
    {
        if (firstChoice == "Maths"   && currentScene == englishSceneName) return true;
        if (firstChoice == "English" && currentScene == mathsSceneName)   return true;
        return false;
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  UTILITY
    // ─────────────────────────────────────────────────────────────────────────

    public void ResetProgress()
    {
        PlayerPrefs.SetInt(VISIT_COUNT_KEY, 0);
        PlayerPrefs.DeleteKey(FIRST_CHOICE_KEY);
        PlayerPrefs.Save();
        sequenceStarted      = false;
        celebrationHasPlayed = false;
        watchingCelebration  = celebrationTimeline != null;
        Debug.Log("GameEnd: Progress reset.");
    }
}