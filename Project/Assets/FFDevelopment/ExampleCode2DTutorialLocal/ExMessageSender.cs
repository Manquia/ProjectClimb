using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System;

// To Run, Press "K" while in game.

/* Messages for FFMessage or FFMessageBox can be created from any
 * type but it is a good practice to name then appropriatly and
 * place then into a logical place. (although F12 does a good job
 * of finding their location).
 * 
 * Event types should all be public structs, and in the case that
 * you must use a class be sure to make sure its not a
 * null reference.
 */

// Example Event TODO make this use logical stuff
[Serializable]
public class PlayerDiedEvent
{
    public float timeOfDeath;
    public float overkillDamage;
    public List<GameObject> Assists = new List<GameObject>();
    public GameObject Weapon; // bullet,sword,etc...
    public GameObject Killer;
    public GameObject PlayerDied;
}

public class ExMessageSender : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        
        if(Input.GetKeyUp(KeyCode.K))  // Activator Triggered
        {
            PlayerDiedEvent e = new PlayerDiedEvent();
            e.Killer = gameObject;
            e.timeOfDeath = Time.realtimeSinceStartup;
            e.overkillDamage = 9001.0f;
            FFMessage<PlayerDiedEvent>.SendToLocal(e);
        }
	
	}
}
