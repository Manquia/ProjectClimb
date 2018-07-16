using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour {

	// Use this for initialization
	void Start ()
    {
		
	}

    LevelManager levelManager;
    public void InitCheckPoint(LevelManager lm)
    {
        levelManager = lm;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            levelManager.UpdateCheckPoint(transform);
        }
    }
}
