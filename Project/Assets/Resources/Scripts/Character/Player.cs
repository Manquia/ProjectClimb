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


    FFAction.ActionSequence oneShotSeq; // never call sync OR  ClearSequence on this one...
    FFAction.ActionSequence orientSeq;
    FFAction.ActionSequence timeScaleSeq;

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

        public Annal<KeyState> use = new Annal<KeyState>(15, KeyState.Constructor);
        public Annal<KeyState> modifier = new Annal<KeyState>(15, KeyState.Constructor);
        public Annal<KeyState> up = new Annal<KeyState>(15, KeyState.Constructor);
        public Annal<KeyState> down = new Annal<KeyState>(15, KeyState.Constructor);
        public Annal<KeyState> left = new Annal<KeyState>(15, KeyState.Constructor);
        public Annal<KeyState> right = new Annal<KeyState>(15, KeyState.Constructor);
        public Annal<KeyState> space = new Annal<KeyState>(15, KeyState.Constructor);
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

        public float timeOnRope = 0.0f;
        public float transitionTimeOnRope = 0.4f;
        public AnimationCurve grabTransitionCurve; // @TODO move to misc?, but save the curve!! @CLEANUP

        // allows for smooth transition from other movementModes
        internal Vector3 grabPosition;
        internal Quaternion grabRotion;

    }
    public RopeConnection OnRope;

    [System.Serializable]
    public class Movement
    {
        public LayerMask groundPhysicsMask;
        public float maxSpeed = 2.5f;
        public float runMultiplier = 2.0f;

        public float jumpForce = 1000.0f;
        public float maxSlopeAngle = 55.0f;
        public float groundTouchHeight = 0.2f;
        public class Details
        {
            public Annal<bool> groundTouches = new Annal<bool>(16, false);
            public Annal<bool> jumping = new Annal<bool>(7, false);
        }
        public Details details = new Details();
        public float climbRadius = 0.45f;
        public float revJumpVel = 1.0f;
        internal Vector3 up;

        public bool grounded
        {
            // if touched ground this frame by any touches
            get
            {
                return details.groundTouches.Contains((v) => v);
            }
        }
        public bool jumping
        {
            get { return details.jumping; }
        }


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

    [System.Serializable]
    public class Miscellaneous
    {
        public AnimationCurve ropeToMovePlayerAlignmentCurve;
        public AnimationCurve ropeToMoveCameraAlignmentCurve;



        public float timeScaleMinimum = 0.05f;
        public FFVar<float> timeScaleVar = new FFVar<float>(1.0f);
        public AnimationCurve timeSlowCurve;
    }
    public Miscellaneous miscellaneous = new Miscellaneous();

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
        orientSeq = action.Sequence();
        timeScaleSeq = action.Sequence();
        oneShotSeq = action.Sequence();

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
        if (OnRope.rope != null)
            DestroyOnRope(Mode.Frozen);
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

                if (movement.grounded)
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

        UpdateTimeScale();
    }

    void UpdateTimeScale()
    {
        const float minTimeScale = 0.1f;
        float timeScaleVar = miscellaneous.timeScaleVar.Val;
        float newTimeScale = minTimeScale + (1.0f / (timeScaleVar + minTimeScale));

        Time.timeScale = newTimeScale;
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
        
        if (movement.grounded && movement.jumping)
            movement.details.jumping.Record(false);
    }

    void UpdateGroundRaycast()
    {
        GroundRaycastPattern(movement.groundPhysicsMask);
    }
    
    

    float dt;
    void UpdateInput()
    {
        input.moveDir.x = 0.0f;//Input.GetAxis("Horizontal");
        input.moveDir.y = 0.0f;
        input.moveDir.z = 0.0f;//Input.GetAxis("Vertical");

        UpdateKeyState(input.up, KeyCode.W);
        UpdateKeyState(input.down, KeyCode.S);
        UpdateKeyState(input.left, KeyCode.A);
        UpdateKeyState(input.right, KeyCode.D);
        UpdateKeyState(input.space, KeyCode.Space);
        UpdateKeyState(input.modifier, KeyCode.LeftShift);
        UpdateKeyState(input.use, KeyCode.E);

        input.moveDir.x += input.right.Recall(0).down() ? 1.0f : 0.0f;
        input.moveDir.x += input.left .Recall(0).down() ? -1.0f : 0.0f;
        input.moveDir.z += input.up   .Recall(0).down() ? 1.0f : 0.0f;
        input.moveDir.z += input.down .Recall(0).down() ? -1.0f : 0.0f;

        input.moveDirRel = transform.rotation * input.moveDir;

        dt = Time.deltaTime;
    }
    static void UpdateKeyState(Annal<KeyState> ks, KeyCode code)
    {
        if (Input.GetKeyDown(code)) ks.Record(KeyState.GetPressedKeyState());
        else if (Input.GetKeyUp(code)) ks.Record(KeyState.GetReleasedKeyState());
        else if (Input.GetKey(code)) ks.Record(KeyState.GetDownKeyState());
        else ks.Record(KeyState.GetUpKeyState()); // up = !down && !Pressed && !Released
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
        if (movement.grounded && input.space.Contains((v) => v.down()))
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
        if(!input.space.Recall(0).down() && myBody.velocity.y > 0.0f)
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
        if (input.modifier.Recall(0).down())
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

        if (input.space.Recall(0).pressed())
        {
            RopePump(pumpAmount);
        }

        bool flipClimbMod = false;
        // going up
        if (input.up.Recall(0).down())
        {
            if (input.modifier.Recall(0).down() == flipClimbMod)
                leanVec += new Vector3(0.0f, 0.0f, 1.0f);
            else
                climbVec += 1.0f;
        }
        // going down
        if (input.down.Recall(0).down())
        {
            if (input.modifier.Recall(0).down() == flipClimbMod)
                leanVec += new Vector3(0.0f, 0.0f, -1.0f);
            else
                climbVec += -1.0f;
        }

        bool flipRotateMod = false;
        // going right
        if (input.right.Recall(0).down() && !input.left.Recall(0).down())
        {
            if (input.modifier.Recall(0).down() == flipRotateMod)
                leanVec += new Vector3(1.0f, 0.0f, 0.0f);
        }
        // going left
        if (input.left.Recall(0).down() && !input.right.Recall(0).down())
        {
            if (input.modifier.Recall(0).down() == flipRotateMod)
                leanVec += new Vector3(-1.0f, 0.0f, 0.0f);
        }

        // Pump
        if (input.space.Recall(0).down())
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

        // Remove self from rope?
        if (input.use.Recall(0).pressed())
        {
            DestroyOnRope(Mode.Movement);
        }

    }
    private int OnRopeControllerUpdate(RopeControllerUpdate e)
    {
        Debug.Assert(OnRope != null, "UpdateRope is being called when OnRope is null");

        // count time for mu on rope
        OnRope.timeOnRope += e.dt;

        var rope = OnRope.rope;
        FFPath ropePath = rope.GetPath();
        float ropeLength = ropePath.PathLength;
        Vector3 ropeVecNorm = rope.RopeVecNorm();
        float mu = Mathf.Clamp(OnRope.timeOnRope / OnRope.transitionTimeOnRope, 0.0f, 1.0f);
        float sampleMu = OnRope.grabTransitionCurve.Evaluate(mu);

        var distOnPath = Mathf.Clamp(ropeLength - (OnRope.distUpRope), 0.0f, ropeLength);

        // update Character Position
        var AngleFromDown = Quaternion.FromToRotation(Vector3.down, ropeVecNorm);
        var angularRotationOnRope = Quaternion.AngleAxis(OnRope.angleOnRope, ropeVecNorm) * AngleFromDown;
        var positionOnRope = ropePath.PointAlongPath(distOnPath);
        Vector3 characterPos = positionOnRope +                                   // Position on rope
            (angularRotationOnRope * -Vector3.forward * OnRope.distFromRope) +  // set offset out from rope based on rotation
            (ropeVecNorm * -OnRope.distPumpUp);                                  // vertical offset from pumping
        transform.position = Vector3.Lerp(OnRope.grabPosition, characterPos, sampleMu);

        // update charater rotation
        var vecToRope = positionOnRope - transform.position;
        var forwardRot = Quaternion.LookRotation(vecToRope, -ropeVecNorm);
        transform.rotation = Quaternion.Lerp(OnRope.grabRotion, forwardRot, sampleMu);
        var characterRot = forwardRot * Quaternion.AngleAxis(OnRope.rotationPitch, transform.right) * Quaternion.AngleAxis(OnRope.rotationYaw, transform.forward);
        transform.rotation = Quaternion.Lerp(OnRope.grabRotion, characterRot, sampleMu);


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
            if (!movement.jumping && velocity.y > 0.0f) // when not jumping and going up
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


    #region Transitions

    public void SetupOnRope(RopeController rc)
    {
        Mode oldMode = mode;
        var ropePath = rc.GetPath();
        
        var playerPos = transform.position;
        var playerRot = transform.rotation;
        float distAlongRope = 0;
        ropePath.NearestPointAlongPath(playerPos, out distAlongRope);
        float distUpRope = ropePath.PathLength - distAlongRope;
        Vector3 nearestPointOnRope = ropePath.PointAlongPath(distAlongRope);
        Vector3 vecToNearestPointOnRope = nearestPointOnRope - playerPos;
        Vector3 vecToNearestPointOnRopeXZ = Vector3.ProjectOnPlane(vecToNearestPointOnRope, rc.RopeVecNorm());

        OnRope.angleOnRope = (Mathf.Atan2(vecToNearestPointOnRopeXZ.z, vecToNearestPointOnRopeXZ.x) * Mathf.Rad2Deg) - 90.0f;
        OnRope.distUpRope = distUpRope;
        OnRope.rope = rc;
        OnRope.timeOnRope = 0;              // set time to 0
        OnRope.grabPosition = playerPos;    // Set position for transition
        OnRope.grabRotion = playerRot;      // set rotation for trasitions

        SwitchMode(Mode.Rope);

        SetVelocityRef(new FFRef<Vector3>(
            () => OnRope.rope.VelocityAtDistUpRope(OnRope.distUpRope),
            (v) => {} ));

        // place ourselves onto the rope
        RopeControllerUpdate rcu;
        rcu.controller = rc;
        rcu.dt = 0.0f;
        OnRopeControllerUpdate(rcu);

        // Set Camera Controller's bound b/c unity can't do crap!
        // we should always grab onto the rope with it forward facing
        cameraController.cameraTurn = 0.0f;

        // finish any time slow effects
        timeScaleSeq.RunToEnd();

        FFMessageBoard<RopeControllerUpdate>.Connect(OnRopeControllerUpdate, OnRope.rope.gameObject);

        // wash movement details
        movement.details.groundTouches.Wash(false);
        movement.details.jumping.Wash(false);
    }

    void DestroyOnRope(Mode newMode)
    {
        // preserve velocity of rope before switching mode
        var ropeVelocity = OnRope.rope.VelocityAtDistUpRope(OnRope.distUpRope);
        myBody.velocity = ropeVelocity;

        if (OnRope.rope != null)
            FFMessageBoard<RopeControllerUpdate>.Disconnect(OnRopeControllerUpdate, OnRope.rope.gameObject);

        SwitchMode(newMode);

        // orient the player to its proper setup for 
        switch (newMode)
        {
            case Mode.Frozen:
                break;
            case Mode.Movement:

                // clear orient sequence of anything currently happeneing so we don't additivly hurt anything
                orientSeq.ClearSequence();

                // orient the player for movement Move
                {
                    Vector3 cameraForward = cameraController.transform.forward;
                    float forwardAngle = (Mathf.Atan2(cameraForward.z, -cameraForward.x) * Mathf.Rad2Deg) - 90.0f;
                    Quaternion forwardXZ = Quaternion.AngleAxis(forwardAngle, Vector3.up);
                    float angleTowardUprightOrientation = Quaternion.Angle(forwardXZ, transform.localRotation);
                    float anglesPerSecond = 120.0f;
                    float timeToUpright = angleTowardUprightOrientation / anglesPerSecond;

                    orientSeq.Property(ffrotation, forwardXZ, miscellaneous.ropeToMovePlayerAlignmentCurve, timeToUpright);
                }

                // orient the player's camera for movement Move
                {
                    Quaternion cameraUprightForward = Quaternion.identity;
                    float anglesTowardForwardAlignment = Quaternion.Angle(cameraUprightForward, cameraController.transform.localRotation);
                    float anglesPerSecond = 90.0f;
                    float timeToAlign = anglesTowardForwardAlignment / anglesPerSecond;
                    
                    orientSeq.Property(cameraController.ffrotation, cameraUprightForward, miscellaneous.ropeToMoveCameraAlignmentCurve, timeToAlign);
                }

                // When we aren't on the ground
                // update our physics to see if we should be on the ground
                GroundRaycastPattern(movement.groundPhysicsMask);
                if(movement.grounded)
                {
                    // throw self up high
                    timeScaleSeq.Property(miscellaneous.timeScaleVar, miscellaneous.timeScaleVar + 0.5f, miscellaneous.timeSlowCurve, 0.8f);


                }

                break;
            case Mode.Rope:
                break;
            case Mode.Climb:
                break;
        }

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


    #endregion Transitions

}
