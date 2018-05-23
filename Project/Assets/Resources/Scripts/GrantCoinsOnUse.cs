using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public struct GiveCoins
{
    public int coinCount;
}
public class GrantCoinsOnUse : MonoBehaviour {

    public bool singleUse = true;
    bool used = false;
    public int coinCount = 5;
    public int coinCountVarience = 3;

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
        if (singleUse && used)
            return 0;

        if (singleUse)
            used = true;

        GrantCoins(e);
        return 1;
    }



    private void GrantCoins(PlayerInteract.Use e)
    {
        // Send out event to player
        GiveCoins gc;
        gc.coinCount = coinCount + Random.Range(-coinCountVarience, coinCountVarience);
        FFMessageBoard<GiveCoins>.SendAllConnected(gc, e.actor.gameObject);
    }
}
