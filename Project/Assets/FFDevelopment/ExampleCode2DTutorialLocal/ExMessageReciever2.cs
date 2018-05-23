using UnityEngine;
using System.Collections;

public class ExMessageReciever2 : MonoBehaviour {

    // Use this for initialization
    void Start()
    {
        // FFMessage is global which means that it is not sensative
        // to where it is called from
        FFMessage<PlayerDiedEvent>.Connect(OnPlayerDiedEvent);

    }

    // Example of a 1-off event
    int OnPlayerDiedEvent(PlayerDiedEvent e)
    {
        // It is ok to disconnect/connect/send events from within
        // an event call. However, added events within the same
        // event type will not be called on the event they are added.
        FFMessage<PlayerDiedEvent>.Disconnect(OnPlayerDiedEvent);
        Debug.Log("The Player died!, I am quite sad. I am going to be quiet now...  (One Time Event example)");

        return 0;
    }


    void OnDestroy()
    {
        // remember to disconnect or else a null exception might happen
        // if a listener who has been destroyed is called. Disconnecting
        // a function which has already been disconnected previously is fine.
        FFMessage<PlayerDiedEvent>.Disconnect(OnPlayerDiedEvent);
    }
}
