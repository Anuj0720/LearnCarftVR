using UnityEngine;

public class VFXPlayer : MonoBehaviour
{
    [Header("Drag Prefab from Project")]
    public ParticleSystem vfxPrefab;

    [Header("Effect Scale")]
    public float effectScale = 1f;

    private ParticleSystem spawnedEffect;

    void OnEnable()
    {
        if (vfxPrefab == null)
        {
            Debug.LogWarning("No VFX Prefab assigned on: " + gameObject.name);
            return;
        }

        // Spawn effect at this cube position only (no rotation)
        spawnedEffect = Instantiate(vfxPrefab, transform.position, Quaternion.identity);
        spawnedEffect.transform.localScale = Vector3.one * effectScale;
        spawnedEffect.Play();

        Debug.Log("VFX Playing on: " + gameObject.name);
    }

    void OnDisable()
    {
        if (spawnedEffect == null) return;

        // Destroy effect only when cube is disabled
        spawnedEffect.Stop();
        Destroy(spawnedEffect.gameObject);
        spawnedEffect = null;

        Debug.Log("VFX Destroyed on: " + gameObject.name);
    }
}