using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoopingParallax : MonoBehaviour {

    public float speed;
    private Renderer rendererMat;

    // Start is called before the first frame update
    void Start() {
        rendererMat = GetComponent<Renderer>();
    }

    // Update is called once per frame
    void Update() {
        Vector2 offset = new Vector2(Time.time * speed, 0);

        rendererMat.material.mainTextureOffset = offset;
    }
}
