#if UNITY_EDITOR
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ExampleWindow : EditorWindow
{
    internal static ExampleWindow win;
    
    [MenuItem("Window/Custom/ExampleWindow")]
    public static void openWindow()
    {
        win = GetWindow<ExampleWindow>(false, "Example Window");
        win.Focus();
        win.Show();
    }


    Vector2 testScroll;
    Vector2 testScroll2;
    private void OnGUI()
    {
        if(GUILayout.Button("Close Me"))
        {
            if (win == null)
                win = GetWindow<ExampleWindow>(false, "Example Window");

            win.Close();
        }

        GUILayout.BeginHorizontal();
        GUILayout.TextArea("-- Horizontal 1 --");
        GUILayout.TextArea("-- Horizontal 2 --");
        GUILayout.EndHorizontal();

        testScroll = GUILayout.BeginScrollView(testScroll);

        // Things go ehere
        for(int i = 0; i < 25; ++i)
        {
            GUILayout.TextArea("Scroll View Area " + i);
        }
        
        GUILayout.EndScrollView();


        GUILayout.BeginHorizontal();
        GUILayout.TextArea("-- BOT Horizontal 1 --");
        GUILayout.TextArea("-- BOT Horizontal 2 --");
        GUILayout.EndHorizontal();


        testScroll2 = GUILayout.BeginScrollView(testScroll2);

        // Things go ehere
        for (int i = 0; i < 25; ++i)
        {
            GUILayout.BeginHorizontal();

            GUILayout.TextArea("A Left Area " + i);
            ++i;
            GUILayout.TextArea("A Middle Area " + i);
            ++i;
            GUILayout.TextArea("A Right Area " + i);

            GUILayout.EndHorizontal();
        }

        GUILayout.EndScrollView();
    }

}

#endif