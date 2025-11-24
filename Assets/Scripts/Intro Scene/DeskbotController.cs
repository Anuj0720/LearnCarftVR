using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class DeskbotController : MonoBehaviour
{
    [Header("Audio Clips")]
    public AudioClip introClip;
    public AudioClip hintClip;
    public AudioClip successClip;

    [Header("Animation Clips")]
    public string idleAnimation = "StaticIdle";      
    public string talkingAnimation = "CombatIdle";   
    public string celebrateAnimation = "Jump_Start"; 

    [Header("Settings")]
    public float blendTime = 0.2f;
    public float hintDelay = 5f;

    private AudioSource audioSource;
    private Animator animator;

    private float idleTimer = 0f;
    private bool speaking = false;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
    }

    void Start()
    {
        PlayIntro();
    }

    void Update()
    {
        if (speaking) return;

        idleTimer += Time.deltaTime;
        if (idleTimer >= hintDelay)
        {
            PlayHint();
            idleTimer = 0f;
        }
    }

    public void ResetIdleTimer()
    {
        idleTimer = 0f;
    }

    IEnumerator PlayVoice(AudioClip clip, bool celebrate = false)
    {
        if (clip == null) yield break;

        speaking = true;

        // Play talking anim
        if (animator != null)
            animator.CrossFade(talkingAnimation, blendTime);

        audioSource.PlayOneShot(clip);
        yield return new WaitForSeconds(clip.length);

        // Idle return
        if (animator != null)
            animator.CrossFade(idleAnimation, blendTime);

        if (celebrate && animator != null)
            animator.CrossFade(celebrateAnimation, 0.1f);

        speaking = false;
    }

    //-----------------------------------------------------------

    public void PlayIntro()
    {
        StartCoroutine(PlayVoice(introClip));
    }

    public void PlayHint()
    {
        StartCoroutine(PlayVoice(hintClip));
    }

    public void PlaySuccess()
    {
        StartCoroutine(PlayVoice(successClip, true));
    }
}
