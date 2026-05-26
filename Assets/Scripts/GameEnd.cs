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

    [Header("Game Ending Timeline (assign in BOTH scenes)")]
    public PlayableDirector gameEndingTimeline;

    // ─────────────────────────────────────────────────────────────────────────
    //  PRIVATE
    // ─────────────────────────────────────────────────────────────────────────

    private SceneLoader sceneLoader;
    private bool        sequenceStarted = false;

    private const string VISIT_COUNT_KEY  = "GameVisitCount";
    private const string FIRST_CHOICE_KEY = "FirstSceneChoice";

    // ─────────────────────────────────────────────────────────────────────────
    //  UNITY LIFECYCLE
    // ─────────────────────────────────────────────────────────────────────────

    void Start()
    {
        // Grab SceneLoader from the scene automatically
        sceneLoader = FindObjectOfType<SceneLoader>();

        if (sceneLoader == null)
            Debug.LogWarning("GameEnd: No SceneLoader found in scene!");

        if (celebrationTimeline != null)
            celebrationTimeline.stopped += OnCelebrationFinished;
    }

    void OnDestroy()
    {
        if (celebrationTimeline != null)
            celebrationTimeline.stopped -= OnCelebrationFinished;
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  PUBLIC — called from Menu buttons
    // ─────────────────────────────────────────────────────────────────────────

    public void OnMenuChooseMaths()
    {
        ResetProgress();
        PlayerPrefs.SetString(FIRST_CHOICE_KEY, "Maths");
        PlayerPrefs.Save();
        sceneLoader.LoadSceneByName(mathsSceneName);   // SceneLoader handles fade + sound
    }

    public void OnMenuChooseEnglish()
    {
        ResetProgress();
        PlayerPrefs.SetString(FIRST_CHOICE_KEY, "English");
        PlayerPrefs.Save();
        sceneLoader.LoadSceneByName(englishSceneName); // SceneLoader handles fade + sound
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  CELEBRATION FINISHED CALLBACK
    // ─────────────────────────────────────────────────────────────────────────

    private void OnCelebrationFinished(PlayableDirector director)
    {
        if (sequenceStarted) return;
        sequenceStarted = true;

        string firstChoice  = PlayerPrefs.GetString(FIRST_CHOICE_KEY, "");
        string currentScene = SceneManager.GetActiveScene().name;

        Debug.Log($"GameEnd: Celebration ended | firstChoice={firstChoice} | currentScene={currentScene}");

        if (IsLastScene(firstChoice, currentScene))
        {
            Debug.Log("GameEnd: Last scene detected → playing game ending timeline.");
            PlayGameEndingTimeline();
        }
        else
        {
            // Increment count and move to next scene via SceneLoader
            int count = PlayerPrefs.GetInt(VISIT_COUNT_KEY, 0) + 1;
            PlayerPrefs.SetInt(VISIT_COUNT_KEY, count);
            PlayerPrefs.Save();

            string nextScene = (currentScene == mathsSceneName) ? englishSceneName : mathsSceneName;
            Debug.Log($"GameEnd: Moving to next scene → {nextScene}");
            sceneLoader.LoadSceneByName(nextScene);    // SceneLoader handles fade + sound
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  LAST SCENE DETECTION
    // ─────────────────────────────────────────────────────────────────────────

    private bool IsLastScene(string firstChoice, string currentScene)
    {
        if (firstChoice == "Maths"   && currentScene == englishSceneName) return true;
        if (firstChoice == "English" && currentScene == mathsSceneName)   return true;
        return false;
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  GAME ENDING TIMELINE
    // ─────────────────────────────────────────────────────────────────────────

    private void PlayGameEndingTimeline()
    {
        if (gameEndingTimeline != null)
            gameEndingTimeline.Play();
        else
            Debug.LogWarning("GameEnd: gameEndingTimeline is not assigned!");
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  UTILITY
    // ─────────────────────────────────────────────────────────────────────────

    public void ResetProgress()
    {
        PlayerPrefs.SetInt(VISIT_COUNT_KEY, 0);
        PlayerPrefs.DeleteKey(FIRST_CHOICE_KEY);
        PlayerPrefs.Save();
        sequenceStarted = false;
        Debug.Log("GameEnd: Progress reset.");
    }
}