using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteract : MonoBehaviour
{

    public struct LookingAt
    {
        public Transform actor;
        public Transform target;
    }
    public struct Looking
    {
        public Transform actor;
        public Transform target;
    }
    public struct Use
    {
        public Transform actor;
        public Transform target;
    }
    public struct LookingAway
    {
        public Transform actor;
        public Transform target;
    }
    private LookingAt      PlayerIsLookingAt;
    private Looking        playerIsLooking;
    private LookingAway    playerIsLookingAway;
    private Use            playerIsUsing;

    public LookingAt   isLookingAt   { get { return PlayerIsLookingAt;   } }
    public Looking     isLooking     { get { return playerIsLooking;     } }
    public LookingAway isLookingAway { get { return playerIsLookingAway; } }
    public Use         isUsing       { get { return playerIsUsing;       } }


    public float maxInteractDistance = 10.0f;
    public float interactionRadius = 0.5f;

    public Camera interactorCamera;

    public Transform interactedObject;
    public Transform prevInteractedObject;

	// Use this for initialization
	void Start ()
    {
	}
	
	// Update is called once per frame
	public void UpdateInteract()
    {
        RaycastHit hit;

        PlayerIsLookingAt.target = null;
        playerIsLooking.target = null;
        playerIsLookingAway.target = null;
        playerIsUsing.target = null;

        // If we hit something, send the use command
        if (LookRaycast(out hit))
        {
            // clicked on object
            if (Input.GetMouseButtonDown(0))
            {
                SendUse(hit.transform.gameObject);
            }

            // looking at new object
            if (hit.transform != interactedObject)
            {
                prevInteractedObject = interactedObject;
                interactedObject = hit.transform;

                // looking away from prev object
                if(prevInteractedObject != null)
                {
                    SendLookAway(prevInteractedObject.gameObject);
                }

                if(interactedObject != null)
                {
                    SendLookAt(interactedObject.gameObject);
                }
            }

            SendLooking(interactedObject.gameObject);
        }
        else
        {
            // looking away
            if(interactedObject != null)
            {
                SendLookAway(interactedObject.gameObject);
            }

            prevInteractedObject = interactedObject;
            interactedObject = null;
        }
    }


    void SendLookAt(GameObject go)
    {
        //Debug.Log("PlayerInteract.SendLookAt");
        PlayerIsLookingAt.actor = interactorCamera.transform;
        PlayerIsLookingAt.target = go.transform;
        FFMessageBoard<LookingAt>.SendAllConnected(PlayerIsLookingAt, go);
    }
    void SendLookAway(GameObject go)
    {
        //Debug.Log("PlayerInteract.SendLookAway");
        playerIsLookingAway.actor = interactorCamera.transform;
        playerIsLookingAway.target = go.transform;
        FFMessageBoard<LookingAway>.SendAllConnected(playerIsLookingAway, go);
    }
    void SendLooking(GameObject go)
    {
        //Debug.Log("PlayerInteract.SendLooking");
        playerIsLooking.actor = interactorCamera.transform;
        playerIsLooking.target = go.transform;
        FFMessageBoard<Looking>.SendAllConnected(playerIsLooking, go);
    }
    void SendUse(GameObject go)
    {
        //Debug.Log("PlayerInteract.SendUse");
        playerIsUsing.actor = transform;
        playerIsUsing.target = go.transform;
        FFMessageBoard<Use>.SendAllConnected(playerIsUsing, go);
    }

    bool LookRaycast(out RaycastHit hit)
    {
        return Physics.SphereCast(
            interactorCamera.transform.position,
            interactionRadius,
            interactorCamera.transform.forward,
            out hit, maxInteractDistance);
    }
}
