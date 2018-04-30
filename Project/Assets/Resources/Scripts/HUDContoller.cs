using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HUDContoller : MonoBehaviour {


    public Animator anim;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update ()
    {

        if(Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
		
	}

    bool paused = false;
    public void TogglePause()
    {
        paused = !paused;

        if (paused)
        {
            anim.SetBool("Open", true);
            Pause(true);
        }
        else
        {
            anim.SetBool("Open", false);
            Pause(false);
        }
    }


    float oldTimeScale = 1.0f;
    private void Pause(bool active)
    {
        if (active)
        {
            oldTimeScale = Time.timeScale;
            Time.timeScale = 0.0f;
            CaptureMouse(false);
        }
        else
        {
            Time.timeScale = oldTimeScale;
            CaptureMouse(true);
        }
    }

    public void CaptureMouse(bool capture)
    {
        if (capture)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

}
