using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : FFComponent
{
    // TODO
    // [f] Snap to ground over small ledges
    // [b] Jump seems to be triggering mulitiple times
    // [f] sound on jumping
    // [f] sound on walking
    // [f] sound on landing

    public CameraController cameraController;
    public DynamicAudioPlayer dynAudioPlayer;
    public IK_Snap ikSnap;

    public SpriteRenderer fadeScreenMaskSprite;
    public UnityEngine.UI.Image fadeScreenMaskImage;
    public float fadeTime = 1.5f;
    FFAction.ActionSequence fadeScreenSeq;

    internal Rigidbody myBody;
    internal CapsuleCollider myCol; 

    private FFRef<Vector3> velocityRef;
    private FFRef<Vector3> GetVelocityRef()
    {
        return velocityRef;
    }
    void SetVelocityRef(FFRef<Vector3> velocityRef)
    {
        Debug.Assert(velocityRef != null);
        this.velocityRef = velocityRef;

    }

    public enum Mode
    {
        Frozen = 0,
        Movement = 1,
        Rope = 2,
        Climb = 3,
    }

    public Annal<Mode> mode = new Annal<Mode>(16, Mode.Frozen);

    [System.Serializable]
    public class InputState
    {
        public Vector3 moveDir;
        public Vector3 moveDirRel;
        public bool modifier;
        public bool up;
        public bool down;
        public bool left;
        public bool right;

        // @TODO make this into key states instead of bools
        public Annal<bool> space = new Annal<bool>(15, false);
        public Annal<bool> spaceHeld = new Annal<bool>(15, false);
    }
    public InputState input;

    [System.Serializable]
    public class RopeConnection
    {
        public RopeController rope;

        public float pumpSpeed = 0.003f;
        public float pumpAcceleration = 0.5f;
        public float pumpResetSpeed = 0.05f;

        public float climbSpeed = 0.5f;
        public float rotateSpeed = 20.0f;
        public float leanSpeed = 0.9f;
        
        public float distUpRope;
        public float distPumpUp = 0.0f;
        
        public float maxPumpUpDist = 0.1f;
        public float maxPumpDownDist = 0.0f;

        // Offset Along Rope (vertical along rope)
        public float leftHandOffsetOnRope;
        public float rightHandOffsetOnRope;
        public float leftFootOffsetOnRope;
        public float rightFootOffsetOnRope;

        // Left/Right posiition OFfset
        public Vector3 leftHandOffset;
        public Vector3 rightHandOffset;
        public Vector3 leftFootOffset;
        public Vector3 rightFootOffset;

        // Rotations
        public Vector3 leftHandRot;
        public Vector3 rightHandRot;
        public Vector3 leftFootRot;
        public Vector3 rightFootRot;
        
        // Functional stuff
        public float angleOnRope; // in degrees
        public float distFromRope;
        public float rotationYaw;
        public float rotationPitch;
        
        // @TODO @Polish
        public float onRopeAngularVelocity; // <--- Rename...
        
    }
    public RopeConnection OnRope;

    [System.Serializable]
    public class Movement
    {
        public float maxSpeed = 2.5f;
        public float runMultiplier = 2.0f;

        public float jumpForce = 1000.0f;
        public float maxSlopeAngle = 55.0f;
        public float groundTouchHeight = 0.2f;
        public class Details
        {
            public Annal<bool> groundTouches = new Annal<bool>(16, false);
            public Annal<bool> jumping = new Annal<bool>(16, false);
        }
        public Details details = new Details();
        public float climbRadius = 0.45f;
        public float revJumpVel = 1.0f;
        internal Vector3 up;
    }
    public Movement movement;
    [System.Serializable]
    public class MovementData
    {
        public AnimationCurve moveForce;
        public float redirectForce = 3.25f;
        public float maxSpeed = 2.5f;
        public float friction = 0.1f;
    }
    public MovementData OnGroundData;
    public MovementData OnAirData;

    bool grounded
    {
        // if touched ground this frame by any touches
        get
        {
            return movement.details.groundTouches.Contains((v) => v);
        }
    }
    bool jumping
    {
        get { return movement.details.jumping; }
    }

    // @TODO make this into an annal


    //bool groundedThisFrame = false;

    // Use this for initialization
    private void Awake()
    {
        myBody = GetComponent<Rigidbody>();
        myCol = GetComponent<CapsuleCollider>();
        cameraController.SetupCameraController(this);


        // start in movement state
        SwitchMode(Mode.Movement);
    }
    void Start ()
    {

        // Set referece to 0 for initialization
        SetVelocityRef(new FFVar<Vector3>(Vector3.zero));
        if (dynAudioPlayer != null)
        {
            dynAudioPlayer.SetDynamicValue(new FFRef<float>(
                    () => GetVelocityRef().Getter().magnitude,
                    (v) => { }));
        }

        // Fade Screen
        {
            // init
            //fadeScreenSeq = action.Sequence();
            //fadeScreenSeq.affectedByTimeScale = false;
            //Seq_FadeOutScreenMasks();
        }
    }
    void Seq_FadeOutScreenMasks()
    {
        fadeScreenMaskSprite.gameObject.SetActive(true);
        fadeScreenMaskImage.gameObject.SetActive(true);
        
        fadeScreenSeq.Property(new FFRef<Color>(() => fadeScreenMaskSprite.color, (v) => fadeScreenMaskSprite.color = v), fadeScreenMaskSprite.color.MakeClear(), FFEase.E_Continuous, fadeTime);
        fadeScreenSeq.Property(new FFRef<Color>(() => fadeScreenMaskImage.color, (v) => fadeScreenMaskImage.color = v), fadeScreenMaskImage.color.MakeClear(), FFEase.E_Continuous, fadeTime);
        Seq_DisableScreenMasks();
    }
    void Seq_FadeScreenMasksToColor(Color color)
    {
        fadeScreenMaskSprite.gameObject.SetActive(true);
        fadeScreenMaskImage.gameObject.SetActive(true);

        fadeScreenSeq.Property(new FFRef<Color>(() => fadeScreenMaskSprite.color, (v) => fadeScreenMaskSprite.color = v), color, FFEase.E_Continuous, fadeTime);
        fadeScreenSeq.Property(new FFRef<Color>(() => fadeScreenMaskImage.color, (v) => fadeScreenMaskImage.color = v), color, FFEase.E_Continuous, fadeTime);

        fadeScreenSeq.Sync();
    }
    void Seq_DisableScreenMasks()
    {
        fadeScreenSeq.Sync();
        fadeScreenSeq.Call(DisableGameObject, fadeScreenMaskSprite.gameObject);
        fadeScreenSeq.Call(DisableGameObject, fadeScreenMaskImage.gameObject);
    }
    void Seq_FadeInScreenMasks()
    {
        fadeScreenMaskSprite.gameObject.SetActive(true);
        fadeScreenMaskImage.gameObject.SetActive(true);
        
        fadeScreenSeq.Property(new FFRef<Color>(() => fadeScreenMaskSprite.color, (v) => fadeScreenMaskSprite.color = v), fadeScreenMaskSprite.color.MakeOpaque(), FFEase.E_Continuous, fadeTime);
        fadeScreenSeq.Property(new FFRef<Color>(() => fadeScreenMaskImage.color, (v) => fadeScreenMaskImage.color = v), fadeScreenMaskImage.color.MakeOpaque(), FFEase.E_Continuous, fadeTime);
        
        fadeScreenSeq.Sync();
    }

    // @Cleanup, @Move?
    public void LoadMainMenu()
    {
        Seq_FadeInScreenMasks();
        fadeScreenSeq.Call(LoadLevelOfName, "Menu");
    }
    public void LoadVictoryScreen(float delay)
    {
        fadeScreenSeq.Delay(delay);
        fadeScreenSeq.Sync();
        Seq_FadeScreenMasksToColor(Color.white);
        fadeScreenSeq.Call(LoadLevelOfName, "VictoryScreen");
    }
    void OnDestroy()
    {
        if (OnRope != null)
            DestroyOnRope();
    }


    #region Collisions
    private void OnCollisionEnter(Collision collision)
    {
    }
    private void OnCollisionStay(Collision collision)
    {
    }
    private void OnCollisionExit(Collision collision)
    {
    }
    #endregion Collisions

    #region Update
    // Called right before physics, use this for dynamic Player actions
    void FixedUpdate()
    {
        switch (mode.Recall(0))
        {
            case Mode.Frozen:
                // DO NOTHING
                break;
            case Mode.Movement:
                UpdateCameraTurn();

                if (grounded)
                {
                    UpdateMove(dt, OnGroundData);
                    CheckJump();
                }
                else
                {
                    UpdateMove(dt, OnAirData);
                    UpdateAir();
                }

                movement.details.groundTouches.Wash(false);
                break;
            case Mode.Rope:
                break;
            case Mode.Climb:
                break;
            default:
                break;
        }

    }


    // Called right after physics, before rendering. Use for Kinimatic player actions
    void Update()
    {
        UpdateInput();


        switch (mode.Recall(0))
        {
            case Mode.Frozen:
                // DO NOTHING
                break;
            case Mode.Movement:
                UpdateGroundRaycast();
                UpdateJumpState();
                break;
            case Mode.Rope:
                float dt = Time.deltaTime;
                UpdateRopeActions(dt);
                break;
            case Mode.Climb:
                break;
            default:
                break;
        }

    }

    private void UpdateJumpState()
    {
        // grounded && !jumping && spacePressed -> jumping = true
        // grounded && jumping -> jumping = false
        
        if (grounded && movement.details.jumping)
            movement.details.jumping.Record(false);
    }

    void UpdateGroundRaycast()
    {
        var mask = LayerMask.GetMask("Solid");
        
        GroundRaycastPattern(mask);
    }
    
    

    float dt;
    void UpdateInput()
    {
        input.moveDir.x = 0.0f;//Input.GetAxis("Horizontal");
        input.moveDir.y = 0.0f;
        input.moveDir.z = 0.0f;//Input.GetAxis("Vertical");

        input.up = Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W);
        input.down = Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S);
        input.left = Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A);
        input.right = Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D);

        input.moveDir.x += input.right ?  1.0f : 0.0f;
        input.moveDir.x += input.left  ? -1.0f : 0.0f;
        input.moveDir.z += input.up    ?  1.0f : 0.0f;
        input.moveDir.z += input.down  ? -1.0f : 0.0f;

        input.moveDirRel = transform.rotation * input.moveDir;

        input.space.Record(Input.GetKeyDown(KeyCode.Space));
        input.spaceHeld.Record(Input.GetKey(KeyCode.Space));

        input.modifier = Input.GetKey(KeyCode.LeftShift);

        dt = Time.deltaTime;
    }

    private void UpdateCameraTurn()
    {
        // Rotate based on mouse look
        if (cameraController.lookVec.x != 0)
        {
            float turnAmount = cameraController.lookVec.x;
            var rotation = Quaternion.AngleAxis(turnAmount, Vector3.up);
            transform.localRotation = transform.localRotation * rotation;
        }
    }
    void CheckJump()
    {
        var rotOfPlane = Quaternion.FromToRotation(movement.up, Vector3.up);
        var revRotOfPlane = Quaternion.FromToRotation(Vector3.up, movement.up);

        // movement in the Y axis (jump), Grounded && space in the last few frames?
        if (grounded && input.space.Contains((v) => v))
        {
            SnapToGround(maxDistToFloor);
            movement.details.jumping.Record(true);
            movement.details.groundTouches.Wash(false);


            var relVel = rotOfPlane * myBody.velocity;
            var relVelXY = new Vector3(relVel.x, 0.0f, relVel.z);
            myBody.velocity = revRotOfPlane * relVelXY;

            myBody.AddForce(transform.up * movement.jumpForce);
        }
    }
    private void UpdateAir()
    {
        // cancel/reverse jump force
        if(!input.spaceHeld && myBody.velocity.y > 0.0f)
        {
            Vector3 revJumpVel = movement.revJumpVel * myBody.velocity.y * -Vector3.up;
            myBody.velocity = myBody.velocity + revJumpVel;
        }
    }
    void UpdateMove(float dt, MovementData moveData)
    {
        var rotOfPlane = Quaternion.FromToRotation(movement.up, Vector3.up);
        var revRotOfPlane = Quaternion.FromToRotation(Vector3.up, movement.up);

        var maxSpeed = moveData.maxSpeed;
        if (input.modifier)
        {
            maxSpeed *= movement.runMultiplier;
        }


        // apply friction
        {
            myBody.velocity = myBody.velocity - (myBody.velocity * moveData.friction * dt);
        }

        // Sample moveForce
        float moveForce;
        {
            var relVel = rotOfPlane * myBody.velocity;
            var relVelXY = new Vector3(relVel.x, 0.0f, relVel.z);
            float horizontalSpeed = relVelXY.magnitude;
            float scalar = movement.maxSpeed / 9.0f;

            float mu = horizontalSpeed / (horizontalSpeed + scalar);

            moveForce = moveData.moveForce.Evaluate(mu);
        }
        // apply move force to rigid body
        float redirectForce = CalcRedirectForceMultiplier(moveData.redirectForce);
        ApplyMoveForce(moveForce * redirectForce, dt, movement.up);
        
        // Clamp Velocity along horizontal plane
        {
            const float slowingSpeed = 10.0f;
            var relVel = rotOfPlane * myBody.velocity;
            var relVelXY = new Vector3(relVel.x, 0.0f, relVel.z);
            var relVelXYNorm = relVelXY.normalized;
            float horizontalSpeed = relVelXY.magnitude;

            float newSpeed = Mathf.Lerp(horizontalSpeed, maxSpeed, slowingSpeed * dt);
            if(horizontalSpeed > maxSpeed)
            {
                Vector3 newVel = new Vector3(relVelXYNorm.x * newSpeed, relVel.y, relVelXYNorm.z * newSpeed);
                myBody.velocity = revRotOfPlane * newVel;
            }
        }

    }

    // @TODO make this work with Vec2 for directional move input
    void UpdateRopeActions(float dt)
    {
        float pumpAmount = OnRope.pumpSpeed * dt;
        float climbAmount = OnRope.climbSpeed * dt;
        float rotateAmount = OnRope.rotateSpeed * dt;
        float leanAmount = OnRope.leanSpeed * dt;

        Vector3 leanVec = Vector3.zero;
        float climbVec = 0.0f;

        if (input.space)
        {
            RopePump(pumpAmount);
        }

        bool flipClimbMod = false;
        // going up
        if (input.up)
        {
            if (input.modifier == flipClimbMod)
                leanVec += new Vector3(0.0f, 0.0f, 1.0f);
            else
                climbVec += 1.0f;
        }
        // going down
        if (input.down)
        {
            if (input.modifier == flipClimbMod)
                leanVec += new Vector3(0.0f, 0.0f, -1.0f);
            else
                climbVec += -1.0f;
        }

        bool flipRotateMod = false;
        // going right
        if (input.right && !input.left)
        {
            if (input.modifier == flipRotateMod)
                leanVec += new Vector3(1.0f, 0.0f, 0.0f);
        }
        // going left
        if (input.left && !input.right)
        {
            if (input.modifier == flipRotateMod)
                leanVec += new Vector3(-1.0f, 0.0f, 0.0f);
        }

        // Pump
        if (input.space)
        {
            RopePump(pumpAmount);
        }
        else
        {
            var vecToRestingPump = Mathf.Clamp(
                -OnRope.distPumpUp,
                -OnRope.pumpResetSpeed * dt,
                    OnRope.pumpResetSpeed * dt);

            RopePump(vecToRestingPump);
        }

        if (leanVec != Vector3.zero)
            RopeLean(Vector3.Normalize(leanVec) * leanAmount);

        RopeClimb(climbVec * climbAmount);

        // Rotate based on mouse look
        {
            float lookVecX = cameraController.lookVec.x;
            float sensitivityRotate = Mathf.Abs(cameraController.cameraTurn / cameraController.maxTurnAngle);
            sensitivityRotate = sensitivityRotate * sensitivityRotate;

            float turnAmount = lookVecX * sensitivityRotate;
            
            RopeRotateOn(-turnAmount * OnRope.rotateSpeed * dt);
        }

    }
    private int OnRopeControllerUpdate(RopeControllerUpdate e)
    {
        Debug.Assert(OnRope != null, "UpdateRope is being called when OnRope is null");

        var rope = OnRope.rope;
        var ropePath = rope.GetPath();
        var ropeLength = ropePath.PathLength;
        var ropeVecNorm = rope.RopeVecNorm();

        var distOnPath = Mathf.Clamp(ropeLength - (OnRope.distUpRope), 0.0f, ropeLength);
        //var velocity = rope.VelocityAtLength(OnRope.distUpRope);

        // update Character Position
        var AngleFromDown = Quaternion.FromToRotation(Vector3.down, ropeVecNorm);
        var angularRotationOnRope = Quaternion.AngleAxis(OnRope.angleOnRope, ropeVecNorm) * AngleFromDown;
        var positionOnRope = ropePath.PointAlongPath(distOnPath);

        // @ TODO: Add charater offset!
        transform.position = positionOnRope +                                   // Position on rope
            (angularRotationOnRope * -Vector3.forward * OnRope.distFromRope) +  // set offset out from rope based on rotation
            (ropeVecNorm * -OnRope.distPumpUp);                                  // vertical offset from pumping

        var vecForward = positionOnRope - transform.position;


        //Debug.DrawLine(positionOnRope, transform.position, Color.yellow);
        var forwardRot = Quaternion.LookRotation(vecForward, -ropeVecNorm);
        transform.rotation = forwardRot;
        var characterRot = forwardRot * Quaternion.AngleAxis(OnRope.rotationPitch, transform.right) * Quaternion.AngleAxis(OnRope.rotationYaw, transform.forward);
        transform.rotation = characterRot;

        // update Snapping IK
        if (ikSnap != null) // @TODO
        {
            ikSnap.rightHandPos = ropePath.PointAlongPath(distOnPath - OnRope.rightHandOffsetOnRope) + (angularRotationOnRope * OnRope.rightHandOffset);
            ikSnap.leftHandPos = ropePath.PointAlongPath(distOnPath - OnRope.leftHandOffsetOnRope) + (angularRotationOnRope * OnRope.leftHandOffset);
            ikSnap.rightFootPos = ropePath.PointAlongPath(distOnPath - OnRope.rightFootOffsetOnRope) + (angularRotationOnRope * OnRope.rightFootOffset);
            ikSnap.leftFootPos = ropePath.PointAlongPath(distOnPath - OnRope.leftFootOffsetOnRope) + (angularRotationOnRope * OnRope.leftFootOffset);

            ikSnap.rightHandRot = angularRotationOnRope * Quaternion.Euler(OnRope.rightHandRot);
            ikSnap.leftHandRot = angularRotationOnRope * Quaternion.Euler(OnRope.leftHandRot);
            ikSnap.rightFootRot = angularRotationOnRope * Quaternion.Euler(OnRope.rightFootRot);
            ikSnap.leftFootRot = angularRotationOnRope * Quaternion.Euler(OnRope.leftFootRot);
        }

        return 0;
    }

    #endregion

    #region helpers
    
    float distToFloor
    {
        get { return myCol.height * 0.5f; }
    }
    float maxDistToFloor
    {
        get { return distToFloor + movement.groundTouchHeight; }
    }

    void ApplyMoveForce(float magnitude, float dt, Vector3 upVec)
    {
        const float stoppingSpeed = 10.0f;

        // used to counter dt so that inspector values are kinda simular for move/jump
        const float fps = 60.0f;
        // forceRot so that we can easily climb hills
        var forceRot = Quaternion.FromToRotation(Vector3.up, upVec.normalized);
        var velocity = myBody.velocity;

        if (input.moveDirRel != Vector3.zero)
        {
            // apply movement
            myBody.AddForce(forceRot * input.moveDirRel * magnitude * fps * dt, ForceMode.Acceleration);
        }
        else if (velocity != Vector3.zero) // no given movement direction, and have a velocity
        {
            // apply slowing movement
            if (!jumping && velocity.y > 0.0f) // when not jumping and going up
                myBody.velocity = Vector3.Lerp(velocity, new Vector3(0.0f, 0.0f, 0.0f), stoppingSpeed * dt);
            else
                myBody.velocity = Vector3.Lerp(velocity, new Vector3(0.0f, velocity.y, 0.0f), stoppingSpeed * dt);

        }
    }
    float CalcRedirectForceMultiplier(float redirectForceMultiplier)
    {
        // force multiplier for when going in a new direction (Improves responsivness)
        var redirectForce = 1.0f;
        if (myBody.velocity != Vector3.zero && input.moveDir != Vector3.zero)
        {
            float velDotDir = Vector3.Dot(myBody.velocity.normalized, input.moveDirRel.normalized);
            float normRedirectForce = Mathf.Abs((velDotDir - 1.0f) * 0.5f);
            redirectForce = 1.0f + normRedirectForce * redirectForceMultiplier;
        }
        return redirectForce;
    }
    
    void GroundRaycastPattern(int mask)
    {
        RaycastHit hit;
        Vector3 rayOrigin = transform.position;
        Vector3 rayOffset;
        float raycastDist = maxDistToFloor;

        // center raycast down
        if (RaycastGroundCheck(rayOrigin, mask, out hit, raycastDist) == false)
        {
            const int layersOfRaycast = 3;
            const int ringDensity = 6;
            float degreeOffset = 360.0f / ringDensity;
            Quaternion originRot = Quaternion.AngleAxis(degreeOffset, Vector3.up);
            Vector3 forward = transform.forward;
            float radius = myCol.radius;
            for (int i = 0; i < layersOfRaycast; ++i)
            {
                float distFromOrigin = ((float)(i + 1) / (float)layersOfRaycast) * (radius + movement.climbRadius);
                rayOffset = distFromOrigin * forward;
                for (int j = 0; j < ringDensity; ++j)
                {
                    // @TODO impliment climbing stuff
                    if(distFromOrigin > radius)
                    {
                        // DO STUFF HERE THIS INDICATES CLIMBING onto a ledge!!!
                    }
                    rayOffset = originRot * rayOffset;
                    if (RaycastGroundCheck(rayOrigin + rayOffset, mask, out hit, raycastDist))
                        return;
                }
            }

            // didn't reach any ground

            movement.up = Vector3.up;
            movement.details.groundTouches.Wash(false);
        }
    }
    bool RaycastGroundCheck(Vector3 raycastOrigin, int mask, out RaycastHit hit, float dist)
    {
        Debug.DrawRay(raycastOrigin, Vector3.down, Color.yellow);
        if (Physics.Raycast(raycastOrigin, Vector3.down, out hit, dist, mask))
        {
            float angleFromUp = Vector3.Angle(-hit.normal.normalized, -Vector3.up);

            // Ground not too steep to be considered ground
            if (angleFromUp <= movement.maxSlopeAngle)
            {
                // Set up normal
                movement.up = hit.normal.normalized;
                movement.details.groundTouches.Record(true);
                return true;
            }
        }
        return false;
    }
    void SnapToGround(float maxSnapDist)
    {
        int mask = LayerMask.GetMask("Solid");
        RaycastHit hit;
        if(RaycastGroundCheck(transform.position,mask, out hit, maxSnapDist))
        {
            transform.position = hit.point + Vector3.up * distToFloor;
        }
    }

    void RopeClimb(float amountUp)
    {
        OnRope.distUpRope = Mathf.Clamp(
            amountUp + OnRope.distUpRope,
            0.0f,
            OnRope.rope.GetPath().PathLength);
    }
    void RopeRotateOn(float amountRight)
    {
        OnRope.angleOnRope += amountRight;
    }
    void RopePump(float amountUp)
    {
        var epsilon = 0.00001f;
        var oldDist = OnRope.distPumpUp;
        OnRope.distPumpUp = Mathf.Clamp(OnRope.distPumpUp + amountUp, -OnRope.maxPumpDownDist, OnRope.maxPumpUpDist);

        // Done with the pump
        if (OnRope.distPumpUp + epsilon > OnRope.maxPumpUpDist)
            return;
        if (OnRope.distPumpUp - epsilon < -OnRope.maxPumpDownDist)
            return;

        if (oldDist != OnRope.distFromRope)
            OnRope.rope.velocity += OnRope.rope.velocity * (amountUp * OnRope.pumpAcceleration);
    }
    void RopeLean(Vector3 amountVec)
    {
        OnRope.rope.velocity += transform.rotation * amountVec;
        //Debug.DrawLine(transform.position, transform.position + amountVec * 20.0f, Color.grey);
    }



    void DisableGameObject(object gameObject_obj)
    {
        ((GameObject)gameObject_obj).SetActive(false);
    }
    void EnableGameObject(object gameObject_obj)
    {
        ((GameObject)gameObject_obj).SetActive(true);
    }
    void LoadLevelOfName(object string_LevelName)
    {
        Time.timeScale = 1.0f;
        SceneManager.LoadScene((string)string_LevelName);
    }
    #endregion

    public void SetupOnRope(RopeController rc)
    {
        var ropePath = rc.GetComponent<FFPath>();
        
        var playerPos = transform.position;
        float distAlongRope = 0;
        ropePath.NearestPointAlongPath(playerPos, out distAlongRope);
        float distUpRope = ropePath.PathLength - distAlongRope;

        OnRope.distUpRope = distUpRope;
        OnRope.rope = rc;
        SwitchMode(Mode.Rope);

        SetVelocityRef(new FFRef<Vector3>(
            () => OnRope.rope.VelocityAtDistUpRope(OnRope.distUpRope),
            (v) => {} ));

        RopeControllerUpdate rcu;
        rcu.controller = rc;
        OnRopeControllerUpdate(rcu);

        FFMessageBoard<RopeControllerUpdate>.Connect(OnRopeControllerUpdate, OnRope.rope.gameObject);
    }

    void DestroyOnRope()
    {
        // TODO preserve velocity of rope before switching mode

        if(OnRope.rope != null)
            FFMessageBoard<RopeControllerUpdate>.Disconnect(OnRopeControllerUpdate, OnRope.rope.gameObject);

        OnRope.rope = null;
    }

    
    void SwitchMode(Mode newMode)
    {
        // @TODO make this pass an event
        mode.Record(newMode);

        switch (newMode)
        {
            case Mode.Frozen:
                myBody.useGravity = false;
                myBody.isKinematic = true;
                break;
            case Mode.Movement:
                myBody.useGravity = true;
                myBody.isKinematic = false;
                break;
            case Mode.Rope:
                myBody.useGravity = false;
                myBody.isKinematic = true;
                break;
            case Mode.Climb:
                myBody.useGravity = false;
                myBody.isKinematic = true;
                break;
        }
    }

}
