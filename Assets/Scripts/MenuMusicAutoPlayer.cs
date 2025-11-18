using UnityEngine;

/// <summary>
/// Drop this on any scene GameObject to ensure the menu music is playing without restarting.
/// Useful for menu-like scenes (e.g., Singleplayer Menu, Level Select, Main Menu) when you
/// want continuous background music between them.
/// </summary>
public class MenuMusicAutoPlayer : MonoBehaviour
{
    [Header("Behavior")]
    [Tooltip("Play menu music if not already playing when this scene starts")] 
    public bool playOnStart = true;

    private void Start()
    {
        if (!playOnStart) return;
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.PlayMenuMusicIfNotPlaying();
        }
        else
        {
            Debug.LogWarning("[MenuMusicAutoPlayer] MusicManager.Instance is null. Ensure a persistent MusicManager exists in a prior scene or add one to this scene.");
        }
    }
}
