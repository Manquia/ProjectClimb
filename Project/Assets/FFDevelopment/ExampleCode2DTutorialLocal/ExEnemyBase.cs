using UnityEngine;
using System.Collections;

public class ExEnemyBase : FFComponent {

    [HideInInspector]
    public ExMessageBoxHub controller;
	// Use this for initialization

    // Notice: ExEnemy 1 show this design paterns, however ExEnemy2/3 uses gameobject locallity
    // which is a far better design pattern to follow. this just serves as an example of how to
    // use FFMessageBox.
    public void FindController()
    {
        Transform trans = transform;

        do // look throw all parents for ExMessageBoxHub
        {
            controller = trans.gameObject.GetComponent<ExMessageBoxHub>();
            if (controller)
            {
                break;
            }
            else
            {
                trans = trans.parent;
            }
        } while (trans);
    }
}
