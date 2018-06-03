using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrappleController : MonoBehaviour {

    internal Player mySource;
    internal FFPath myPath;
    internal bool grappleInFlight = false;


    Transform modelRoot;
    Rigidbody mybody;

    // In world coordinates
    List<Vector3> flightTrojectory = new List<Vector3>();

    public void Init(Player p, Vector3 startposition, Vector3 startVelocity)
    {
        mybody = GetComponent<Rigidbody>();
        modelRoot = transform.Find("ModelRoot");
        myPath = GetComponent<FFPath>();
        myPath.DynamicPath = false;
        mySource = p;
        grappleInFlight = true;

        // Setup position  @TODO Make this come out of the gun!!!
        transform.position = startposition;

        // Setup velocity // @TODO make this have a dot OR Prediction line for the player to see where the grapple will go....
        mybody.velocity = Vector3.zero;
        mybody.AddForce(startVelocity, ForceMode.VelocityChange);

        // allign to velocity
        AlignModelOrientation();

        // Reserve points (~3 seconds of flight)
        flightTrojectory.Capacity = 200;

        // setup points
        myPath.points = new Vector3[2];
        myPath.points[0] = Vector3.zero;
        //@TODO  @POLISH make this use the IK position/offset to make it come out of the gun: mySource.OnRope.rightHandOffset OR mySource.ikSnap.RightHandPos  ????
        myPath.points[1] = transform.InverseTransformPoint(mySource.transform.position);
    }

    private void OnCollisionEnter(Collision collision)
    {
        var contacts = collision.contacts;


        // choose the first point as our anchor
        transform.position = contacts[0].point;
        mybody.isKinematic = true;
        mybody.useGravity = false;
        grappleInFlight = false;

        // 
        // simplify the rope to 2 pts. This should happen over time eventually
        // @TODO make this to a Line of sight reduction algorithm when we get
        // multi-segment rope support @ROPE @REFACTOR @TODO @MULTI
        var newPts = new Vector3[2];
        newPts[0] = Vector3.zero;
        newPts[1] = myPath.points[myPath.points.Length - 1];
        myPath.points = newPts;
        
    }

    private void FixedUpdate()
    {
        if(grappleInFlight)
        {
            AlignModelOrientation();
            flightTrojectory.Add(transform.position);

            const int simPtsToPathPts = 6;

            int torjMaxIndex = flightTrojectory.Count - 1;
            int pathPtCount = (flightTrojectory.Count / simPtsToPathPts) + 1;

            // need to get more points in our path?
            if (myPath.points.Length < pathPtCount) // @SPEED
                myPath.points = new Vector3[pathPtCount];


            var worldToLocal = transform.worldToLocalMatrix;
            for(int i = 0; i < pathPtCount; ++i)
            {
                int torjIndex = Mathf.Clamp(torjMaxIndex - i  * simPtsToPathPts, 0, torjMaxIndex);
                myPath.points[i] = transform.InverseTransformPoint(flightTrojectory[torjIndex]);
            }
        }
        
    }


    void AlignModelOrientation()
    {
        modelRoot.up = mybody.velocity;
    }


}
