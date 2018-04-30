using UnityEngine;
using System.Collections;

public class ExMessageReciever1 : MonoBehaviour {

    // Use this for initialization
    void Start()
    {
        // Connect to an Event, this also registers the event to
        // FFMessageSystem so that the event's behavior can be 
        // Recorded.
        FFMessage<PlayerDiedEvent>.Connect(ReportPlayerDeath);
    }

    void ReportPlayerDeath(PlayerDiedEvent e)
    {
        Debug.Log("The Player has died! He was killed by " + e.Killer
            + " Who murderded his face off with over " + e.overkillDamage
            + " damage! This news just in at " + e.timeOfDeath);
    }

    void OnDestroy()
    {
        // remember to disconnect or else a null exception might happen
        // if a listener who has been destroyed is called. Disconnecting
        // a function which has already been disconnected previously is fine.
        FFMessage<PlayerDiedEvent>.Disconnect(ReportPlayerDeath);
    }
}
