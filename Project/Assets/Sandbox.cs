using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sandbox : FFComponent {

    public FFAction.ActionSequence rotateSeq;
    public Controls myControls;

	// Use this for initialization
	void Start ()
    {
        rotateSeq = action.Sequence();
    }
	
	// Update is called once per frame
	void Update ()
    {
        if(Input.GetKey(KeyCode.R) && Input.GetKeyDown(KeyCode.Alpha1))
        {
            rotateSeq.Property(ffrotation, transform.localRotation * Quaternion.AngleAxis(30, Vector3.up), FFEase.E_SmoothStartEnd, 2.5f);
        }

        if (Input.GetKey(KeyCode.R) && Input.GetKeyDown(KeyCode.Alpha2))
        {
            rotateSeq.Property(ffrotation, transform.localRotation * Quaternion.AngleAxis(30, Vector3.right), FFEase.E_Continuous, 2.5f);
        }

        if (Input.GetKey(KeyCode.R) && Input.GetKeyDown(KeyCode.Alpha3))
        {
            rotateSeq.Property(ffrotation, transform.localRotation * Quaternion.AngleAxis(30, Vector3.forward), FFEase.E_SmoothEnd, 2.5f);
        }
    }
}
