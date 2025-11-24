using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class ShapeCollectible : MonoBehaviour
{
    public string shapeTag = "Shape_Cube";
    public AudioClip collectClip;

    private AudioSource audioSource;
    private ShapeHoverGlow glow;
    private XRGrabInteractable grab;

    void Awake()
    {
        glow = GetComponent<ShapeHoverGlow>();
        audioSource = gameObject.AddComponent<AudioSource>();
        grab = GetComponent<XRGrabInteractable>();

        grab.selectEntered.AddListener(OnGrab);
        grab.selectExited.AddListener(OnRelease);
    }

    void OnGrab(SelectEnterEventArgs args)
    {
        glow?.SetHover(true);
    }

    void OnRelease(SelectExitEventArgs args)
    {
        glow?.SetHover(false);

        TryPlaceInSlot();
    }

    void TryPlaceInSlot()
    {
        var scene = FindObjectOfType<Scene1Controller>();
        if (scene == null) return;

        foreach (var s in scene.slots)
        {
            float distance = Vector3.Distance(transform.position, s.transform.position);

            if (!s.occupied && s.acceptedTag == shapeTag && distance < 0.2f)
            {
                // Play sound
                if (collectClip != null)
                    audioSource.PlayOneShot(collectClip);

                // Snap to slot
                s.Snap(transform);

                // Remove grab behavior after placed
                grab.enabled = false;
                GetComponent<Rigidbody>().isKinematic = true;

                // Notify Scene Controller
                scene.OnShapeSnapped();
                return;
            }
        }
    }
}
