using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelTransition : MonoBehaviour
{
    internal string LevelName;

    private void Start()
    {
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
        SceneManager.LoadScene(LevelName);
    }
}
