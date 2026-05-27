using UnityEngine;

/// <summary>
/// Attach this to every word block prefab.
/// 'correctSlotIndex' is the 1-based sequence position (1 = first word, 2 = second word, etc.)
/// 'wordText' is the word printed on this block (for debug / UI display).
/// </summary>
public class WordBlockData : MonoBehaviour
{
    [Tooltip("The word shown on this block (e.g. \"The\", \"cat\", \"sat\")")]
    public string wordText = "";

    [Tooltip("Which slot this block belongs to (1-based). Must match a WordSlot's slotIndex.")]
    public int correctSlotIndex = 1;

    // ── Runtime state ────────────────────────────────────────────────────────
    [HideInInspector] public bool isPlacedCorrectly = false;

    // Reference to the hint number object spawned above this block (set by HintNumberSpawner)
    [HideInInspector] public GameObject spawnedHintNumber = null;
}
