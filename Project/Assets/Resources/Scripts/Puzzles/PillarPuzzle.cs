using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PillarPuzzle : MonoBehaviour {


    public Transform pillarsRoot;
    struct PillarData
    {
        public bool state;
        public float mu;
        public Transform transform;
    }

    bool puzzleIsOn = true;
    PillarData[] data  = new PillarData[9];


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
                data[i].transform = child;
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
        // change mu
        for(int i = 0; i < data.Length; ++i)
        {
            float muDelta = (1.0f / pillarMoveTime);
            float dir = ((data[i].state || puzzleIsOn) && !(data[i].state && puzzleIsOn)) ? 1.0f : -1.0f;

            data[i].mu += dt * muDelta * dir;
            data[i].mu = Mathf.Clamp(data[i].mu, 0.0f, 1.0f);
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
