using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

public class HUDContoller : FFComponent {

    public float fadeTime = 5.0f;
    public Animator anim;

    public Text helperText;
    public Text selfDestructText;


    FFAction.ActionSequence fadeSeq;
	// Use this for initialization
	void Start ()
    {
        fadeSeq = action.Sequence();

        if (SceneManager.GetActiveScene().name == "Level1")
        {
            FlashHelpText();
        }

        FFMessage<ResetLevel>.Connect(OnResetLevel);
        
        //FadeIn();
    }
    private void OnDestroy()
    {
        FFMessage<ResetLevel>.Disconnect(OnResetLevel);
    }


    float timeInactive = 0;
    public float timeInactiveFlashCooldown = 5.3f;
    bool keyPressedSinceStart = false;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
            fadeSeq.ClearSequence();
            helperText.gameObject.SetActive(false);
        }

        if (keyPressedSinceStart == false)
        {
            timeInactive += Time.deltaTime;
            keyPressedSinceStart = Input.anyKey;

            if (timeInactive > timeInactiveFlashCooldown)
            {
                timeInactive -= timeInactiveFlashCooldown;
                FlashHelpText();
            }
        }

        // Update destroy time
        if (selfDestructTimeRemaining > 0.0f)
        {
            selfDestructTimeRemaining -= Time.deltaTime;
            selfDestructTimeRemaining = Math.Max(selfDestructTimeRemaining, 0.0f);
            selfDestructText.text = "Remaining Time: " + selfDestructTimeRemaining.ToString("###.0");
        }
    }

    float selfDestructTimeRemaining = -1.0f;
    internal void DisplaySelfDestructCountdown(float selfDestructTime)
    {
        selfDestructTimeRemaining = selfDestructTime;

        FadeOut(selfDestructText);
    }
    private int OnResetLevel(ResetLevel e)
    {
        fadeSeq.ClearSequence();

        selfDestructTimeRemaining = -1.0f;
        selfDestructText.gameObject.SetActive(false);
        selfDestructText.color = selfDestructText.color.MakeClear();


        return 0;
    }

    private void FlashHelpText()
    {
        FadeOut(helperText);
        fadeSeq.Call(FadeIn, helperText);
    }


    // @REPEAT CODE from player
    void FadeOut(object graphic)
    {
        Graphic item = (Graphic)graphic;
        item.gameObject.SetActive(true);
        fadeSeq.Property(new FFRef<Color>(() => item.color, (v) => item.color = v), item.color.MakeOpaque(), FFEase.E_SmoothEnd, fadeTime);
        fadeSeq.Sync();
    }
    void FadeIn(object graphic)
    {
        Graphic item = (Graphic)graphic;
        fadeSeq.Property(new FFRef<Color>(() => item.color, (v) => item.color = v), item.color.MakeClear(), FFEase.E_SmoothStart, fadeTime);
        fadeSeq.Sync();
        fadeSeq.Call(DisableGameObject, item.gameObject);
        fadeSeq.Sync();
    }

    void DisableGameObject(object go)
    {
        GameObject gomeObject = (GameObject)go;
        gomeObject.SetActive(false);
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
