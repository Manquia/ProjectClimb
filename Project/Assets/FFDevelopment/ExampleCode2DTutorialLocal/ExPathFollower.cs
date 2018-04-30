using UnityEngine;
using System.Collections;

public struct PathFollowerCompletedLoopEvent
{
    public float distTraveled;
}

public class ExPathFollower : FFComponent {

    public float distance = 0.0f;
    [Range(0.1f, 20.0f)]
    public float speed = 1.0f;

    private int loopCounter = 0;

    public FFPath PathToFollow;

    void Awake()
    {
    }

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        distance += Time.fixedDeltaTime * speed;

        var path = PathToFollow.GetComponent<FFPath>();
        if(path)
        {
            var position = path.PointAlongPath(distance);
            transform.position = position;
            if((int)(distance % path.PathLength) > loopCounter)
            {
                loopCounter = (int)(distance % path.PathLength);
                PathFollowerCompletedLoopEvent e;
                e.distTraveled = path.PathLength;
                FFMessage<PathFollowerCompletedLoopEvent>.SendToLocal(e);
            }
        }
	}

}