using UnityEngine;
using TMPro;

public class WordCube : MonoBehaviour
{
    public string wordValue = "";
    public TextMeshPro wordLabel;

    void Start()
    {
        if (wordLabel != null)
            wordLabel.text = wordValue;
    }
}