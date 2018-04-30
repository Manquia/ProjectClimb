using UnityEngine;
using System.Collections;

public class ExEnemy2 : ExEnemyBase {

    private FFAction.ActionSequence seq;

    [Range(-1,1)]
    public float PercentHorizonal;
    [Range(-1, 1)]
    public float PercentVirtical;

    [Range(0.0f,10.0f)]
    public float randomVariance;

    [Range(0.1f,2.0f)]
    public float AttackSpeed;

    private bool active = false;

    // Better design pattern than ExEnemy1 which is far more scalable/easy. Also anything
    // as in other controlers can tie into this event when needed. This allows your sound
    // /animation/etc... controllers to connect to anything it needs related to an event
    // on this gameobject
    void Awake()
    {
        FFMessageBoard<ActivateEnemiesEvent>.Connect(activate, gameObject);
        FFMessageBoard<DeactivateEnemiesEvent>.Connect(deactivate, gameObject);
    }

    void OnDestroy()
    {
        FFMessageBoard<ActivateEnemiesEvent>.Disconnect(activate, gameObject);
        FFMessageBoard<DeactivateEnemiesEvent>.Disconnect(deactivate, gameObject);
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

    // move down and right
    void Stage1()
    {
        float seedx = Random.Range(-1.0f, 1.0f);
        float seedy = Random.Range(-1.0f, 1.0f);
        
        Vector3 randVec3 = new Vector3(
            (seedx + PercentHorizonal) * randomVariance,
            (seedy + PercentVirtical) * randomVariance);

        seq.Property(ffscale, randVec3 * 20, FFEase.E_Continuous, AttackSpeed);
        seq.Property(ffposition, randVec3 + ffposition.Val, FFEase.E_Continuous, AttackSpeed);

        seq.Sync();
        seq.Call(Stage1);
    }

}
