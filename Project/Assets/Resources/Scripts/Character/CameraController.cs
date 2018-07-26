using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : FFComponent {

    Player player;
    Player.Mode mode
    {
        get { return player.mode;  }
    }
    public Vector2 lookSensitivity = new Vector2(10.0f,10.0f);

    public float maxPitchUpAngle = 80.0f;
    public float maxPitchDownAngle = 70.0f;
    public float maxTurnAngle = 75.0f;

    public Transform LerpTarget;
    public float lerpFraction;

    internal Transform cameraTrans;


	
    public void SetupCameraController(Player player)
    {
        this.player = player;

        cameraTrans = transform.Find("Camera");

        Debug.Assert(cameraTrans != null, "Camera is missing from camera");
        SetCursorState(CursorLockMode.Locked);

    }
    void Destroy()
    {
        SetCursorState(CursorLockMode.None);
    }
	
	// Update is called once per frame
	void Update ()
    {
        UpdateRotation();
        UpdatePosition();
    }
    
    // return look vector
    public float turnVector()
    {
        if(lookVec.x > 0.0f || cameraTurn == maxTurnAngle)
        {
            return 1.0f;
        }
        if(lookVec.x < 0.0f || cameraTurn == -maxTurnAngle)
        {
            return -1.0f;
        }
        return lookVec.x;
    }


    internal Vector2 lookVec;
    // Store this becuase Unity's rotation isn't good
    public float cameraTurn = 0.0f;
    private float cameraPitch = 0.0f;
    private void UpdateRotation()
    {
        lookVec.x = lookSensitivity.x * Input.GetAxis("Mouse X") * Time.deltaTime;
        lookVec.y = lookSensitivity.y * Input.GetAxis("Mouse Y") * Time.deltaTime;

        // @TODO @CLEANUP @ROPE @LookBox/Bounds?
        if(lookVec.x != 0.0f && mode == Player.Mode.Rope)
        { // rotate player relative to mouse movement
            // Limit rotation
            var turnAngle = Mathf.Clamp(cameraTurn + lookVec.x, -maxTurnAngle, maxTurnAngle);
            float angleDelta = turnAngle - cameraTurn;
            var turnRotDelta = Quaternion.AngleAxis(angleDelta, Vector3.up);
            transform.localRotation = transform.localRotation * turnRotDelta;
        
            cameraTurn = turnAngle;
        }

        if (lookVec.y != 0.0f)
        { // Pitch Camera
            // Limit rotation
            var pitchAngle = Mathf.Clamp(cameraPitch + lookVec.y, -maxPitchDownAngle, maxPitchUpAngle);
            float angleDelta = pitchAngle - cameraPitch;
            var pitchRotDelta = Quaternion.AngleAxis(angleDelta, -Vector3.right);
            cameraTrans.localRotation = cameraTrans.localRotation * pitchRotDelta;

            cameraPitch = pitchAngle;
        }

    }


    void UpdatePosition()
    {
        if(LerpTarget != null)
        {
            transform.position = Vector3.Lerp(transform.position, LerpTarget.position, lerpFraction);
        }
    }
    


    void SetCursorState(CursorLockMode mode)
    {
        Cursor.lockState = mode;
        // Hide cursor when locking
        Cursor.visible = (CursorLockMode.Locked != mode);
    }
}
