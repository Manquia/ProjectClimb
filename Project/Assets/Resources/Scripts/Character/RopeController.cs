using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct RopeControllerUpdate
{
    public RopeController controller;
}

public class RopeController : MonoBehaviour
{
    // @PERF @SPEED Make this not require dynamicPath. Lots of IK samples so
    // if this is slow that is low hanging fruit...

    private FFPath path; // rope is exactly 2 points
    public FFPath GetPath() { return path; }

    public float length = 25.0f;
    public float mass = 1.5f;
    public float springForce = 45.0f;

    public Vector3 velocity = Vector3.zero;
    public float speed { get { return velocity.magnitude; } }
    public float friction = 0.05f;

    public float ropeRotation = 0.0f;

    public Vector3 VelocityAtDistUpRope(float distUpRope)
    {
        var pathLength = path.PathLength;
        var distOnPath = Mathf.Clamp(pathLength - distUpRope, 0.0f, pathLength);

        return velocity * (distOnPath / pathLength);
    }

    public Quaternion RopeRotation()
    {
        var ropeVec = path.PositionAtPoint(1) - path.PositionAtPoint(0);
        var ropeVecNorm = Vector3.Normalize(ropeVec);
        var down = Vector3.Normalize(Physics.gravity);
        var rightVec = -Vector3.Cross(ropeVecNorm, down);
        var vecAlongEdgeOfSphere = Vector3.Normalize(Vector3.Cross(ropeVecNorm, rightVec));

        return Quaternion.LookRotation(vecAlongEdgeOfSphere, -ropeVecNorm);
    }

    // Use this for initialization
    void Start()
    {
        path = GetComponent<FFPath>();
        Debug.Assert(path.points.Length == 2);

        FFMessageBoard<PlayerInteract.Use>.Connect(OnUse, gameObject);
    }
    private void OnDestroy()
    {
        FFMessageBoard<PlayerInteract.Use>.Disconnect(OnUse, gameObject);
    }

    private int OnUse(PlayerInteract.Use e)
    {
        // make sure its a player
        var player = e.actor.GetComponent<Player>();

        Debug.Assert(player != null, "PlayerInteract.Use refered an object which wasn't a player on a RopeController!");

        player.SetupOnRope(this);

        return 1;
    }

    void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;
        UpdateRopeMovement(dt);
        UpdateRopeVisuals();

        SendUpdateEvent();
    }
    
    public Vector3 RopeVecNorm()
    {
        var ropeVec = path.PositionAtPoint(1) - path.PositionAtPoint(0);
        return Vector3.Normalize(ropeVec);
    }

    void UpdateRopeMovement(float dt)
    {
        var epsilon = 0.005f;
        var ropeVec = path.PositionAtPoint(1) - path.PositionAtPoint(0);
        var ropeVecNorm = Vector3.Normalize(ropeVec);

        var down = Vector3.Normalize(Physics.gravity);


        // @ TODO @MAYBE Switch to useing this over vecAlongEdgeOfSphere?
        //var AngleFromDown = Quaternion.FromToRotation(Vector3.down, ropeVecNorm);
        //var angularRotationOnRope = Quaternion.AngleAxis(ropeRotation, ropeVecNorm) * AngleFromDown;

        if (ropeVec.magnitude + epsilon >= length &&
            Vector3.Dot(ropeVec, down) > 0.0f) //  rope is tight (Orbiting)
        {
            var rightVec = -Vector3.Cross(ropeVecNorm, down);
            var vecAlongEdgeOfSphere = Vector3.Normalize(Vector3.Cross(ropeVecNorm, rightVec));

            var percentAlongLine = Vector3.Dot(vecAlongEdgeOfSphere, down); // may be off!!!

            Debug.DrawLine(path.PositionAtPoint(1), path.PositionAtPoint(1) + vecAlongEdgeOfSphere, Color.green);

            // cast velocity along circumfrance of sphere
            velocity += dt * percentAlongLine * Physics.gravity.magnitude * vecAlongEdgeOfSphere;
        }
        else // rope is slack (falling straight down)
        {
            velocity += dt * Physics.gravity;
        }
        
        // Apply springy nature of rope
        if(ropeVec.magnitude >= length)
        {
            var deltaDist = ropeVec.magnitude - length;

            // remove all velocity not inline with currenct direction
            velocity = Vector3.ProjectOnPlane(velocity, Vector3.Normalize(ropeVecNorm));

            // Apply spring force
            velocity += deltaDist * (springForce / 100.0f) * -ropeVecNorm;

            // Set Rope Position to match radius
            path.points[1] = Vector3.Normalize(ropeVecNorm) * length;
        }

        // apply friction
        velocity = Vector3.Lerp(velocity, Vector3.zero, friction * dt);

        // Apply velocity
        path.points[1] += dt * velocity;

        // make sure rope is still within bounds
        ropeVec = path.points[1] - path.points[0];
        ropeVecNorm = Vector3.Normalize(ropeVec);
        if (ropeVec.magnitude >= length)
        {
            // Set Rope Position to match radius when its beyond the point
            path.points[1] = Vector3.Normalize(ropeVecNorm) * length;
        }


        Debug.DrawLine(path.PositionAtPoint(1), path.PositionAtPoint(1) + velocity, Color.red);
    }

    public float distBetweenRopeVisuals = 0.1f;
    private List<Transform> visualElements = new List<Transform>();

    void UpdateRopeVisuals()
    {
        Debug.Assert(path.DynamicPath, "Path must be set to dynamic");
        //Debug.Log("UpdateRopeVisuals PathLength: " + path.PathLength);

        var ropeVec = path.PositionAtPoint(1) - path.PositionAtPoint(0);
        var ropeVecNorm = Vector3.Normalize(ropeVec);

        //var down = Vector3.Normalize(Physics.gravity);
        //var rightVec = -Vector3.Cross(ropeVecNorm, down);
        //var vecAlongEdgeOfSphere = Vector3.Normalize(Vector3.Cross(ropeVecNorm, rightVec));

        var AngleFromDown = Quaternion.FromToRotation(Vector3.down, ropeVecNorm);
        var angularRotationOnRope = Quaternion.AngleAxis(ropeRotation, ropeVecNorm) * AngleFromDown;

        // Draw visuals along rope
        int indexElement = 0;
        for(float distAlongPath = 0; distAlongPath <= path.PathLength; distAlongPath += distBetweenRopeVisuals, ++indexElement)
        {
            Transform element;
            if (indexElement == visualElements.Count)
                AddVisualElement();

            element = visualElements[indexElement];
            element.gameObject.SetActive(true);
            element.position = path.PointAlongPath(distAlongPath);
            //element.rotation = Quaternion.LookRotation(vecAlongEdgeOfSphere, -ropeVecNorm);
            element.rotation = angularRotationOnRope;
        }

        for(;indexElement < visualElements.Count; ++indexElement)
        {
            visualElements[indexElement].gameObject.SetActive(false);
        }
    }

    void AddVisualElement()
    {
        var element = Instantiate(FFResource.Load_Prefab("RopeSegment")).transform;
        element.SetParent(transform);
        visualElements.Add(element);
    }

    void SendUpdateEvent()
    {
        RopeControllerUpdate rc;
        rc.controller = this;
        FFMessageBoard<RopeControllerUpdate>.Send(rc, gameObject, 1000); // other objects listen to use for Update
    }


}
