using UnityEngine;
using UnityEngine.SceneManagement;

public class Scene1Controller : MonoBehaviour
{
    public Slot[] slots;
    public DeskbotController deskbot;
    public GameObject roomLights;

    private int placedCount = 0;

    public void OnShapeSnapped()
    {
        placedCount++;
        deskbot.ResetIdleTimer();

        if (placedCount == slots.Length)
            CompleteScene();
    }

    void CompleteScene()
    {
        if (roomLights != null)
            roomLights.SetActive(true);

        deskbot.PlaySuccess();

        Invoke(nameof(NextScene), 2.5f);
    }

    void NextScene()
    {
        SceneManager.LoadScene("Scene2_PhonicsClassroom");
    }
}
