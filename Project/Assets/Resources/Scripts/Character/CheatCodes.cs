using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class CheatCodes : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

        if (Input.GetKey(KeyCode.Alpha1)) SceneManager.LoadScene("Demo 1");
        if (Input.GetKey(KeyCode.Alpha2)) SceneManager.LoadScene("Demo 2");
        if (Input.GetKey(KeyCode.Alpha3)) SceneManager.LoadScene("Demo 3");

        if (Input.GetKey(KeyCode.Escape)) Application.Quit();
    }
}
