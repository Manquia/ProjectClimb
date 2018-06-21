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
        if (Input.GetKey(KeyCode.R))
        {
            test();

        }


    }


    void test()
    {
        CapsuleCollider col;
        Rigidbody body = GetComponent<Rigidbody>();

        body.MovePosition(transform.position + Vector3.forward * 0.5f);


    }
}
