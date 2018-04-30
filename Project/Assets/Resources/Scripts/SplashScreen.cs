using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SplashScreen : FFComponent
{

    public string selectedLevel = "Menu";


    public Color startColor;
    public Color endColor;
    public float fadeInTime = 1.5f;
    public float fadeOutTime = 0.9f;
    public float waitTime = 2.0f;
    public TextMesh pressAnyKeyText;


    FFAction.ActionSequence seq;
    // Use this for initialization
    void Start()
    {
        seq = action.Sequence();
        var spriteRend = GetComponent<SpriteRenderer>();
        spriteRend.enabled = true;
        spriteRend.color = startColor;


        // Fade in out of black into splashScreen
        var spriteColorRef = ffSpriteColor;
        seq.Property(spriteColorRef, spriteColorRef.Val.MakeClear(), FFEase.E_Continuous, fadeInTime);
        seq.Sync();

        // delay for wait time
        seq.Delay(waitTime);
        seq.Sync();

        // show text
        var textColorRef = new FFRef<Color>(() => pressAnyKeyText.color, (v) => pressAnyKeyText.color = v);
        textColorRef.Setter(textColorRef.Val.MakeClear());
        seq.Property(textColorRef, textColorRef.Val.MakeOpaque(), FFEase.E_Continuous, 0.45f);

        // Update Sequence for press any key
        seq.Sync();
        seq.Call(UpdateSeq);
    }

    // Update is called once per frame
    void UpdateSeq()
    {
        if (Input.anyKey)
        {

            // Load selected Level
            var spriteColorRef = ffSpriteColor;
            seq.Property(spriteColorRef, spriteColorRef.Val.MakeOpaque(), FFEase.E_Continuous, fadeOutTime);
            seq.Sync();
            seq.Call(LoadSelectedLevel);
        }
        else
        {
            // continue updating
            seq.Sync();
            seq.Call(UpdateSeq);
        }
    }
    

    private void LoadSelectedLevel()
    {
        SceneManager.LoadScene(this.selectedLevel);
    }
}
