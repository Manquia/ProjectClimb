using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SpinMotor : MonoBehaviour {

    public Vector3 axis;
    public float speed;


    Rigidbody rigidBody;
    float degrees = 0;
	// Use this for initialization
	void Start ()
    {
        rigidBody = GetComponent<Rigidbody>();


        rigidBody.useGravity = false;
        rigidBody.isKinematic = true;
        rigidBody.constraints = RigidbodyConstraints.FreezeAll;
        
	}
	
	// Update is called once per frame
	void Update ()
    {
        degrees += Time.deltaTime * speed;
        degrees -= Mathf.Floor(degrees / 360.0f) * 360.0f;

        transform.localRotation = Quaternion.AngleAxis(degrees, axis);
    }
}
