using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelIntroController : MonoBehaviour
{
    public Player player;
    public bool animationIsFinished = false;

	// Use this for initialization
	void Start ()
    {
	}
	
	// Update is called once per frame
	void Update ()
    {
        if(animationIsFinished)
        {
            FinishLevelIntroCutscene();
        }
    }
    

    public void FinishLevelIntroCutscene()
    {
        // enable player object
        player.enabled = true;

        // disable cimimatric Camera
        gameObject.SetActive(false);
    }
}
