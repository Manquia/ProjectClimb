using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using System;

struct TriggerFade
{
}

// This is the controller of the splace screen object
public class SceneTransition : FFComponent {

    public float OpenFadeTime;
    public float PreFadeTime;
    public float FadeTime;
    public float PostFadeTime;

    public string LevelToLoad = "Level_01";
    
    public enum FadeTrigger
    {
        KeyPress,
        Trigger,
    }
    private FFAction.ActionSequence FadeSequence;
    public FadeTrigger trigger;

    // Use this for initialization
    void Start () {

        // Initialize
        {
            FadeSequence = action.Sequence();
        }

        // Fade Sequence
        {
            FadeSequence.Property(
                ffSpriteColor,
                new Color(ffSpriteColor.Val.r, ffSpriteColor.Val.g, ffSpriteColor.Val.b, 0.0f),
                FFEase.E_SmoothStart,
                OpenFadeTime);
            
            FadeSequence.Sync();
            FadeSequence.Delay(PreFadeTime);
            FadeSequence.Sync();

            if(trigger == FadeTrigger.KeyPress)
            {
                InputUpdate();
            }
            else
            {
                FFMessage<TriggerFade>.Connect(OnTriggerFade);
            }
        }
	}

    private void OnDestroy()
    {
        FFMessage<TriggerFade>.Disconnect(OnTriggerFade);
    }

    private int OnTriggerFade(TriggerFade e)
    {
        FadeToNextLevel();
        return 0;
    }

    // self queuing message
    void InputUpdate()
    {
        var fadeToNextLevel =
            Input.anyKey;

        if (fadeToNextLevel)
        {
            FadeToNextLevel();
        }
        else // continue to check for update
        {
            FadeSequence.Sync();
            FadeSequence.Call(InputUpdate);
        }
    }

    void FadeToNextLevel()
    {
        FadeSequence.Sync();
        FadeSequence.Property(
            ffSpriteColor,
            new Color(ffSpriteColor.Val.r, ffSpriteColor.Val.g, ffSpriteColor.Val.b, 1.0f),
            FFEase.E_SmoothStartEnd,
            FadeTime);

        FadeSequence.Sync();
        FadeSequence.Delay(PostFadeTime);

        FadeSequence.Sync();
        FadeSequence.Call(LoadTransitionLevel);
    }
    
    void LoadTransitionLevel()
    {
        SceneManager.LoadScene(LevelToLoad);
    }
	
}
