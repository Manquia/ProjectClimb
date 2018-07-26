using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RootLerp : MonoBehaviour {

    public float lerpFraction = 0.4f;
    public Vector3 offset;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void LateUpdate()
    {
        transform.localPosition = Vector3.Lerp(transform.localPosition, offset, lerpFraction);
    }
}
