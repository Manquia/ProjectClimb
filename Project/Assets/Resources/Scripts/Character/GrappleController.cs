using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrappleController : MonoBehaviour {

    internal Player mySource;
    internal FFPath myPath;
    internal RopeController myrope;

    public bool grappleInFlight = false;


    Transform modelRoot;
    Rigidbody mybody;

    // In world coordinates
    List<Vector3> flightTrojectory = new List<Vector3>();

    public void Init(Player p, Vector3 startposition, Vector3 startVelocity)
    {
        myrope = GetComponent<RopeController>();
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

        myrope.length = (newPts[0] - newPts[1]).magnitude;
    }

    public void GrappleUpdatePath()
    {
        if(grappleInFlight)
        {
            // Destroy Rope b/c its too long!!!
            // @TODO make Sounds effects, maybe even VFX
            // for this thing. @Polish @Sound
            if(flightTrojectory.Count > 600)
            {
                Debug.Log("Destroyed Grapple/Rope b/c it was getting too long");
                Destroy(gameObject);
            }

            AlignModelOrientation();
            flightTrojectory.Add(transform.position);

            const int simPtsToPathPts = 12;

            int torjMaxIndex = flightTrojectory.Count - 1;
            int pathPtCount = (flightTrojectory.Count / simPtsToPathPts) + 1;

            // need to get more points in our path?
            if (myPath.points.Length < pathPtCount)
            {
                myPath.points = new Vector3[pathPtCount];
            }
            pathPtCount = myPath.points.Length;

            var worldToLocal = transform.worldToLocalMatrix;
            Vector3 worldPos = transform.position;
            int pathIndex = 1;
            int trojIndex = Mathf.Max(torjMaxIndex - (flightTrojectory.Count % simPtsToPathPts), (flightTrojectory.Count % simPtsToPathPts) - 1);

            // first point is always at the last position
            myPath.points[0] = flightTrojectory[torjMaxIndex] - worldPos;
            while (pathIndex < pathPtCount)
            {
                myPath.points[pathIndex] = flightTrojectory[trojIndex] - worldPos;

                ++pathIndex;
                trojIndex = Mathf.Min(trojIndex - simPtsToPathPts, torjMaxIndex);
            }
        }
        
    }


    void AlignModelOrientation()
    {
        modelRoot.up = mybody.velocity;
    }


}
