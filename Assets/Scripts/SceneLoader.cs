using UnityEngine;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }

    [Header("Fade Settings")]
    public Color fadeColor = Color.black;
    public float fadeSpeed = 1.5f;

    [Header("Audio Settings")]
    public AudioClip buttonClickSound;
    public AudioClip sceneTransitionSound;

    private AudioSource audioSource;
    private string targetSceneName;  // used by LoadSceneByName

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    // ── Called from Menu buttons ──────────────────────────────────────────────
    public void LoadMathsScene()
    {
        PlaySounds();
        Invoke("GoToMaths", 0.5f);
    }

    public void LoadEnglishScene()
    {
        PlaySounds();
        Invoke("GoToEnglish", 0.5f);
    }

    // ── Called from GameEnd.cs dynamically ───────────────────────────────────
    public void LoadSceneByName(string sceneName)
    {
        targetSceneName = sceneName;
        PlaySounds();
        Invoke("GoToTargetScene", 0.5f);
    }

    // ── Sound helper ─────────────────────────────────────────────────────────
    void PlaySounds()
    {
        if (buttonClickSound != null)
            audioSource.PlayOneShot(buttonClickSound);

        if (sceneTransitionSound != null)
            audioSource.PlayOneShot(sceneTransitionSound);
    }

    // ── Scene transition methods ──────────────────────────────────────────────
    void GoToMaths()
    {
        Initiate.Fade("Maths", fadeColor, fadeSpeed);
    }

    void GoToEnglish()
    {
        Initiate.Fade("Scene_2", fadeColor, fadeSpeed);
    }

    void GoToTargetScene()
    {
        Initiate.Fade(targetSceneName, fadeColor, fadeSpeed);
    }
}