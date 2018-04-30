using UnityEngine;
using System.Collections;

public class ExReporter : MonoBehaviour {

	// Use this for initialization
	void Start () {
        FFMessage<PathFollowerCompletedLoopEvent>.Connect(LoopCompleted);
	}

    // for example of Global Type-based Messages/Events
    int TotalLoopsCompleted = 0;
    float TotalDistanceTraveled = 0.0f;

    void LoopCompleted(PathFollowerCompletedLoopEvent e)
    {
        ++TotalLoopsCompleted;
        TotalDistanceTraveled += e.distTraveled;
    }
	
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            var messages = FFPrivate.FFMessageSystem.GetStats();
            foreach(var mes in messages)
            {
                Debug.Log(mes);
            }
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log("ExPathFollowers have completed " + TotalLoopsCompleted
                + " loops around their Paths and have traveled a total of " + TotalDistanceTraveled);
        }
    }


    void OnDestroy()
    {
        // remember to disconnect or else a null exception might happen
        // if a listener who has been destroyed is called. Disconnecting
        // a function which has already been disconnected previously is fine.
        FFMessage<PathFollowerCompletedLoopEvent>.Disconnect(LoopCompleted);
    }
}
