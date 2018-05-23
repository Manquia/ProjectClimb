using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct GiveObject
{
    public string ObjectName;
    public int objectCount;
}


public class GrantItemOnUse : MonoBehaviour {

    public bool singleUse = true;
    bool used = false;

    public string ObjectName;
    public int objectCount = 1;
    public int objectCountVarience = 0;

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

        GrantObject(e);
        return 1;
    }



    private void GrantObject(PlayerInteract.Use e)
    {
        // Send out event to player
        GiveObject giveObject;
        giveObject.ObjectName = ObjectName;
        giveObject.objectCount = objectCount + Random.Range(-objectCountVarience, objectCountVarience);
        FFMessageBoard<GiveObject>.SendAllConnected(giveObject, e.actor.gameObject);
    }
}
