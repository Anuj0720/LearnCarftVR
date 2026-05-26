using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameEnd : MonoBehaviour
{
    [Header("Scene Settings")]
    public string menuSceneName   = "Menu";
    public string mathsSceneName  = "Maths";
    public string englishSceneName = "Scene_2";

    [Header("Manual Scene Override")]
    [Tooltip("If you want to tell the script which scene you are in, fill this. Otherwise the active scene name is used.")]
    public string currentSceneOverride;

    [Header("Timeline References")]
    public PlayableDirector celebrationTimeline;
    public PlayableDirector gameEndTimeline;

    [Header("Transition Settings")]
    [Tooltip("Seconds to wait after celebration ends before the next scene starts.")]
    public float celebrationTransitionDelay = 1.0f;

    [Header("Count Tracking")]
    [Tooltip("The game flow count: 0 = menu, 2 = first module entered, 3 = second module entered.")]
    public int count;

    private SceneLoader sceneLoader;
    private bool        celebrationHasPlayed = false;
    private bool        watchingCelebration  = false;

    private const string COUNT_KEY = "GameCount";

    void Awake()
    {
        sceneLoader = SceneLoader.Instance;
        if (sceneLoader == null)
            sceneLoader = FindObjectOfType<SceneLoader>();

        if (sceneLoader == null)
            Debug.LogWarning("[GameEnd] No SceneLoader found in scene.");
    }

    void Start()
    {
        count = PlayerPrefs.GetInt(COUNT_KEY, 0);
        string activeScene = GetCurrentSceneName();

        if (activeScene == menuSceneName)
        {
            SetCount(0);
            Debug.Log("[GameEnd] Menu scene detected. Count set to 0.");
        }
        else if (count < 2 && (activeScene == mathsSceneName || activeScene == englishSceneName))
        {
            SetCount(2);
            Debug.Log($"[GameEnd] Module scene detected with missing saved count. Count forced to 2. Scene={activeScene}");
        }

        if (celebrationTimeline == null)
        {
            Debug.LogWarning("[GameEnd] celebrationTimeline is NOT assigned.");
            return;
        }

        if (gameEndTimeline != null)
            gameEndTimeline.Stop();

        celebrationHasPlayed = false;
        watchingCelebration  = true;

        Debug.Log($"[GameEnd] Ready. Current scene: {activeScene}, Count={count}");
    }

    void Update()
    {
        if (!watchingCelebration || celebrationTimeline == null) return;

        if (!celebrationHasPlayed)
        {
            if (celebrationTimeline.state == PlayState.Playing)
            {
                celebrationHasPlayed = true;
                Debug.Log("[GameEnd] Celebration timeline started.");
            }
            return;
        }

        if (celebrationHasPlayed && celebrationTimeline.state != PlayState.Playing)
        {
            watchingCelebration = false;
            Debug.Log("[GameEnd] Celebration timeline ended.");
            OnCelebrationFinished();
        }
    }

    public void OnMenuChooseMaths()
    {
        Debug.Log("[GameEnd] Menu choice: Maths");
        SetCount(2);
        TransitionToScene(mathsSceneName);
    }

    public void OnMenuChooseEnglish()
    {
        Debug.Log("[GameEnd] Menu choice: English");
        SetCount(2);
        TransitionToScene(englishSceneName);
    }

    private void OnCelebrationFinished()
    {
        string currentScene = GetCurrentSceneName();
        count = PlayerPrefs.GetInt(COUNT_KEY, 0);

        Debug.Log($"[GameEnd] Celebration finished in scene: {currentScene}. Count={count}");

        if (count == 3)
        {
            Debug.Log("[GameEnd] Count is 3. Final celebration complete. Playing game end timeline.");
            StartCoroutine(PlayGameEndTimeline());
            return;
        }

        if (count == 2)
        {
            string nextScene = GetNextScene(currentScene);
            if (string.IsNullOrEmpty(nextScene))
            {
                Debug.LogWarning("[GameEnd] Could not determine next scene after celebration.");
                return;
            }

            SetCount(3);
            Debug.Log($"[GameEnd] First module complete. Count set to 3. Waiting {celebrationTransitionDelay} seconds before loading: {nextScene}");
            StartCoroutine(TransitionToSceneAfterDelay(nextScene));
            return;
        }

        Debug.LogWarning($"[GameEnd] Unexpected count value ({count}) after celebration. Resetting to menu.");
        ResetProgress();
        TransitionToScene(menuSceneName);
    }

    private IEnumerator PlayGameEndTimeline()
    {
        if (gameEndTimeline == null)
        {
            Debug.LogWarning("[GameEnd] gameEndTimeline is not assigned. Returning to menu.");
            ResetProgress();
            TransitionToScene(menuSceneName);
            yield break;
        }

        gameEndTimeline.Play();
        Debug.Log("[GameEnd] Game end timeline started.");

        yield return new WaitUntil(() => gameEndTimeline.state != PlayState.Playing);

        Debug.Log("[GameEnd] Game end timeline finished. Returning to menu.");
        ResetProgress();
        TransitionToScene(menuSceneName);
    }

    private string GetCurrentSceneName()
    {
        if (!string.IsNullOrEmpty(currentSceneOverride))
            return currentSceneOverride;

        return SceneManager.GetActiveScene().name;
    }

    private string GetNextScene(string currentScene)
    {
        if (currentScene == mathsSceneName)
            return englishSceneName;

        if (currentScene == englishSceneName)
            return mathsSceneName;

        return string.Empty;
    }

    private void TransitionToScene(string sceneName)
    {
        if (sceneLoader == null)
        {
            sceneLoader = SceneLoader.Instance;
            if (sceneLoader == null)
                sceneLoader = FindObjectOfType<SceneLoader>();
        }

        if (sceneLoader == null)
        {
            Debug.LogWarning($"[GameEnd] SceneLoader is missing in current scene. Falling back to direct load: {sceneName}");
            SceneManager.LoadScene(sceneName);
            return;
        }

        Debug.Log($"[GameEnd] Transitioning to scene: {sceneName}");
        sceneLoader.LoadSceneByName(sceneName);
    }

    private IEnumerator TransitionToSceneAfterDelay(string sceneName)
    {
        yield return new WaitForSeconds(celebrationTransitionDelay);
        TransitionToScene(sceneName);
    }

    private void SetCount(int value)
    {
        count = value;
        PlayerPrefs.SetInt(COUNT_KEY, value);
        PlayerPrefs.Save();
    }

    public void ResetProgress()
    {
        SetCount(0);
        celebrationHasPlayed = false;
        watchingCelebration  = celebrationTimeline != null;
        Debug.Log("[GameEnd] Progress reset.");
    }
}
