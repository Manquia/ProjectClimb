using UnityEngine;
using System.Collections;

// Example to Activate/Deactivate using a FFMessageBox

// Press 'A' to Activate, 'D' to Deactivate.

public struct ActivateEnemiesEvent
{
    public float timeTakenToActivate;
}

public struct DeactivateEnemiesEvent
{
    public float timeTakenToDeactivate;
}

public class ExMessageBoxHub : MonoBehaviour {

    // NOTICE: using FFMessageBoard<type>.SendToLocalDown(type message, GameObject gameobject) is
    // a far better solution than using this design pattern. However I wanted to show an example
    // of how to use these in a script versus the FFMessageBoard. ExEnemy1/2 use this while ExEnemy3
    // uses the far superior FFMessageBoard<type>.SendToLocalDown.



    // for ExEnemies 1/2
    public FFMessageBox<ActivateEnemiesEvent> Activator = new FFMessageBox<ActivateEnemiesEvent>();
    public FFMessageBox<DeactivateEnemiesEvent> Deactivator = new FFMessageBox<DeactivateEnemiesEvent>();

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.A))  // Activator Triggered
        {

            // for ExEnemies 1/2
            ActivateEnemiesEvent e;
            e.timeTakenToActivate = Time.realtimeSinceStartup;
            Activator.SendToLocal(e);

            // for ExEnemies 3
            FFMessageBoard<ActivateEnemiesEvent>.SendToLocalDown(e, gameObject);
        }

        if (Input.GetKeyDown(KeyCode.D))  // Deactivator Triggered
        {

            // for ExEnemies 1/2
            DeactivateEnemiesEvent e;
            e.timeTakenToDeactivate = Time.realtimeSinceStartup;
            Deactivator.SendToLocal(e);

            // for ExEnemies 3
            FFMessageBoard<DeactivateEnemiesEvent>.SendToLocalDown(e, gameObject);
        }
	
	}
}
