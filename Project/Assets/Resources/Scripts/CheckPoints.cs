using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPoints : MonoBehaviour {


    List<Transform> checkPoints = new List<Transform>();
    public Player player;

	// Use this for initialization
	void Start ()
    {
        // Get Checkpoints
        foreach(Transform child in transform)
        {
            checkPoints.Add(child);
        }
		
	}
	
	// Update is called once per frame
	void Update ()
    {
        //if(Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
        //{
            if(Input.GetKey(KeyCode.Alpha1)) TeleportToCheckPoint(0);
            if(Input.GetKey(KeyCode.Alpha2)) TeleportToCheckPoint(1);
            if(Input.GetKey(KeyCode.Alpha3)) TeleportToCheckPoint(2);
            if(Input.GetKey(KeyCode.Alpha4)) TeleportToCheckPoint(3);
            if(Input.GetKey(KeyCode.Alpha5)) TeleportToCheckPoint(4);
        //}
	}

    private void TeleportToCheckPoint(int CheckPointindex)
    {
        if(CheckPointindex < checkPoints.Count)
        {
            player.transform.position = checkPoints[CheckPointindex].position;
        }
    }

}
