using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class DynamicAudioPlayer : MonoBehaviour {

    [Serializable]
    public class DynamicAudioElement
    {
        //public enum Type
        //{
        //    Constant,
        //    Pulse,
        //}

        //public Type type;
        public bool fPulse = false;
        public AnimationCurve pulseCurve;

        public AnimationCurve volumeCurve;
        public AnimationCurve pitchCurve;
        public AudioClip clip;
        public AudioSource src;
    }


    private AudioSource audioSrc;
    private FFRef<float> valueRef;
    public float toleranceThreshold;
    public DynamicAudioElement[] elements;

	// Use this for initialization
	void Start ()
    {
        foreach (var element in elements)
        {
            element.src = gameObject.AddComponent<AudioSource>();
            element.src.clip = element.clip;
            element.src.loop = !element.fPulse;
            element.src.volume = 0.0f;

            if(element.fPulse == false)
                element.src.Play();
        }
	}
	
	// Update is called once per frame
	void Update ()
    {
		
	}

    public void SetDynamicValue(FFRef<float> valueRef)
    {
        this.valueRef = valueRef;
    }


    void FixedUpdate()
    {
        foreach (var element in elements)
        {
            UpdateAudioElement(element, Time.fixedDeltaTime);
        }
    }

    void UpdateAudioElement(DynamicAudioElement element, float dt)
    {
        var src = element.src;

        var value = valueRef;
        var samplePoint = value / (toleranceThreshold + value);
        
        src.volume = element.volumeCurve.Evaluate(samplePoint);
        src.pitch =  element.pitchCurve.Evaluate(samplePoint);

        if(element.fPulse) // check to see if we should play the sound since we aren't looping
        {
            var roll = UnityEngine.Random.Range(
                Mathf.Min(0.99f, element.pulseCurve.Evaluate(valueRef)),
                1.0f);
            
            if(roll > 1.0f - dt)
            {
                src.Play();
            }
        }
    }
}
