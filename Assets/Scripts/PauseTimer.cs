using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseTimer : MonoBehaviour
{
    public void Pause()
    {
        Time.timeScale = 0;
    }
}
