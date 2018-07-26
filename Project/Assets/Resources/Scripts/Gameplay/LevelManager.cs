using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        Debug.Assert(checkPointPositions.points.Length > 1, "We should have atleast 1 checkPoint and 1 level transitions");
        Debug.Assert(player != null, "We should have a reference to the player so we can reset him");

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
    }
    void makeCheckpoints()
    {
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
        var ltGO = Instantiate(levelTransitionPrefab);
        var ltTrans = ltGO.transform;
        var lt = ltGO.GetComponent<LevelTransition>();

        lt.LevelName = nextLevelName;
        ltTrans.position = checkPointPositions.PositionAtPoint(checkPointPositions.points.Length - 1);
    }

    public void UpdateCheckPoint(Transform checkPoint)
    {
        var cpIndex = checkPointList.FindLastIndex((Transform trans) => trans == checkPoint);

        if (cpIndex > currentCheckPointIndex)
            currentCheckPointIndex = cpIndex;
    }

    public void ResetPlayer()
    {
        TeleportToCheckPoint(currentCheckPointIndex);
        ResetLevel();
        // @TODO you died b/c you fell or something!!! or something...
    }

    void TeleportToCheckPoint(int index)
    {
        index = Mathf.Min(index, checkPointList.Count - 1);
        var cp = checkPointList[index];

        player.transform.position = cp.position;
    }

    void ResetLevel()
    {
        ResetLevel rl;
        FFMessage<ResetLevel>.SendToLocal(rl);
    }

}
