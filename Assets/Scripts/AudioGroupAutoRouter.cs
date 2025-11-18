using UnityEngine;
using UnityEngine.Audio;

// Attach this to any GameObject with an AudioSource to auto-route it to a mixer group.
// Configure mixer + groupName in the inspector, or set a tag ("Music" or "SFX") and enable useTag.
[DisallowMultipleComponent]
public class AudioGroupAutoRouter : MonoBehaviour
{
    public AudioMixer mixer;
    [Tooltip("Exact group name inside the mixer (e.g., 'Master/Music' or 'Master/SFX'). If set, this takes precedence over tags.")]
    public string groupName;
    [Tooltip("If true, will use GameObject tag ('Music' or 'SFX') to pick a group when groupName is empty.")]
    public bool useTagIfNoGroupName = true;

    [Tooltip("Fallback group path if tag not recognized.")]
    public string fallbackGroupName = "Master";

    private void Awake()
    {
        TryRoute();
    }

    public void TryRoute()
    {
        var src = GetComponent<AudioSource>();
        if (!src)
            return;
        if (!mixer)
        {
            Debug.LogWarning($"[AudioGroupAutoRouter] No mixer assigned on {name}");
            return;
        }

        string desired = groupName;
        if (string.IsNullOrEmpty(desired) && useTagIfNoGroupName)
        {
            if (CompareTag("Music")) desired = "Master/Music";
            else if (CompareTag("SFX")) desired = "Master/SFX";
        }
        if (string.IsNullOrEmpty(desired)) desired = fallbackGroupName;

        var groups = mixer.FindMatchingGroups(desired);
        if (groups != null && groups.Length > 0)
        {
            src.outputAudioMixerGroup = groups[0];
            // Debug.Log($"[AudioGroupAutoRouter] Routed {name} to group '{desired}'");
        }
        else
        {
            Debug.LogWarning($"[AudioGroupAutoRouter] Group '{desired}' not found in mixer for {name}");
        }
    }
}
