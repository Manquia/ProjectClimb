using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class IK_Snap : MonoBehaviour
{


    public void SetIK(bool active)
    {
        // Set all IK to active
        if (active)
        {
            useIK = leftHandIK = rightHandIK = leftFootIK = rightFootIK = active;
        }
        else
        {
            useIK = leftHandIK = rightHandIK = leftFootIK = rightFootIK = active;
            ResetIKValues();
        }
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
    void Start()
    {
        anim = GetComponent<Animator>();

        // record animation data from the start so that we can revert back from whatever IK we had...
        {
            IKPoint pt;
            pt.rotWeight = 0.0f;
            pt.posWeight = 0.0f;
            pt.rot = Quaternion.identity;
            pt.pos = Vector3.zero;

            pt.goal = AvatarIKGoal.LeftFoot; startingIKPoints.Add(pt);
            pt.goal = AvatarIKGoal.LeftHand; startingIKPoints.Add(pt);
            pt.goal = AvatarIKGoal.RightFoot; startingIKPoints.Add(pt);
            pt.goal = AvatarIKGoal.RightHand; startingIKPoints.Add(pt);

            for (int i = 0; i < startingIKPoints.Count; ++i)
            {
                pt = startingIKPoints[i];
                pt.rotWeight = anim.GetIKRotationWeight(pt.goal);
                pt.posWeight = anim.GetIKPositionWeight(pt.goal);
                pt.rot = anim.GetIKRotation(pt.goal);
                pt.pos = anim.GetIKPosition(pt.goal);
                startingIKPoints[i] = pt;
            }
        }


    }

    void ResetIKValues()
    {
        foreach (var pt in startingIKPoints)
        {
            anim.SetIKRotationWeight(pt.goal, pt.rotWeight);
            anim.SetIKPositionWeight(pt.goal, pt.posWeight);
            anim.SetIKRotation(pt.goal, pt.rot);
            anim.SetIKPosition(pt.goal, pt.pos);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    void FixedUpdate()
    {


    }

    struct IKPoint
    {
        public AvatarIKGoal goal;
        public float rotWeight;
        public Quaternion rot;
        public float posWeight;
        public Vector3 pos;
    }
    List<IKPoint> startingIKPoints = new List<IKPoint>();

    void OnAnimatorIK()
    {
        if (useIK == false)
            return;

        //Debug.Log("Doing IK");
        if (leftHandIK)
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

        if (leftFootIK)
        {
            anim.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1.0f);
            anim.SetIKPosition(AvatarIKGoal.LeftFoot, leftFootPos);

            anim.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 1.0f);
            anim.SetIKRotation(AvatarIKGoal.LeftFoot, leftFootRot);
        }

        if (rightFootIK)
        {
            anim.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1.0f);
            anim.SetIKPosition(AvatarIKGoal.RightFoot, rightFootPos);

            anim.SetIKRotationWeight(AvatarIKGoal.RightFoot, 1.0f);
            anim.SetIKRotation(AvatarIKGoal.RightFoot, rightFootRot);
        }

    }
}