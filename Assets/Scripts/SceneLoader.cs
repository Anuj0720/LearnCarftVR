using UnityEngine;

public class SceneLoader : MonoBehaviour
{
    [Header("Fade Settings")]
    public Color fadeColor = Color.black;
    public float fadeSpeed = 1.5f;

    [Header("Audio Settings")]
    public AudioClip buttonClickSound;
    public AudioClip sceneTransitionSound;

    private AudioSource audioSource;

    void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

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

    void PlaySounds()
    {
        if (buttonClickSound != null)
            audioSource.PlayOneShot(buttonClickSound);

        if (sceneTransitionSound != null)
            audioSource.PlayOneShot(sceneTransitionSound);
    }

    void GoToMaths()
    {
        Initiate.Fade("Maths", fadeColor, fadeSpeed);
    }

    void GoToEnglish()
    {
        Initiate.Fade("Scene_2", fadeColor, fadeSpeed);
    }
}