using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseTimer : MonoBehaviour
{
    public void Pause()
    {
        Time.timeScale = 0;

        // Ensure battle music remains active when pausing in a gameplay scene
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.PlayBattleMusic();
        }
    }
}
