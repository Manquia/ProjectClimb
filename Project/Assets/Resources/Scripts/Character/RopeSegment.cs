using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RopeSegment : MonoBehaviour {

    Transform GetOwner()
    {
        return transform.parent;
    }

    private void Start()
    {
        FFMessageBoard<PlayerInteract.Use>.Connect(OnUse, gameObject);
    }
    private void OnDestroy()
    {
        FFMessageBoard<PlayerInteract.Use>.Connect(OnUse, gameObject);
    }

    private int OnUse(PlayerInteract.Use e)
    {
        // forward to RopeController
        return FFMessageBoard<PlayerInteract.Use>.SendUp(e, GetOwner().gameObject);
    }
}
