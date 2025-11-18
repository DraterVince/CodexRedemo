using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefaultScale : MonoBehaviour
{
    public float defaultScale = 0.9f;
    private Vector3 defaultScaleVec;
    private void Update()
    {
        defaultScaleVec = Vector3.one * defaultScale;
        ScaleChildren(defaultScaleVec);
    }
    public void ScaleChildren(Vector3 scale)
    {
        foreach (Transform child in transform)
        {
            child.localScale = scale;
        }
    }
}
