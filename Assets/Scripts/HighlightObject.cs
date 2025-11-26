using UnityEngine;

public class HighlightObject : MonoBehaviour
{
    public Material normalMat;
    public Material highlightMat;
    private Renderer rend;

    void Start()
    {
        rend = GetComponent<Renderer>();
        rend.material = normalMat;
    }

    public void Highlight(bool state)
    {
        rend.material = state ? highlightMat : normalMat;
    }
}
