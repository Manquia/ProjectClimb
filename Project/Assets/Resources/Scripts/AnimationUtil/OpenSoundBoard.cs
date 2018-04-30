using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class OpenSoundBoard : MonoBehaviour
{
    public AudioClip sound1;
    public AudioClip sound2;
    public AudioClip sound3;
    public AudioClip sound4;

    AudioSource src;


    // Use this for initialization
    void Start ()
    {
        src = GetComponent<AudioSource>();	
	}


    public void PlaySound1() { src.PlayOneShot(sound1); }
    public void PlaySound2() { src.PlayOneShot(sound2); }
    public void PlaySound3() { src.PlayOneShot(sound3); }
    public void PlaySound4() { src.PlayOneShot(sound4); }
}
