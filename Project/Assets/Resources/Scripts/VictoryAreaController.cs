using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VictoryAreaController : MonoBehaviour {


    public Transform activateOnWin;
    public Inventory playerInventory;
    public Player player;

    public float winDelay = 1.5f;

    // Use this for initialization

    void Start()
    {
        FFMessageBoard<PlayerInteract.Use>.Connect(OnUse, gameObject);
    }
    private void OnDestroy()
    {
        FFMessageBoard<PlayerInteract.Use>.Disconnect(OnUse, gameObject);
    }

    private int OnUse(PlayerInteract.Use e)
    {
        if(playerInventory.hasPowerObject)
        {
            activateOnWin.gameObject.SetActive(true);
            player.LoadVictoryScreen(winDelay);
        }
        return 1;
    }

}
