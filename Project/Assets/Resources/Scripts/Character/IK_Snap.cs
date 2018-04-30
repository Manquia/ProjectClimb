using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class IK_Snap : MonoBehaviour {

    
    public void SetIK(bool active)
    {
        // Set all IK to active
        useIK = leftHandIK = rightHandIK = leftFootIK = rightFootIK = active;
    }

    public bool useIK = true;
    public bool leftHandIK;
    public bool rightHandIK;
    public bool leftFootIK;
    public bool rightFootIK;

    public Vector3 leftHandPos;
    public Vector3 rightHandPos;
    public Vector3 leftFootPos;
    public Vector3 rightFootPos;

    public Quaternion leftHandRot;
    public Quaternion rightHandRot;
    public Quaternion leftFootRot;
    public Quaternion rightFootRot;

    private Animator anim;

	// Use this for initialization
	void Start ()
    {
        anim = GetComponent<Animator>();
	}
	
	// Update is called once per frame
	void Update ()
    {
		
	}

    void FixedUpdate()
    {
        

    }


    void OnAnimatorIK()
    {
        if (useIK == false)
            return;

        //Debug.Log("Doing IK");
        if(leftHandIK)
        {
            anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1.0f);
            anim.SetIKPosition(AvatarIKGoal.LeftHand, leftHandPos);

            anim.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1.0f);
            anim.SetIKRotation(AvatarIKGoal.LeftHand, leftHandRot);
        }

        if (rightHandIK)
        {
            anim.SetIKPositionWeight(AvatarIKGoal.RightHand, 1.0f);
            anim.SetIKPosition(AvatarIKGoal.RightHand, rightHandPos);

            anim.SetIKRotationWeight(AvatarIKGoal.RightHand, 1.0f);
            anim.SetIKRotation(AvatarIKGoal.RightHand, rightHandRot);
        }

        if(leftFootIK)
        {
            anim.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1.0f);
            anim.SetIKPosition(AvatarIKGoal.LeftFoot, leftFootPos);

            anim.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 1.0f);
            anim.SetIKRotation(AvatarIKGoal.LeftFoot, leftFootRot);
        }

        if(rightFootIK)
        {
            anim.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1.0f);
            anim.SetIKPosition(AvatarIKGoal.RightFoot, rightFootPos);

            anim.SetIKRotationWeight(AvatarIKGoal.RightFoot, 1.0f);
            anim.SetIKRotation(AvatarIKGoal.RightFoot, rightFootRot);
        }

    }
}


/*
        // right hand IK check
        {
            var posRay = transform.position + (transform.forward * 0.25f) + (Vector3.up * 2.0f);
var vecRay = Vector3.Normalize(-transform.up + (Vector3.right * 0.5f));
var distOfRay = 1.0f;
Debug.DrawLine(posRay, posRay + vecRay* distOfRay, Color.red);
            if (Physics.Raycast(posRay, vecRay, out leftHit, distOfRay))
            {
                rightHandIK = true;
                rightHandPos = leftHit.point;
            }
            else
            {
                rightHandIK = false;
            }
        }

        // left hand IK check
        {
            var posRay = transform.position + (transform.forward * 0.25f) + (Vector3.up * 2.0f);
var vecRay = Vector3.Normalize(-transform.up + (-Vector3.right * 0.5f));
var distOfRay = 1.0f;
Debug.DrawLine(posRay, posRay + vecRay* distOfRay, Color.red);
            if (Physics.Raycast(posRay, vecRay, out rightHit, distOfRay))
            {
                leftHandIK = true;
                leftHandPos = rightHit.point;
            }
            else
            {
                leftHandIK = false;
            }

        }
        */