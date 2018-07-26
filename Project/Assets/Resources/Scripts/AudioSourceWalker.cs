using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioSourceWalker : FFComponent {

    AudioSource audioSrc;
    FFAction.ActionSequence walkerSeq;

	// Use this for initialization
	void Start ()
    {
        audioSrc = GetComponent<AudioSource>();
        walkerSeq = action.Sequence();

        basePitch     = audioSrc.pitch;
        baseVolume    = audioSrc.volume;
        baseSterioPan = audioSrc.panStereo;


        WalkAudio();
    }

    internal float basePitch     = 0.1f;
    internal float baseVolume    = 0.1f;
    internal float baseSterioPan = 0.1f;

    public float pitchVolumePairDelta = 0.1f;
    public float pitchDelta     = 0.1f;
    public float volumeDelta    = 0.1f;
    public float sterioPanDelta = 0.1f;

    public float transitionTimeMedian = 5.0f;
    public float transitionTimeDelta = 2.5f;

    FFRef<float> audioPitchRef()
    {
        return new FFRef<float>(() => audioSrc.pitch, (v) => { audioSrc.pitch = v; });
    }

    FFRef<float> audioVolumeRef()
    {
        return new FFRef<float>(() => audioSrc.volume, (v) => { audioSrc.volume = v; });
    }

    FFRef<float> audioSterioPanRef()
    {
        return new FFRef<float>(() => audioSrc.panStereo, (v) => { audioSrc.panStereo = v; });
    }
    void WalkAudio()
    {
        float time = Random.Range(-transitionTimeDelta, transitionTimeDelta) + transitionTimeMedian;
        walkerSeq.ClearSequence();
        var pitchDeltaSeq = Random.Range(-pitchDelta, pitchDelta);
        var volumeDeltaSeq = Random.Range(-volumeDelta, volumeDelta);
        var sterioPanDeltaSeq = Random.Range(-sterioPanDelta, sterioPanDelta);

        var audioPitchReference = audioPitchRef();
        var audioVolumeReference = audioVolumeRef();

        if(pitchDeltaSeq != 0.0f)       walkerSeq.Property(audioPitchReference, basePitch       + pitchDeltaSeq,     FFEase.E_SmoothStartEnd, time); // below is not indendent @TODO?
        if(volumeDeltaSeq != 0.0f)      walkerSeq.Property(audioVolumeReference, baseVolume     + volumeDeltaSeq,    FFEase.E_SmoothStartEnd, time); // below is not indendent @TODO?
        if(sterioPanDeltaSeq != 0.0f)   walkerSeq.Property(audioSterioPanRef(), baseSterioPan   + sterioPanDeltaSeq, FFEase.E_SmoothStartEnd, time); // below is not indendent @TODO?

        var pitchVolumeDelta = Random.Range(-pitchVolumePairDelta, pitchVolumePairDelta);
        if(pitchVolumeDelta != 0.0f) walkerSeq.Property(audioPitchReference, audioPitchReference + pitchDeltaSeq + pitchVolumeDelta, FFEase.E_SmoothStartEnd, time);
        if(pitchVolumeDelta != 0.0f) walkerSeq.Property(audioVolumeReference, audioVolumeReference + volumeDeltaSeq + pitchVolumeDelta, FFEase.E_SmoothStartEnd, time);

        walkerSeq.Delay(time);

        walkerSeq.Sync();
        walkerSeq.Call(WalkAudio);
    }
	


}
