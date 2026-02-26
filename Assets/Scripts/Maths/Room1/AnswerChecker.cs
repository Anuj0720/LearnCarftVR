using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class AnswerChecker : MonoBehaviour
{
    public int correctAnswer = 4;  // for 2 + 2
    private XRSocketInteractor socket;

    void Start()
    {
        socket = GetComponent<XRSocketInteractor>();
        socket.selectEntered.AddListener(OnObjectPlaced);
    }

    void OnObjectPlaced(SelectEnterEventArgs args)
    {
        BlockData block = args.interactableObject.transform.GetComponent<BlockData>();

        if (block != null)
        {
            if (block.value == correctAnswer)
            {
                Debug.Log("Correct Answer!");
            }
            else
            {
                Debug.Log("Wrong Answer");
            }
        }
    }
}