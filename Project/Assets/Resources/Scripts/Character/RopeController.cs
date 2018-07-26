using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct RopeDestroy
{
    public RopeController controller;
}

public class RopeController : MonoBehaviour
{
    public struct ExternalGraphicsUpdate
    {
        public RopeController rope;
        public float dt;
    }
    public struct ExternalPhysicsUpdate
    {
        public RopeController rope;
        public float dt;
    }
    // @PERF @SPEED Make this not require dynamicPath. Lots of IK samples so
    // if this is slow that is low hanging fruit...

    private List<Transform> visualElements = new List<Transform>();
    Transform visualElementRopeEndTop;
    Transform visualElementRopeEndBot;
    private List<Transform> collisionElements = new List<Transform>();

    private FFPath path; // rope is exactly 2 points
    internal GrappleController grapple;

    public FFPath GetPath() { return path; }

    public float length = 25.0f;
    public float mass = 1.5f;
    public float springForce = 45.0f;

    public Vector3 velocity = Vector3.zero;
    public float speed { get { return velocity.magnitude; } }
    public float friction = 0.05f;

    public float ropeRotation = 0.0f;
    public float distBetweenRopeVisuals = 0.1f;
    public bool isStatic = true;



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

    public Vector3 RopeVecNorm()
    {
        var ropeVec = path.PositionAtPoint(1) - path.PositionAtPoint(0);
        return Vector3.Normalize(ropeVec);
    }

    public GameObject visualBetweensPrefab;
    public GameObject visualEndsPrefab;
    public GameObject visualTopPrefab;
    public GameObject collisionPrefab;

    Vector3 ropeStartPos;
    Vector3 ropeStartVel;
    bool ropeStateIsStatic;

    // Use this for initialization
    void Awake()
    {   
        path = GetComponent<FFPath>();
        grapple = GetComponent<GrappleController>();
        path.DynamicPath = false;
        Debug.Assert(path.points.Length > 1, "Path should alwasy have atleast 2 points");

        MakeEnds();


        // set langth to the current point distance
        if (length < 0.0f)
        {
            path.SetupPointData();
            length = path.PathLength;
        }

        FFMessageBoard<PlayerInteract.Use>.Connect(OnUse, gameObject);

        if(GetComponent<GrappleController>() == null)
        {
            ropeStartPos = path.points[1];
            ropeStartVel = velocity;
            ropeStateIsStatic = isStatic;
        }
        FFMessage<ResetLevel>.Connect(OnResetlevel);
    }

    private int OnResetlevel(ResetLevel e)
    {

        if (GetComponent<GrappleController>() == null)
        {
            ResetRope();
        }
        else
        {
            Destroy(gameObject);
        }
        return 0;
    }
    public void ResetRope()
    {
        Debug.Assert(GetComponent<GrappleController>() == null, "Cannot reset a grappleing hook, this should be destroyed instaed");

        path.points[1] = ropeStartPos;
        velocity       = ropeStartVel; 
        isStatic       = ropeStateIsStatic;

    }

    private void OnDestroy()
    {
        FFMessageBoard<PlayerInteract.Use>.Disconnect(OnUse, gameObject);
        FFMessage<ResetLevel>.Disconnect(OnResetlevel);

        RopeDestroy rd;
        rd.controller = this;
        FFMessageBoard<RopeDestroy>.Send(rd, gameObject);
    }

    private int OnUse(PlayerInteract.Use e)
    {
        // make sure its a player
        var player = e.actor.GetComponent<Player>();

        Debug.Assert(player != null, "PlayerInteract.Use refered an object which wasn't a player on a RopeController!");

        player.SetupOnRope(this, player.OnRope.transitionTypeGrabRope);

        return 1;
    }

    void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;

        if (isStatic == false)
            UpdateRopeMovement(dt);
        path.SetupPointData(); // calculate updated path data


        Debug.DrawLine(path.PositionAtPoint(1), path.PositionAtPoint(1) + velocity * 2.0f, Color.red);

        SendExternalPhysicsEvent(dt);
    }

    private void Update()
    {
        float dt = Time.deltaTime;


        SendUpdateExternalGraphicsEvent(dt);
        UpdateRopeVisuals();
        UpdateRopeCollision();

        if (grapple != null)
            grapple.GrappleUpdatePath(dt);
    }

    // returns the segmentVec in Local Coordinates to the rope
    public Vector3 SegmentVec(float distOnPath)
    {
        int segmentIndex = 0;
        path.PrevPoint(distOnPath, out segmentIndex);
        Vector3 prevSegmentPoint = path.points[segmentIndex];
        Vector3 nextSegemntPoint = path.points[segmentIndex + 1];
        return nextSegemntPoint - prevSegmentPoint;
    }


    public void UpdateRopeMovement(float dt)
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


    }


    void UpdateRopeVisuals()
    {
        var ropeVec = path.PositionAtPoint(1) - path.PositionAtPoint(0);
        var ropeVecNorm = Vector3.Normalize(ropeVec);

        // Calculate the first rotation relative to an absolute down
        var angularRotationOnRope = Quaternion.AngleAxis(ropeRotation, ropeVecNorm) * Quaternion.FromToRotation(Vector3.down, ropeVecNorm);

        if (visualElementRopeEndTop != null)
        {
            visualElementRopeEndTop.position = path.PositionAtPoint(0);
            var staticRotation = Quaternion.FromToRotation(Vector3.up, Vector3.Normalize(-ropeStartPos));
            //visualElementRopeEndTop.rotation = angularRotationOnRope;
            visualElementRopeEndTop.rotation = staticRotation;
        }

        // Draw visuals along rope
        int indexElement = 0;
        int segmentIndex = 0;
        for(float distAlongPath = distBetweenRopeVisuals * 0.5f; distAlongPath <= path.PathLength; distAlongPath += distBetweenRopeVisuals, ++indexElement)
        {
            // move to next segment for rotation values
            if(distAlongPath > path.linearDistanceAlongPath[segmentIndex + 1])
            {
                ++segmentIndex;
                Vector3 segmentDir = path.points[segmentIndex + 1] - path.points[segmentIndex];
                var AngleFromDown = Quaternion.FromToRotation(Vector3.down, segmentDir);
                angularRotationOnRope = Quaternion.AngleAxis(ropeRotation, ropeVecNorm) * AngleFromDown;
            }

            Transform element;
            if (indexElement == visualElements.Count)
                AddVisualElement();

            element = visualElements[indexElement];
            element.gameObject.SetActive(true);
            element.position = path.PointAlongPath(distAlongPath);
            //element.rotation = Quaternion.LookRotation(vecAlongEdgeOfSphere, -ropeVecNorm);
            element.rotation = angularRotationOnRope;
        }

        if (visualElementRopeEndBot != null)
        {
            visualElementRopeEndBot.position = path.PositionAtPoint(path.points.Length - 1);
            visualElementRopeEndBot.rotation = angularRotationOnRope;
        }

        for(;indexElement < visualElements.Count; ++indexElement)
        {
            visualElements[indexElement].gameObject.SetActive(false);
        }
    }

    private void UpdateRopeCollision()
    {
        // Place Collisions elements on each joint of the rope
        int indexElement = 0;
        int pointCount = path.points.Length;
        Vector3 scale = Vector3.one;
        for (indexElement = 0; indexElement < pointCount - 1; ++indexElement)
        {
            Transform element;
            if (indexElement == collisionElements.Count)
                AddCollisionElement();

            element = collisionElements[indexElement];
            element.gameObject.SetActive(true);

            Vector3 startPt = path.points[indexElement];
            Vector3 endPt = path.points[indexElement + 1];
            Vector3 vec = endPt - startPt;
            scale.y = vec.magnitude;

            element.localPosition = startPt + (vec * 0.5f);
            element.up = vec;
            element.localScale = scale;
        }

        for (; indexElement < collisionElements.Count; ++indexElement)
        {
            collisionElements[indexElement].gameObject.SetActive(false);
        }

    }

    void AddVisualElement()
    {
        var element = Instantiate(visualBetweensPrefab).transform;
        element.SetParent(transform);
        visualElements.Add(element);
    }

    void AddCollisionElement()
    {
        var element = Instantiate(collisionPrefab).transform;
        element.SetParent(transform);
        collisionElements.Add(element);
    }
    void MakeEnds()
    {
        if (visualTopPrefab != null)
        { 
            visualElementRopeEndTop = Instantiate(visualTopPrefab).transform;
            visualElementRopeEndTop.SetParent(transform);
        }

        if (visualEndsPrefab != null)
        {
            visualElementRopeEndBot = Instantiate(visualEndsPrefab).transform;
            visualElementRopeEndBot.SetParent(transform);
        }
        
    }

    void SendExternalPhysicsEvent(float dt)
    {
        ExternalPhysicsUpdate epu;
        epu.dt = dt;
        epu.rope = this;
        FFMessageBoard<ExternalPhysicsUpdate>.Send(epu, gameObject, 1000);
    }
    void SendUpdateExternalGraphicsEvent(float dt)
    {
        ExternalGraphicsUpdate egu;
        egu.rope = this;
        egu.dt = dt;
        FFMessageBoard<ExternalGraphicsUpdate>.Send(egu, gameObject, 1000); // other objects listen to use for Update
    }


}
