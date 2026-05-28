using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class BlockVFX : MonoBehaviour
{
    [Header("Is This The Correct Answer Block")]
    public bool isCorrectBlock = false;

    [Header("Correct Answer VFX")]
    public ParticleSystem correctVFXPrefab;

    [Header("Wrong Answer VFX")]
    public ParticleSystem wrongVFXPrefab;

    [Header("Effect Scale")]
    public float effectScale = 1f;

    private ParticleSystem spawnedCorrectEffect;
    private ParticleSystem spawnedWrongEffect;

    private XRGrabInteractable grabInteractable;

    void Start()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();

        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.AddListener(OnSelected);
            grabInteractable.selectExited.AddListener(OnDeselected);
        }
    }

    void OnSelected(SelectEnterEventArgs args)
    {
        // Placed on socket → play VFX
        if (args.interactorObject is XRSocketInteractor)
        {
            Invoke(nameof(CheckAndPlayEffect), 0.3f);
        }
        // Grabbed by hand → stop VFX
        else
        {
            CancelInvoke(nameof(CheckAndPlayEffect));
            StopAllVFX();
        }
    }

    void OnDeselected(SelectExitEventArgs args)
    {
        // Removed from socket → stop VFX
        if (args.interactorObject is XRSocketInteractor)
        {
            CancelInvoke(nameof(CheckAndPlayEffect));
            StopAllVFX();
        }
    }

    void CheckAndPlayEffect()
    {
        if (isCorrectBlock)
            PlayCorrectEffect();
        else
            PlayWrongEffect();
    }

    void PlayCorrectEffect()
    {
        if (correctVFXPrefab == null)
        {
            Debug.LogWarning("No Correct VFX Prefab assigned on: " + gameObject.name);
            return;
        }

        // Parent to block so it moves with it
        spawnedCorrectEffect = Instantiate(correctVFXPrefab, transform.position, Quaternion.identity);
        spawnedCorrectEffect.transform.SetParent(transform);
        spawnedCorrectEffect.transform.localScale = Vector3.one * effectScale;
        spawnedCorrectEffect.Play();

        Debug.Log("Correct VFX Playing on: " + gameObject.name);
    }

    void PlayWrongEffect()
    {
        if (wrongVFXPrefab == null)
        {
            Debug.LogWarning("No Wrong VFX Prefab assigned on: " + gameObject.name);
            return;
        }

        // Parent to block so it moves with it
        spawnedWrongEffect = Instantiate(wrongVFXPrefab, transform.position, Quaternion.identity);
        spawnedWrongEffect.transform.SetParent(transform);
        spawnedWrongEffect.transform.localScale = Vector3.one * effectScale;
        spawnedWrongEffect.Play();

        Debug.Log("Wrong VFX Playing on: " + gameObject.name);
    }

    void StopAllVFX()
    {
        if (spawnedCorrectEffect != null)
        {
            spawnedCorrectEffect.Stop();
            Destroy(spawnedCorrectEffect.gameObject);
            spawnedCorrectEffect = null;
        }

        if (spawnedWrongEffect != null)
        {
            spawnedWrongEffect.Stop();
            Destroy(spawnedWrongEffect.gameObject);
            spawnedWrongEffect = null;
        }
    }

    void OnDisable()
    {
        CancelInvoke(nameof(CheckAndPlayEffect));
        StopAllVFX();
    }

    void OnDestroy()
    {
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.RemoveListener(OnSelected);
            grabInteractable.selectExited.RemoveListener(OnDeselected);
        }
    }
}