﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

struct ResetLevel
{
}

public class LevelManager : MonoBehaviour
{
    public string nextLevelName;
    public GameObject checkPointPrefab;
    public GameObject levelTransitionPrefab;
    public Player player;

    FFPath checkPointPositions;
    List<Transform> checkPointList = new List<Transform>();
    int currentCheckPointIndex;
    // Use this for initialization
    void Start()
    {
        checkPointPositions = GetComponent<FFPath>();
        //Debug.Assert(checkPointPositions.points.Length > 1, "We should have atleast 1 checkPoint and 1 level transitions");
        //Debug.Assert(player != null, "We should have a reference to the player so we can reset him");

        currentCheckPointIndex = 0;

        makeCheckpoints();
        makeLevelTransition();
    }


    private void Update()
    {
        if (Input.GetKey(KeyCode.R)) ResetPlayer();

        if (Input.GetKey(KeyCode.Alpha1)) TeleportToCheckPoint(0);
        if (Input.GetKey(KeyCode.Alpha2)) TeleportToCheckPoint(1);
        if (Input.GetKey(KeyCode.Alpha3)) TeleportToCheckPoint(2);
        if (Input.GetKey(KeyCode.Alpha4)) TeleportToCheckPoint(3);
        if (Input.GetKey(KeyCode.Alpha5)) TeleportToCheckPoint(4);
        if (Input.GetKey(KeyCode.Alpha6)) TeleportToCheckPoint(5);
        if (Input.GetKey(KeyCode.Alpha7)) TeleportToCheckPoint(6);
        if (Input.GetKey(KeyCode.Alpha8)) TeleportToCheckPoint(7);
        if (Input.GetKey(KeyCode.Alpha9)) TeleportToCheckPoint(8);
        if (Input.GetKey(KeyCode.Alpha0)) TeleportToCheckPoint(9);

        if(Input.GetKey(KeyCode.LeftControl))
        {
            if (Input.GetKey(KeyCode.Alpha1)) LoadLevel(0);
            if (Input.GetKey(KeyCode.Alpha2)) LoadLevel(1);
            if (Input.GetKey(KeyCode.Alpha3)) LoadLevel(2);
            if (Input.GetKey(KeyCode.Alpha4)) LoadLevel(3);
        }
    }
    void makeCheckpoints()
    {
        if (checkPointPrefab == null)
            return;

        for (int i = 0; i < checkPointPositions.points.Length - 1; ++i)
        {
            var cpGO = Instantiate(checkPointPrefab);
            var cpTrans = cpGO.transform;
            var cp = cpGO.GetComponent<Checkpoint>();

            cp.InitCheckPoint(this);
            cpTrans.position = checkPointPositions.PositionAtPoint(i);
            checkPointList.Add(cpTrans);
        }
    }
    void makeLevelTransition()
    {
        if (levelTransitionPrefab == null)
            return;

        var ltGO = Instantiate(levelTransitionPrefab);
        var ltTrans = ltGO.transform;
        var lt = ltGO.GetComponent<LevelTransition>();

        lt.InitLevelTransition(this);
        ltTrans.position = checkPointPositions.PositionAtPoint(checkPointPositions.points.Length - 1);
    }

    public void UpdateCheckPoint(Transform checkPoint)
    {
        var cpIndex = checkPointList.FindLastIndex((Transform trans) => trans == checkPoint);

        if (cpIndex > currentCheckPointIndex)
            currentCheckPointIndex = cpIndex;
    }

    bool IsInDeathPit = false;
    public void PlayerFellIntoDeathPit()
    {
        if (IsInDeathPit)
            return;

        player.fadeScreenSeq.ClearSequence();
        IsInDeathPit = true;
        player.Seq_FadeInScreenMasks(0.3f);
        player.fadeScreenSeq.Sync();
        player.fadeScreenSeq.Call(player.PlayDeathLandingSound);
        player.fadeScreenSeq.Call(ResetPlayer);
        player.fadeScreenSeq.Delay(0.9f);
        player.fadeScreenSeq.Sync();
        player.fadeScreenSeq.Call(player.Seq_FadeOutScreenMasks, 1.3f);
    }

    public void ResetPlayer()
    {
        IsInDeathPit = false;
        TeleportToCheckPoint(currentCheckPointIndex);
        ResetLevel();
    }

    // Helpers
    void TeleportToCheckPoint(int index)
    {
        if (checkPointList.Count == 0)
            return;

        index = Mathf.Min(index, checkPointList.Count - 1);
        var cp = checkPointList[index];

        player.transform.position = cp.position;
        ResetLevel();
    }
    void ResetLevel()
    {
        ResetLevel rl;
        FFMessage<ResetLevel>.SendToLocal(rl);
    }


    void LoadLevelOJB(object str_levelName)
    {
        SceneManager.LoadScene((string)str_levelName);
    }
    void LoadLevel(int levelIndex)
    {
        SceneManager.LoadScene(levelIndex);
    }
    internal void TransitionToNextLevel()
    {
        player.fadeScreenSeq.ClearSequence();
        player.Seq_FadeInScreenMasks();
        player.fadeScreenSeq.Call(LoadLevelOJB, nextLevelName);
    }

}
