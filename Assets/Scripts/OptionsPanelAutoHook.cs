using UnityEngine;

/// <summary>
/// Attach this to your Options/Settings panel GameObject.
/// When the panel becomes enabled/visible, it asks SettingsManager to refresh slider bindings.
/// This ensures sliders created or shown at runtime get wired up.
/// </summary>
using System.Collections;

public class OptionsPanelAutoHook : MonoBehaviour
{
    private void OnEnable()
    {
        TryRefresh();
        // In case SettingsManager appears a frame later (scene timing), try for a few frames
        StartCoroutine(TryRefreshForFrames(10));
    }

    private void TryRefresh()
    {
        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.RefreshSliders();
        }
        else
        {
            Debug.LogWarning("[OptionsPanelAutoHook] SettingsManager.Instance is null. Will retry shortly. Ensure a persistent SettingsManager exists.");
        }
    }

    private IEnumerator TryRefreshForFrames(int frames)
    {
        for (int i = 0; i < frames; i++)
        {
            if (SettingsManager.Instance != null)
            {
                SettingsManager.Instance.RefreshSliders();
                // Do not apply volume here to avoid unintended changes on open
                yield break;
            }
            yield return null;
        }
        Debug.LogWarning("[OptionsPanelAutoHook] Unable to find SettingsManager after retries. Sliders may not be wired.");
    }
}
