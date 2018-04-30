using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class UICommands : FFComponent
{
    public string StartupLevelName;
    public string CreditsLevelName;
    public string MenuLevelName;

    public UnityEngine.UI.Image fadeImage;
    public float fadeTime = 0.5f;
    
#region UnityEvent

    void Start()
    {
        loadSequence = action.Sequence();
        fadeInSequence = action.Sequence();
        // Always make the mouse visible on (UICommands should be in a menu level)
        CaptureMouse(false);
        
        // activate after the fade in is complete if we have a fadeImage
        if(fadeImage != null)
        {
            fadeImage.gameObject.SetActive(true);
            fadeInSequence.Property(new FFRef<Color>(() => fadeImage.color, (v) => fadeImage.color = v), fadeImage.color.MakeClear(), FFEase.E_Continuous, fadeTime);
            fadeInSequence.Sync();
            fadeInSequence.Call(DisableScreenFade);
        }
    }
    void DisableScreenFade()
    {
        fadeImage.gameObject.SetActive(false);
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            // @TODO @REMOVE, @CLEANUP
            ToggleInGamePanelUI();


            ClearLoadSequence();
        }
    }
    
    #endregion

    public void ToggleInGamePanelUI()
    {
        var panel = transform.Find("Panel");

        if(panel != null)
        {
            panel.gameObject.SetActive(!panel.gameObject.activeSelf);
        }
    }
    

    public void LoadLevel1(){ LoadHWAssignment(1); }
    public void LoadLevel2(){ LoadHWAssignment(2); }
    public void LoadLevel3(){ LoadHWAssignment(3); }
    public void LoadLevel4(){ LoadHWAssignment(4); }
    public void LoadLevel5(){ LoadHWAssignment(5); }
    public void LoadLevel6(){ LoadHWAssignment(6); }
    public void LoadLevel7(){ LoadHWAssignment(7); }

    FFAction.ActionSequence loadSequence;
    FFAction.ActionSequence fadeInSequence;

    public void LoadHWAssignment(int number)
    {
        loadSequence.ClearSequence();
        
        fadeImage.gameObject.SetActive(true);

        // Do fade out on loadSequence when we have a fade image
        if (fadeImage != null)
        {
            loadSequence.Property(
                new FFRef<Color>(() => fadeImage.color, (v) => fadeImage.color = v), Color.black, FFEase.E_Continuous, fadeTime);
        }

        loadSequence.Sync();
        loadSequence.Call(LoadLevelByName, "HW" + number);
    }
    void LoadLevelByName(object string_LevelName)
    {
        SceneManager.LoadScene((string)string_LevelName);
    }
    
    void ClearLoadSequence()
    {
        loadSequence.ClearSequence();
        if (fadeImage != null)
        {
            fadeImage.color = fadeImage.color.MakeClear();
            fadeImage.gameObject.SetActive(false);
        }
    }
    
    public void LoadCredits()
    {
        SceneManager.LoadScene(CreditsLevelName);
    }
    public void LoadMenu()
    {
        SceneManager.LoadScene(MenuLevelName);
    }
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
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
