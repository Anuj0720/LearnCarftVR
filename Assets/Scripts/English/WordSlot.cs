using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Attach to each answer-slot socket (XRSocketInteractor).
///
/// FIX: Listener is added in Awake() — not Start() — so it catches blocks
/// that are activated by Timeline activation tracks (which can fire before Start).
/// Also uses a frame-delay safety net to guarantee the socket reference exists.
/// </summary>
[RequireComponent(typeof(XRSocketInteractor))]
public class WordSlot : MonoBehaviour
{
    [Tooltip("1-based position this slot represents (1 = first word).")]
    public int slotIndex = 1;

    [Header("References")]
    public WordSequenceChecker sequenceChecker;

    [Header("Wrong Answer – Audio")]
    [Tooltip("AudioSource with the buzzer clip already set on it.")]
    public AudioSource buzzerAudioSource;
    [Tooltip("Alternatively: just the clip. Needs Fallback Audio Source below.")]
    public AudioClip   buzzerClip;
    [Tooltip("Shared AudioSource used when only a clip is supplied.")]
    public AudioSource fallbackAudioSource;

    [Header("Visual Feedback")]
    public Renderer slotRenderer;
    public Color wrongFlashColor   = Color.red;
    public Color correctFlashColor = Color.green;
    public float flashDuration     = 0.4f;

    // ── Private ──────────────────────────────────────────────────────────────
    private XRSocketInteractor _socket;
    private bool _filledCorrectly = false;
    private bool _isEjecting      = false;
    private Color _originalColor  = Color.white;

    // ── Awake: subscribe as early as possible ─────────────────────────────────
    void Awake()
    {
        _socket = GetComponent<XRSocketInteractor>();

        // Subscribe immediately so we never miss a placement event
        _socket.selectEntered.AddListener(OnBlockPlaced);

        if (slotRenderer != null)
            _originalColor = slotRenderer.material.color;
    }

    void Start()
    {
        // Validation only — no subscription here to avoid double-subscribe
        if (sequenceChecker == null)
            Debug.LogError($"[WordSlot {slotIndex}] 'Sequence Checker' is NOT assigned in the Inspector!", this);

        if (buzzerAudioSource == null && (buzzerClip == null || fallbackAudioSource == null))
            Debug.LogWarning($"[WordSlot {slotIndex}] No buzzer audio configured. " +
                             "Assign 'Buzzer Audio Source' OR both 'Buzzer Clip' + 'Fallback Audio Source'.", this);
    }

    void OnDestroy()
    {
        if (_socket != null)
            _socket.selectEntered.RemoveListener(OnBlockPlaced);
    }

    // ── Core placement handler ────────────────────────────────────────────────

    void OnBlockPlaced(SelectEnterEventArgs args)
    {
        if (_filledCorrectly || _isEjecting) return;

        // Try to get WordBlockData — search root and children
        WordBlockData block = args.interactableObject.transform.GetComponent<WordBlockData>();
        if (block == null)
            block = args.interactableObject.transform.GetComponentInChildren<WordBlockData>();

        if (block == null)
        {
            Debug.LogWarning($"[WordSlot {slotIndex}] Placed object '{args.interactableObject.transform.name}' " +
                             "has no WordBlockData. Ignoring.", this);
            return;
        }

        Debug.Log($"[WordSlot {slotIndex}] Block placed → word:'{block.wordText}'  " +
                  $"correctSlot:{block.correctSlotIndex}  thisSlot:{slotIndex}");

        if (block.correctSlotIndex == slotIndex)
            HandleCorrect(block);
        else
            HandleWrong(args.interactableObject, block);
    }

    // ── Correct placement ─────────────────────────────────────────────────────

    void HandleCorrect(WordBlockData block)
    {
        _filledCorrectly        = true;
        block.isPlacedCorrectly = true;

        if (block.spawnedHintNumber != null)
        {
            block.spawnedHintNumber.SetActive(false);
            block.spawnedHintNumber = null;
        }

        if (slotRenderer != null)
            StartCoroutine(FlashColor(correctFlashColor));

        Debug.Log($"[WordSlot {slotIndex}] ✅ CORRECT");

        if (sequenceChecker != null)
            sequenceChecker.OnSlotFilledCorrectly(slotIndex);
        else
            Debug.LogError($"[WordSlot {slotIndex}] sequenceChecker is null — victory will NOT fire!", this);
    }

    // ── Wrong placement ───────────────────────────────────────────────────────

    void HandleWrong(IXRSelectInteractable interactable, WordBlockData block)
    {
        Debug.Log($"[WordSlot {slotIndex}] ❌ WRONG — '{block.wordText}' belongs in slot {block.correctSlotIndex}");

        _isEjecting = true;

        // Play buzzer IMMEDIATELY before any coroutine yield
        PlayBuzzer();

        if (slotRenderer != null)
            StartCoroutine(FlashColor(wrongFlashColor));

        StartCoroutine(EjectBlock(interactable));

        if (sequenceChecker != null)
            sequenceChecker.OnWrongPlacement();
    }

    // ── Eject ─────────────────────────────────────────────────────────────────

    IEnumerator EjectBlock(IXRSelectInteractable interactable)
    {
        yield return null; // one frame for XR Toolkit to finish selecting

        if (interactable == null) { _isEjecting = false; yield break; }

        if (_socket.hasSelection)
            _socket.interactionManager.SelectExit(_socket, interactable);

        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();

        XRGrabInteractable grab = interactable.transform.GetComponent<XRGrabInteractable>();
        if (grab != null) grab.enabled = true;

        Rigidbody rb = interactable.transform.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            Vector3 dir = (interactable.transform.position - transform.position).normalized;
            if (dir == Vector3.zero) dir = Vector3.up;
            rb.AddForce(dir * 3f, ForceMode.Impulse);
        }

        yield return new WaitForSeconds(0.5f);
        _isEjecting = false;
    }

    // ── Flash ─────────────────────────────────────────────────────────────────

    IEnumerator FlashColor(Color flash)
    {
        if (slotRenderer == null) yield break;
        slotRenderer.material.color = flash;
        yield return new WaitForSeconds(flashDuration);
        slotRenderer.material.color = _originalColor;
    }

    // ── Buzzer ────────────────────────────────────────────────────────────────

    void PlayBuzzer()
    {
        if (buzzerAudioSource != null && buzzerAudioSource.clip != null)
        {
            buzzerAudioSource.Stop();
            buzzerAudioSource.Play();
            Debug.Log($"[WordSlot {slotIndex}] 🔊 Buzzer played via AudioSource.");
            return;
        }

        if (buzzerClip != null && fallbackAudioSource != null)
        {
            fallbackAudioSource.PlayOneShot(buzzerClip);
            Debug.Log($"[WordSlot {slotIndex}] 🔊 Buzzer played via PlayOneShot.");
            return;
        }

        Debug.LogError($"[WordSlot {slotIndex}] ❌ Buzzer audio NOT configured. " +
                       "Assign 'Buzzer Audio Source' (with clip set on it) OR " +
                       "both 'Buzzer Clip' + 'Fallback Audio Source'.", this);
    }

    // ── Public ────────────────────────────────────────────────────────────────

    public void ResetSlot()
    {
        _filledCorrectly = false;
        _isEjecting      = false;
        if (slotRenderer != null)
            slotRenderer.material.color = _originalColor;
    }
}