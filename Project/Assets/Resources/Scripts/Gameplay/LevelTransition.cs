using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelTransition : MonoBehaviour
{
    LevelManager levelManager;
    public void InitLevelTransition(LevelManager lm)
    {
        levelManager = lm;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            LoadNextLevel();
        }
    }

    void LoadNextLevel()
    {
        levelManager.TransitionToNextLevel();
    }
}
