using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PillarPuzzle : MonoBehaviour {

    public float speedDelta = 0.6f;

    public Transform pillarsRoot;
    struct PillarData
    {
        internal bool state;
        internal float mu;
        internal float speed;
        internal Transform transform;
        internal AudioSource audioSrc;
    }

    bool puzzleIsOn = true;
    PillarData[] data  = new PillarData[9];

    public AudioClip[] PillarMoveSounds;


    // Use this for initialization
    void Start ()
    {
        SetupStates();
        FFMessageBoard<PlayerInteract.Use>.Connect(OnUse, gameObject);
	}

    private int OnUse(PlayerInteract.Use e)
    {
        TogglePuzzleState();
        return 1;
    }
    void TogglePuzzleState()
    {
        // @TODO sounds
        puzzleIsOn = !puzzleIsOn;
    }

    void SetupStates()
    {
        // add pillar transforms
        {
            int i = 0;
            Debug.Assert(pillarsRoot.childCount == 9);
            foreach (Transform child in pillarsRoot)
            {
                var audioSrc = child.GetComponent<AudioSource>();
                data[i].transform = child;
                data[i].speed = 1.0f + UnityEngine.Random.Range(-speedDelta, speedDelta);
                data[i].audioSrc = audioSrc;
                audioSrc.clip = PillarMoveSounds.SampleRandom(null);
                audioSrc.volume = 0.0f;
                audioSrc.Play();
                ++i;
            }
        }

        data[0].state = true;
        data[1].state = true;
        data[2].state = true;
        data[3].state = false;
        data[4].state = false;
        data[5].state = true;
        data[6].state = false;
        data[7].state = true;
        data[8].state = true;
    }


    // Update is called once per frame
    void Update ()
    {
        float dt = Time.deltaTime;
        MovePillars(dt);
	}

    public float pillarMoveTime = 1.5f;
    public float pillarRaisedHeight = 0.0f;
    public float pillarLoweredHeight = -10.0f;

    void MovePillars(float dt)
    {
        const float volumePower = 8.0f;
        const float pitchPower = 8.0f;
        const float pitchDelta = 0.3f;

        // change mu
        for(int i = 0; i < data.Length; ++i)
        {
            var audioSrc = data[i].audioSrc;
            float muDelta = (1.0f / pillarMoveTime);
            float dir = ((data[i].state || puzzleIsOn) && !(data[i].state && puzzleIsOn)) ? 1.0f : -1.0f;

            data[i].mu += dt * muDelta * dir * data[i].speed;
            data[i].mu = Mathf.Clamp(data[i].mu, 0.0f, 1.0f);

            // Basic Formula
            // 1 - (2mu -1)^HighEvenPower
            float mu = data[i].mu;

            // Range(0, 1) 
            float volume = 1 - Mathf.Pow(2 * mu - 1, volumePower);
            // Range(1 - pitchDelta, 1 + pitchDelta) 
            float pitch = ((1 - Mathf.Pow(2 * mu - 1, pitchPower) * (2* pitchDelta)) - pitchDelta) + 1;

            audioSrc.volume = volume;
            audioSrc.pitch = pitch;
        }

        // update pillar positions
        for (int i = 0; i < data.Length; ++i)
        {
            var pos = data[i].transform.localPosition;
            data[i].transform.localPosition = new Vector3(
                pos.x,
                Mathf.Lerp(pillarLoweredHeight, pillarRaisedHeight, data[i].mu),
                pos.z);
        }

    }

}
