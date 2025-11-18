using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Part1Level1 : MonoBehaviour
{
    public void LoadPart1Level1()
    {
        SceneManager.LoadSceneAsync("Part1Level1");
    }
}