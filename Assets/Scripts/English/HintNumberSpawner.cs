using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class HintNumberSpawner : MonoBehaviour
{
    [Header("Option A – One prefab per number  (recommended)")]
    [Tooltip("Size = number of words in your sentence.\n" +
             "Element 0 = '1' prefab, Element 1 = '2' prefab, etc.")]
    public GameObject[] numberPrefabs;

    [Header("Option B – Single prefab with a text component")]
    public bool useSinglePrefabWithText = false;
    public GameObject numberTextPrefab;

    [Header("Position")]
    public Vector3 spawnOffset = new Vector3(0f, 0.25f, 0f);

    [Header("Rotation (Live — changes apply instantly in Play Mode)")]
    public Vector3 spawnRotation = new Vector3(0f, 0f, 0f);

    private Dictionary<WordBlockData, GameObject> _spawnedMap
        = new Dictionary<WordBlockData, GameObject>();

    // ── Live rotation update ──────────────────────────────────────────────────
    void Update()
    {
        if (_spawnedMap.Count == 0) return;

        Quaternion rot = Quaternion.Euler(spawnRotation);
        foreach (var kvp in _spawnedMap)
        {
            if (kvp.Value != null)
                kvp.Value.transform.rotation = rot;
        }
    }

    // ── Public API ────────────────────────────────────────────────────────────
    public void ShowHints()
    {
        HideAllHints();

#if UNITY_2023_1_OR_NEWER
        WordBlockData[] allBlocks = FindObjectsByType<WordBlockData>(FindObjectsSortMode.None);
#else
        WordBlockData[] allBlocks = FindObjectsOfType<WordBlockData>();
#endif

        if (allBlocks.Length == 0)
        {
            Debug.LogWarning("[HintNumberSpawner] No WordBlockData found in scene.", this);
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
                numObj.transform.SetParent(block.transform, worldPositionStays: true);
                block.spawnedHintNumber = numObj;
                _spawnedMap[block] = numObj;
                spawned++;
            }
        }

        Debug.Log($"[HintNumberSpawner] Showing {spawned} hint number(s) over {allBlocks.Length} block(s).");
    }

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
        Quaternion rotation = Quaternion.Euler(spawnRotation);

        if (useSinglePrefabWithText)
        {
            if (numberTextPrefab == null)
            {
                Debug.LogError("[HintNumberSpawner] Option B: 'numberTextPrefab' is null!", this);
                return null;
            }

            GameObject obj = Instantiate(numberTextPrefab, worldPos, rotation);
            obj.name = $"HintNum_{zeroIndex + 1}";

            TextMesh tm = obj.GetComponentInChildren<TextMesh>(true);
            if (tm != null) tm.text = (zeroIndex + 1).ToString();

            return obj;
        }
        else
        {
            if (numberPrefabs == null || numberPrefabs.Length == 0)
            {
                Debug.LogError("[HintNumberSpawner] 'numberPrefabs' array is empty!", this);
                return null;
            }

            if (zeroIndex < 0 || zeroIndex >= numberPrefabs.Length)
            {
                Debug.LogError($"[HintNumberSpawner] No prefab at index {zeroIndex}.", this);
                return null;
            }

            if (numberPrefabs[zeroIndex] == null)
            {
                Debug.LogError($"[HintNumberSpawner] numberPrefabs[{zeroIndex}] is null.", this);
                return null;
            }

            GameObject obj = Instantiate(numberPrefabs[zeroIndex], worldPos, rotation);
            obj.name = $"HintNum_{zeroIndex + 1}";
            return obj;
        }
    }
}