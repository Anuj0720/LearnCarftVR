using UnityEngine;
using TMPro;

public class WordCube : MonoBehaviour
{
    // --------------------------------------------------------
    // SECTION 1 — PUBLIC VARIABLES
    // These show up in the Inspector
    // --------------------------------------------------------

    [Header("── WORD ──────────────────────────────────")]
    public string wordValue = "";
    // Set automatically by Room4Manager — do not fill manually

    [Header("── STATE ────────────────────────────────")]
    public bool isSnapped   = false;
    public int  snappedSlot = -1;
    // -1 means not in any slot

    [Header("── MANAGER REFERENCE ────────────────────")]
    public Room4Manager manager;
    // Set automatically by Room4Manager — do not fill manually

    [Header("── SNAP DISTANCE ──────────────────────")]
    public float snapDistance = 0.5f;
    // Increase this if students struggle to snap
    // Decrease if cubes snap too easily

    [Header("── WORD LABEL ──────────────────────────")]
    public TextMeshPro wordLabel;
    // Drag the TextMeshPro from your cube face here


    // --------------------------------------------------------
    // SECTION 2 — START
    // --------------------------------------------------------

    void Start()
    {
        // Display the word on the cube face when spawned
        if (wordLabel != null)
            wordLabel.text = wordValue;
    }


    // --------------------------------------------------------
    // SECTION 3 — GRAB EVENTS
    // Hook these into XRGrabInteractable events in Inspector
    //
    // Select Entered → WordCube.OnGrabbed
    // Select Exited  → WordCube.OnReleased
    // --------------------------------------------------------

    // Called when student PICKS UP the cube
    public void OnGrabbed()
    {
        if (!isSnapped) return;

        // Remove cube from its slot so slot becomes empty again
        manager.slotContents[snappedSlot] = null;
        isSnapped   = false;
        snappedSlot = -1;
    }

    // Called when student RELEASES the cube
    public void OnReleased()
    {
        if (isSnapped) return;

        // Check all 4 slots — is cube close enough to snap?
        for (int i = 0; i < manager.slots.Length; i++)
        {
            float dist = Vector3.Distance(
                transform.position,
                manager.slots[i].position
            );

            if (dist < snapDistance)
            {
                // Close enough — snap into this slot
                manager.TrySnapCubeToSlot(gameObject, i);
                return;
            }
        }

        // Not close to any slot — cube stays where dropped
        // Student can pick it up and try again
    }


    // --------------------------------------------------------
    // SECTION 4 — VISUAL FEEDBACK
    // Optional hover glow when student is near a slot
    // --------------------------------------------------------

    void Update()
    {
        // Only run this check if cube is being held (not snapped)
        if (isSnapped) return;

        bool nearSlot = false;

        for (int i = 0; i < manager.slots.Length; i++)
        {
            float dist = Vector3.Distance(
                transform.position,
                manager.slots[i].position
            );

            if (dist < snapDistance * 1.5f)
            {
                nearSlot = true;
                break;
            }
        }

        // Glow yellow when near a slot, white when not
        Renderer r = GetComponent<Renderer>();
        if (r != null)
        {
            r.material.color = nearSlot
                ? new Color(1f, 1f, 0.3f)   // yellow glow
                : Color.white;               // normal white
        }
    }
}