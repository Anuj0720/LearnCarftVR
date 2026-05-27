using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spawns 3D number prefabs above each unsolved word block when the hint fires.
///
/// FIX: wordBlocks list is now populated DYNAMICALLY at hint-time using
/// FindObjectsByType<WordBlockData>() — so blocks that are spawned/activated
/// by Timeline activation tracks are found correctly even though they didn't
/// exist at scene Start.
///
/// You do NOT need to manually fill the Word Blocks list in the Inspector.
/// </summary>
public class HintNumberSpawner : MonoBehaviour
{
    [Header("Option A – One prefab per number  (recommended)")]
    [Tooltip("Size = number of words in your sentence.\n" +
             "Element 0 = '1' prefab, Element 1 = '2' prefab, etc.")]
    public GameObject[] numberPrefabs;

    [Header("Option B – Single prefab with a text component")]
    [Tooltip("Enable this to use ONE prefab for all numbers (needs TextMesh or TMPro on it).")]
    public bool useSinglePrefabWithText = false;
    [Tooltip("Prefab with TextMesh / TextMeshPro. Used only when Option B is enabled.")]
    public GameObject numberTextPrefab;

    [Header("Position")]
    [Tooltip("Offset from the block's world position where the number appears.")]
    public Vector3 spawnOffset = new Vector3(0f, 0.25f, 0f);

    // ── Runtime ───────────────────────────────────────────────────────────────
    // Maps WordBlockData → its spawned number GameObject
    private Dictionary<WordBlockData, GameObject> _spawnedMap
        = new Dictionary<WordBlockData, GameObject>();

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>
    /// Called by HintsManager when the hint delay expires.
    /// Dynamically finds all active WordBlockData objects in the scene.
    /// </summary>
    public void ShowHints()
    {
        HideAllHints(); // destroy any previous hint numbers first

        // Find ALL WordBlockData currently active in the scene
        // (includes blocks activated mid-game by Timeline activation tracks)
#if UNITY_2023_1_OR_NEWER
        WordBlockData[] allBlocks = FindObjectsByType<WordBlockData>(FindObjectsSortMode.None);
#else
        WordBlockData[] allBlocks = FindObjectsOfType<WordBlockData>();
#endif

        if (allBlocks.Length == 0)
        {
            Debug.LogWarning("[HintNumberSpawner] No WordBlockData found in scene. " +
                             "Make sure blocks are active before hint fires.", this);
            return;
        }

        int spawned = 0;
        foreach (WordBlockData block in allBlocks)
        {
            if (block == null || block.isPlacedCorrectly) continue;

            int zeroIndex = block.correctSlotIndex - 1;
            GameObject numObj = SpawnNumber(zeroIndex, block.transform.position + spawnOffset);

            if (numObj != null)
            {
                // Parent to block so it moves with it
                numObj.transform.SetParent(block.transform, worldPositionStays: true);
                block.spawnedHintNumber = numObj;
                _spawnedMap[block] = numObj;
                spawned++;
            }
        }

        Debug.Log($"[HintNumberSpawner] Showing {spawned} hint number(s) over {allBlocks.Length} block(s).");
    }

    /// <summary>Destroys all spawned hint numbers and clears state.</summary>
    public void HideAllHints()
    {
        foreach (var kvp in _spawnedMap)
        {
            if (kvp.Value != null) Destroy(kvp.Value);
            if (kvp.Key  != null) kvp.Key.spawnedHintNumber = null;
        }
        _spawnedMap.Clear();
    }

    // ── Spawn helper ──────────────────────────────────────────────────────────

    GameObject SpawnNumber(int zeroIndex, Vector3 worldPos)
    {
        if (useSinglePrefabWithText)
        {
            // ── Option B ─────────────────────────────────────────────────────
            if (numberTextPrefab == null)
            {
                Debug.LogError("[HintNumberSpawner] Option B: 'numberTextPrefab' is null! " +
                               "Assign a prefab with a TextMesh or TMPro component.", this);
                return null;
            }

            GameObject obj = Instantiate(numberTextPrefab, worldPos, Quaternion.identity);
            obj.name = $"HintNum_{zeroIndex + 1}";

            // ── TextMeshPro (3D) ──
            // Uncomment the next two lines if your prefab uses TMPro:
            // TMPro.TextMeshPro tmp = obj.GetComponentInChildren<TMPro.TextMeshPro>(true);
            // if (tmp != null) { tmp.text = (zeroIndex + 1).ToString(); return obj; }

            // ── Legacy TextMesh ──
            TextMesh tm = obj.GetComponentInChildren<TextMesh>(true);
            if (tm != null)
            {
                tm.text = (zeroIndex + 1).ToString();
                return obj;
            }

            Debug.LogWarning($"[HintNumberSpawner] Prefab '{numberTextPrefab.name}' has no " +
                             "TextMesh or TextMeshPro component.", this);
            return obj; // still return; at least the mesh is visible
        }
        else
        {
            // ── Option A ─────────────────────────────────────────────────────
            if (numberPrefabs == null || numberPrefabs.Length == 0)
            {
                Debug.LogError("[HintNumberSpawner] 'numberPrefabs' array is empty! " +
                               "Add one prefab per word slot (index 0 = number 1, etc.).", this);
                return null;
            }

            if (zeroIndex < 0 || zeroIndex >= numberPrefabs.Length)
            {
                Debug.LogError($"[HintNumberSpawner] No prefab at index {zeroIndex}. " +
                               $"Array has {numberPrefabs.Length} element(s). " +
                               "Make sure numberPrefabs.Length >= totalSlots.", this);
                return null;
            }

            if (numberPrefabs[zeroIndex] == null)
            {
                Debug.LogError($"[HintNumberSpawner] numberPrefabs[{zeroIndex}] is null. " +
                               "Drag the prefab into that array slot.", this);
                return null;
            }

            GameObject obj = Instantiate(numberPrefabs[zeroIndex], worldPos, Quaternion.identity);
            obj.name = $"HintNum_{zeroIndex + 1}";
            return obj;
        }
    }
}