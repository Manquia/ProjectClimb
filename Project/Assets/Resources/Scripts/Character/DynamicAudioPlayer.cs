using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class DynamicAudioPlayer : MonoBehaviour {

    [Serializable]
    public class DynamicAudioElement
    {
        public float toleranceThreshold;
        public AnimationCurve volumeCurve;
        public AnimationCurve pitchCurve;
        public AudioClip clip;
        public AudioSource src;
    }


    private FFRef<float> valueRef;
    private AudioSource audioSrc;
    public DynamicAudioElement[] elements;

	// Use this for initialization
	void Start ()
    {
        foreach (var element in elements)
        {
            element.src = gameObject.AddComponent<AudioSource>();
            element.src.clip = element.clip;
            element.src.loop = true;
            element.src.volume = 0.0f;
            
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
        var samplePoint = value / (element.toleranceThreshold + value);
        
        src.volume = element.volumeCurve.Evaluate(samplePoint);
        src.pitch =  element.pitchCurve.Evaluate(samplePoint);
    }
}
