using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Use to change popup
public struct ConfigurePopup
{
    public string title;
    public string text;
}


public class Popup : MonoBehaviour
{
    //public UnityEngine.UI.Text title;
    //public UnityEngine.UI.Text text;
    //public UnityEngine.UI.Image background;
    public Transform popupRoot;
    public bool singleUse;
    bool used = false;
    bool needsKey = false;

	// Use this for initialization
	void Start ()
    {
        FFMessageBoard<ConfigurePopup>.Connect(OnConfiurePopup, gameObject);
        FFMessageBoard<PlayerInteract.LookingAt>.Connect(OnLookAt, gameObject);
        FFMessageBoard<PlayerInteract.LookingAway>.Connect(OnLookAway, gameObject);
        FFMessageBoard<PlayerInteract.Looking>.Connect(OnLooking, gameObject);
        FFMessageBoard<PlayerInteract.Use>.Connect(OnUse, gameObject);

    }
    private void OnDestroy()
    {
        FFMessageBoard<ConfigurePopup>.Disconnect(OnConfiurePopup, gameObject);
        FFMessageBoard<PlayerInteract.LookingAt>.Disconnect(OnLookAt, gameObject);
        FFMessageBoard<PlayerInteract.LookingAway>.Disconnect(OnLookAway, gameObject);
        FFMessageBoard<PlayerInteract.Looking>.Disconnect(OnLooking, gameObject);
        FFMessageBoard<PlayerInteract.Use>.Disconnect(OnUse, gameObject);
    }

    private int OnUse(PlayerInteract.Use e)
    {
        if (singleUse && used)
            return 0;

        if (singleUse)
            used = true;

        return 1;
    }

    private int OnLookAt(PlayerInteract.LookingAt e)
    {
        if (used)
        {
            popupRoot.gameObject.SetActive(false);
        }

        if (needsKey && e.actor.GetComponent<Inventory>().hasKey == false)
        {
            popupRoot.gameObject.SetActive(false);
            return 0;
        }


        popupRoot.gameObject.SetActive(true);
        return 0;
    }
    private int OnLookAway(PlayerInteract.LookingAway e)
    {
        if (used)
        {
            popupRoot.gameObject.SetActive(false);
        }

        popupRoot.gameObject.SetActive(false);
        return 0;
    }
    private int OnLooking(PlayerInteract.Looking e)
    {
        if (used)
        {
            popupRoot.gameObject.SetActive(false);
        }

        // look at camera
        var vecToPlayerCamera = e.actor.position - popupRoot.position;
        transform.forward = -vecToPlayerCamera;
        return 0;
    }



    private int OnConfiurePopup(ConfigurePopup e)
    {
        return 0;
    }

    public void Display()
    {

    }
}
