using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class physicsCharacterController : MonoBehaviour {

    // PRIVATE VARS YOU SHOULDN'T HAVE TO MODIFY MANUALLY
    private bool isGrounded;
    private Rigidbody rb;

    // MODIFY THESE PUBLIC VARS IN YOUR PROJECT VIA THE EDITOR
    // TO FEEL APPROPRIATE/FUN FOR THE SCALE OF YOUR ENVIRONMENT
    public float moveForce = 30f;
    public float jumpForce = 20f;
    public float normalLimit = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // FixedUpdate should be used instead of Update when dealing with Rigidbody
    // https://docs.unity3d.com/ScriptReference/MonoBehaviour.FixedUpdate.html

    void FixedUpdate()
    {
        // movement in the Z axis
        if (Input.GetAxisRaw("Vertical") > 0) rb.AddForce(transform.forward * moveForce);
        else if (Input.GetAxisRaw("Vertical") < 0) rb.AddForce(-transform.forward * moveForce);

        // movement in the X axis
        if (Input.GetAxisRaw("Horizontal") > 0) rb.AddForce(transform.right * moveForce);
        else if (Input.GetAxisRaw("Horizontal") < 0) rb.AddForce(-transform.right * moveForce);

        // movement in the Y axis (jump)
        if (isGrounded && Input.GetButtonDown("Jump"))
        {
            isGrounded = false;
            rb.AddForce(transform.up * jumpForce);
        }
    }

    // player is colliding
    void OnCollisionStay(Collision other)
    {
        // https://docs.unity3d.com/ScriptReference/Collision.html
        foreach (ContactPoint contact in other.contacts)
        {
            // note: this debug ray does not draw when physics is asleep
            Debug.DrawRay(contact.point, contact.normal, Color.white);

            if (contact.normal.y > normalLimit) // the player is sitting on something
            {
                isGrounded = true;
            }
        }
    }

    // player is no longer colliding
    void OnCollisionExit(Collision other)
    {
         isGrounded = false;
    }
}
