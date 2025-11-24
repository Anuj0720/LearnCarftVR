using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class ShapeHoverGlow : MonoBehaviour
{
    public Color glowColor = Color.cyan;
    public float intensity = 2f;

    private Material mat;
    private Color originalColor;

    void Start()
    {
        mat = GetComponent<Renderer>().material;
        originalColor = mat.GetColor("_EmissionColor");
    }

    public void SetHover(bool active)
    {
        if (active)
            mat.SetColor("_EmissionColor", glowColor * intensity);
        else
            mat.SetColor("_EmissionColor", originalColor);
    }
}
