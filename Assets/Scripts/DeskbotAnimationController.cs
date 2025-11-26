using UnityEngine;

public class DeskbotAnimationController : MonoBehaviour
{
    public Animator animator;

    [Header("Trigger Names")]
    public string introTrigger = "Intro";
    public string hintTrigger = "Hint";
    public string successTrigger = "Success";
    public string idleTrigger = "Idle";

    private string lastTrigger = null;

    private void FireTrigger(string trigger)
    {
        if (!animator) return;

        // Reset any previous trigger
        if (!string.IsNullOrEmpty(lastTrigger))
            animator.ResetTrigger(lastTrigger);

        // Fire new trigger
        animator.SetTrigger(trigger);
        lastTrigger = trigger;
    }

    public void PlayIntro()  => FireTrigger(introTrigger);
    public void PlayHint()   => FireTrigger(hintTrigger);
    public void PlaySuccess() => FireTrigger(successTrigger);
    public void PlayIdle()    => FireTrigger(idleTrigger);
}
