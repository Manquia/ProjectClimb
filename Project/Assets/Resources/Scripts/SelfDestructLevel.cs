using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfDestructLevel : FFComponent
{
    public LevelManager levelManager;
    public HUDContoller playerHUD;
    public Player player;

    public AudioSource sirenSource;
    public AudioSource explosionAudioSource;

    public float selfDestructTime = 60.0f;
    public ProxyOnUse button;


    FFAction.ActionSequence destructSeq;

	// Use this for initialization
	void Start ()
    {
        FFMessageBoard<PlayerInteract.Use>.Connect(OnUse, gameObject);
        FFMessage<ResetLevel>.Connect(OnResetLevel);

        destructSeq = action.Sequence();
    }
    private void OnDestroy()
    {
        FFMessageBoard<PlayerInteract.Use>.Disconnect(OnUse, gameObject);
        FFMessage<ResetLevel>.Disconnect(OnResetLevel);
    }

    private int OnResetLevel(ResetLevel e)
    {
        sirenSource.Stop();
        destructSeq.ClearSequence();
        button.used = false;

        // Reset stuff here
        return 0;
    }

    private int OnUse(PlayerInteract.Use e)
    {
        StartSelfDistrucdtSequence();
        return 0;
    }

    void StartSelfDistrucdtSequence()
    {
        sirenSource.Play();
        playerHUD.DisplaySelfDestructCountdown(selfDestructTime);

        destructSeq.Delay(selfDestructTime);

        destructSeq.Sync();
        destructSeq.Call(PlayExplosionSound);
        destructSeq.Call(player.Seq_FadeInScreenMasks, 0.6f);
        destructSeq.Delay(0.6f);

        destructSeq.Sync();
        destructSeq.Delay(0.6f);
        destructSeq.Call(levelManager.ResetPlayer);

        destructSeq.Sync();
        destructSeq.Call(player.Seq_FadeInScreenMasks, 0.6f);
    }

    void PlayExplosionSound()
    {
        explosionAudioSource.Play();
    }

}
