using UnityEngine;
using System.Collections;

public class ExEnemy3 : ExEnemyBase {

    private FFAction.ActionSequence seq;
    private FFAction.ActionSequence seq1;

    public string target;
    public Sprite bulletSprite;
    
    [Range(0.1f, 2.0f)]
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
            if (seq == null && seq1 == null) // first activate
            {
                seq = action.Sequence();
                seq1 = action.Sequence();
                seq.Call(Stage1);
                seq1.Call(Report);
            }
            else
            {
                seq.Resume();
                seq1.Resume();
            }

            active = true;
        }
    }

    void deactivate(DeactivateEnemiesEvent e)
    {
        if (active)
        {
            seq.Pause();
            seq1.Pause();
            active = false;
        }
    }

    // shoot at gameobject target with a delay between attacks of
    // AtackSpeed.
    void Stage1()
    {
        var targetToShoot = GameObject.Find(target);
        if(targetToShoot != null)
        {
            ShootAt(targetToShoot);
        }

        seq.Delay(AttackSpeed);        
        seq.Sync();
        seq.Call(Stage1);
    }

    void Report()
    {
        Debug.Log("Reported!");
        seq1.Delay(1.0f);
        seq1.Sync();
        seq1.Call(Report);
    }

    void ShootAt(GameObject obj)
    {
        var myBullet = new GameObject("Bullet");
        myBullet.AddComponent<SpriteRenderer>().sprite = bulletSprite;
        var myBulletAction = myBullet.GetOrAddComponent<FFAction>();
        var myBulletSeq = myBulletAction.Sequence();
        var myBulletPos = myBullet.transform.ffposition();

        myBulletPos.Val = transform.position;
        myBulletSeq.Property(myBulletPos, obj.transform.position, FFEase.E_Continuous, 2.0f);
        myBulletSeq.Sync();
        myBulletSeq.Call(myBullet.Destroy);
    }
}
