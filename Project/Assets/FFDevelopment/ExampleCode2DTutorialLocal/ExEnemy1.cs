using UnityEngine;
using System.Collections;

public class ExEnemy1 : ExEnemyBase {

    private FFAction.ActionSequence seq;

    // make negative to go left
    public float AttackWidth;
    // make negative to go up
    public float AttackDepth;
    public float AttackSpeed;
    public FFEase ease;



    private bool active = false;

    // Notice: ExEnemy 1 show this design paterns, however ExEnemy2/3 uses gameobject locallity
    // which is a far better design pattern to follow. this just serves as an example of how to
    // use FFMessageBox.
    void Awake()
    {
        FindController();
        if(controller)
        {
            controller.Activator.Connect(activate);
            controller.Deactivator.Connect(deactivate);
        }
    }


    void activate(ActivateEnemiesEvent e)
    {
        if (!active)
        {
            if (seq == null) // first activate
            {
                seq = action.Sequence();
                seq.Call(Stage1);
            }
            else
                seq.Resume();

            active = true;
        }
    }

    void deactivate(DeactivateEnemiesEvent e)
    {
        if (active)
        {
            seq.Pause();
            active = false;
        }
    }

    // move down/up (depends on if AttackWidth is pos/neg) and right/left (depends on if AttackWidth is pos/neg)
    void Stage1()
    {
        Vector3 targetLocation = new Vector3(ffposition.Val.x + AttackWidth, ffposition.Val.y - AttackDepth, ffposition.Val.z);

        // down and right
        seq.Property(ffposition, targetLocation, ease, AttackSpeed / 3);
        seq.Sync();
        seq.Call(Stage2);
    }

    // Move straight right/left (depends on if AttackWidth is pos/neg)
    void Stage2()
    {
        seq.Property(ffposition, new Vector3(ffposition.Val.x - AttackWidth / 2, ffposition.Val.y, ffposition.Val.z), ease, AttackSpeed / 2);
        seq.Sync();
        seq.Call(Stage1);
    }
	
}
