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


    public Light[] lightsToEnable;
    public MeshRenderer[] backDropsToLight;

    public Material backDropLightOffMaterial;
    public Material backDropLightOnMaterial;

    public float selfDestructTime = 60.0f;
    public ProxyOnUse button;


    FFAction.ActionSequence destructSeq;

	// Use this for initialization
	void Start ()
    {
        Alarm(false);
        FFMessageBoard<PlayerInteract.Use>.Connect(OnUse, gameObject);
        FFMessage<ResetLevel>.Connect(OnResetLevel);

        destructSeq = action.Sequence();
    }
    private void OnDestroy()
    {
        FFMessageBoard<PlayerInteract.Use>.Disconnect(OnUse, gameObject);
        FFMessage<ResetLevel>.Disconnect(OnResetLevel);
    }

    void Alarm(bool active)
    {
        var material = active ? backDropLightOnMaterial : backDropLightOffMaterial;

        // set Lights lights
        foreach (var light in lightsToEnable)
        {
            light.gameObject.SetActive(active);
        }
        // set materials
        foreach (var rend in backDropsToLight)
        {
            rend.material = material;
        }
    }

    private int OnResetLevel(ResetLevel e)
    {
        sirenSource.Stop();
        button.used = false;
        Alarm(false);

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
        // @ HACK @CLEANUP
        Alarm(true);

        sirenSource.Play();
        playerHUD.DisplaySelfDestructCountdown(selfDestructTime);
        player.fadeScreenSeq.ClearSequence();
        player.fadeScreenSeq.Delay(selfDestructTime);
        player.fadeScreenSeq.Sync();
        player.fadeScreenSeq.Call(PlayExplosionSound);
        player.Seq_FadeInScreenMasks(1.4f);
        player.fadeScreenSeq.Sync();
        player.fadeScreenSeq.Call(levelManager.ResetPlayer);
        player.fadeScreenSeq.Delay(0.9f);
        player.fadeScreenSeq.Sync();
        player.fadeScreenSeq.Call(player.Seq_FadeOutScreenMasks, 1.3f);

    }

    void PlayExplosionSound()
    {
        explosionAudioSource.Play();
    }

}
