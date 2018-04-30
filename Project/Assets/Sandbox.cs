using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sandbox : MonoBehaviour {


    public Controls myControls;

	// Use this for initialization
	void Start ()
    {

        var controsl = Instantiate(myControls);
        
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
