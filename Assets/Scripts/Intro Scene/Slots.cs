using UnityEngine;

public class Slot : MonoBehaviour
{
    public string acceptedTag;
    public bool occupied = false;

    public void Snap(Transform shape)
    {
        shape.position = transform.position;
        shape.rotation = transform.rotation;
        shape.SetParent(transform);
        occupied = true;
    }
}
