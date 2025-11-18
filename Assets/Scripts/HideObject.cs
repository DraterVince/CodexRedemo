using UnityEngine;

public class HideObject : MonoBehaviour
{
    public Timer timer;
    public GameObject targetObject;
    public float duration = 2f;

    // Call this from your button's OnClick event
    public void PauseAndHide()
    {
        StartCoroutine(PauseAndHideCoroutine());
    }

    private System.Collections.IEnumerator PauseAndHideCoroutine()
    {
        if (timer != null) timer.PauseTimer();
        if (targetObject != null) targetObject.SetActive(false);
        yield return new WaitForSeconds(duration);
        if (targetObject != null) targetObject.SetActive(true);
        if (timer != null) timer.StartTimer();
    }
}