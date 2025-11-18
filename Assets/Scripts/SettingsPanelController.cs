using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Simple controller to open/close a settings panel from UI Buttons.
/// Assign this component to any GameObject and hook your UI buttons' OnClick events
/// to call OpenPanel() / ClosePanel().
/// </summary>
public class SettingsPanelController : MonoBehaviour
{
    [Header("Panel to toggle")]
    public GameObject panel;

    [Header("Fallback find settings")]
    public bool findByNameFallback = true;
    public string panelName = "SettingsPanel";

    public void OpenPanel()
    {
        if (panel == null && findByNameFallback)
        {
            panel = GameObject.Find(panelName);
            Debug.Log("[SettingsPanelController] Panel was null, searched by name '" + panelName + "' => " + (panel ? "FOUND" : "NOT FOUND"));
        }

        if (panel == null)
        {
            Debug.LogWarning("[SettingsPanelController] Panel not assigned and not found.");
            return;
        }

        Debug.Log("[SettingsPanelController] Opening panel '" + panel.name + "'");
        panel.SetActive(true);
    }

    public void ClosePanel()
    {
        if (panel == null && findByNameFallback)
        {
            panel = GameObject.Find(panelName);
        }

        if (panel == null)
        {
            Debug.LogWarning("[SettingsPanelController] Panel not assigned and not found.");
            return;
        }

        Debug.Log("[SettingsPanelController] Closing panel '" + panel.name + "'");
        panel.SetActive(false);
    }
}
