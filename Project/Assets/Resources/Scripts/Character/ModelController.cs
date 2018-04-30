using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModelController : MonoBehaviour {


    public float leftFootPositionWeight;
    public float leftFootRotationWeight;
    public Transform leftFootObj;


    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }
    void OnAnimatorIK(int layerIndex)
    {
        animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, leftFootPositionWeight);
        animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, leftFootRotationWeight);
        animator.SetIKPosition(AvatarIKGoal.LeftFoot, leftFootObj.position);
        animator.SetIKRotation(AvatarIKGoal.LeftFoot, leftFootObj.rotation);
    }
    
}
