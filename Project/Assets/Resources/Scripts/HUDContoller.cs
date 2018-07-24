using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUDContoller : FFComponent {

    public Image fadeImage;
    public float fadeTime = 1.2f;
    public Animator anim;


    FFAction.ActionSequence fadeSeq;
	// Use this for initialization
	void Start () {

        //fadeSeq = action.Sequence();
        //FadeIn();
    }


    // @REPEAT CODE from player
    void FadeOut()
    {
        fadeImage.gameObject.SetActive(true);
        fadeSeq.Property(new FFRef<Color>(() => fadeImage.color, (v) => fadeImage.color = v),fadeImage.color.MakeOpaque(), FFEase.E_Continuous, fadeTime);
        fadeSeq.Sync();
    }
    void FadeIn()
    {
        fadeSeq.Property(new FFRef<Color>(() => fadeImage.color, (v) => fadeImage.color = v), fadeImage.color.MakeClear(), FFEase.E_Continuous, fadeTime);
        fadeSeq.Sync();
        fadeSeq.Call(DisableGameObject, fadeImage.gameObject);
        fadeSeq.Sync();
    }

    void DisableGameObject(object go)
    {
        GameObject gomeObject = (GameObject)go;
        gomeObject.SetActive(false);
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
