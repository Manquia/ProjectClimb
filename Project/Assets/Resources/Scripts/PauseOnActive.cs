using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseOnActive : MonoBehaviour
{     
    //@TODO @HACK probably seperate this logic out or just make a pause contoller

    float oldTimeScale = 1.0f;
    private void OnEnable()
    {
        oldTimeScale = Time.timeScale;
        Time.timeScale = 0.0f;
        CaptureMouse(false);
    }
    private void OnDisable()
    {
        Time.timeScale = oldTimeScale;
        CaptureMouse(true);
    }


    public void CaptureMouse(bool capture)
    {
        if (capture)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }
}
