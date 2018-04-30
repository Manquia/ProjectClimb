using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

public class TowerCreatorWindow : EditorWindow {

    Vector3 spawnPos;
    GameObject spawnPosObj;
    GameObject towerRoot;

    public enum Stage
    {
        Idle,
        Create,
        Modify,
    }
    Stage stage;

    [MenuItem("Tools/TowerCreator")]
    static void Init()
    {
        Debug.Log("Init TowerCreatorWindow");
        // Get existing open window or if none, make a new one:
        TowerCreatorWindow window = (TowerCreatorWindow)EditorWindow.GetWindow(typeof(TowerCreatorWindow));
        window.Show();
    }
    private void OnDisable()
    {
        Debug.Log("Disable TowerCreatorWindow");
        if (spawnPosObj)
            DestroyImmediate(spawnPosObj);
    }
    
    void OnGUI()
    {
        if(spawnPosObj == null)
        {
            spawnPosObj = new GameObject("DebugPT");
            spawnPosObj.AddComponent<FFDebugDrawBox>();
            var line = spawnPosObj.AddComponent<FFDebugDrawLine>();
            line.End = line.Start + (Vector3.up * 10.0f);
            line.DrawColorSelected = Color.green * 0.9f;
            line.DrawColor = Color.green * 0.5f;
        }
        
        GUILayout.Label("Tower Creator", EditorStyles.boldLabel);

        //stage = (Stage)EditorGUILayout.EnumPopup("Stage", stage);

        // selection changed AND is not a tower selection
        if(towerRoot != UnityEditor.Selection.activeGameObject &&
            IsTowerRoot(Selection.activeGameObject) == false)
        {
            stage = Stage.Idle;
            towerRoot = null;
        }



        towerRoot = UnityEditor.Selection.activeGameObject;
        if (IsTowerRoot(towerRoot))
        {
            switch (stage)
            {
                case Stage.Create:
                    Debug.Log("Create, When we already have a tower selected");
                    stage = Stage.Modify;
                    goto case Stage.Modify; // fallthrough
                case Stage.Modify:
                    UpdateTower();
                    break;
                case Stage.Idle:
                    stage = Stage.Modify; // started modifying a thing
                    break;
            }
        }
        else // not GenTower is being Created/Modified
        {
            switch (stage)
            {
                case Stage.Create:
                    towerRoot = new GameObject("GenTower");
                    towerRoot.transform.position = spawnPosObj.transform.position;
                    var path = towerRoot.AddComponent<FFPath>();
                    path.points[1] = Vector3.up * 5.0f;
                    path.points[2] = Vector3.up * 10.0f;
                    path.InterpolationType = FFPath.PathInterpolator.Curved;
                    UnityEditor.Selection.activeGameObject = towerRoot;
                    stage = Stage.Modify;

                    Create();
                    break;
                case Stage.Modify:
                    //Debug.Log("Modify, when no object has been selected");
                    stage = Stage.Idle;
                    goto case Stage.Idle; // fallthrough
                case Stage.Idle:
                    if (EditorGUILayout.Toggle("Create", false))
                        goto case Stage.Create;
                    break;
            }
        }
    }

    bool IsTowerRoot(GameObject go)
    {
        return towerRoot != null && towerRoot.name.Contains("GenTower") && towerRoot.GetComponent<FFPath>();
    }


    void Create()
    {
    }

    private void UpdateTower()
    {
    }

}
