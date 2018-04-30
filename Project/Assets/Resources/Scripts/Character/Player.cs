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
    public class GroundMovement
    {
        public float moveForce = 100.0f;
        public float redirectForce = 3.25f;
        public float maxSpeed = 2.5f;
        public float jumpForce = 1000.0f;
        public float friction = 0.1f;
        public float maxSlopeAngle = 55.0f;
        public float groundTouchHeight = 0.2f;
        public class Details
        {
            public Annal<bool> groundTouches = new Annal<bool>(16, false);
            public Annal<bool> jumping = new Annal<bool>(16, false);
        }
        public Details details = new Details();
        public float climbRadius = 0.45f;
        internal Vector3 up;
        
    }
    public GroundMovement OnGround;


    FFAction.ActionSequence fadeScreenSeq;
    [System.Serializable]
    public class AirMOvement
    {
        public float moveForce = 25.0f;
        public float redirectForce = 3.25f;
        public float maxSpeed = 2.5f;
        public float revJumpVel = 1.0f;
        public float friction = 0.1f;
        public State state = State.GoingDown;

        // @TODO Set these flags, Currently doesn nothing
        [Flags]
        public enum State
        {
            None = 0,
            Launching = 1,
            GoingUp = 2,
            Peaking = 4,
            GoingDown = 8,
            Grounded = 16,
        }
    }
    public AirMOvement OnAir;

    bool grounded
    {
        // if touched ground this frame by any touches
        get
        {
            return OnGround.details.groundTouches.Contains((v) => v);
        }
    }
    bool jumping
    {
        get { return OnGround.details.jumping; }
    }

    // @TODO make this into an annal


    //bool groundedThisFrame = false;

    // Use this for initialization
    private void Awake()
    {
        myBody = GetComponent<Rigidbody>();
        myCol = GetComponent<CapsuleCollider>();
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

        // make sure stuff is on, We have it off from the
        // start of the level
        GetComponent<CameraController>().enabled = true;
        transform.Find("Camera").gameObject.SetActive(true);

        // Fade Screen
        {
            // init
            fadeScreenSeq = action.Sequence();
            fadeScreenSeq.affectedByTimeScale = false;
            Seq_FadeOutScreenMasks();
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
        if (grounded)
            UpdateMoveGround(dt);
        else
            UpdateMoveAir(dt);
        
        OnGround.details.groundTouches.Wash(false);
    }
    // Called right after physics, before rendering. Use for Kinimatic player actions
    void Update()
    {
        UpdateInput();

        UpdateGroundRaycast();
        UpdateJumpState();
        // @TODO have this work through an event or something
        //if (OnRope.rope != null)
        //    SetupOnRope();
        // @TODO add statemachine OR check on OnRope
        // @ROPE @FIX
        // Controls for movement on a rope
        // UpdateRope(up, down, left, right, space, modifier, dt);
    }

    private void UpdateJumpState()
    {
        // grounded && !jumping && spacePressed -> jumping = true
        // grounded && jumping -> jumping = false
        
        if (grounded && OnGround.details.jumping)
            OnGround.details.jumping.Record(false);
    }

    void UpdateGroundRaycast()
    {
        var mask = LayerMask.GetMask("Solid");
        
        GroundRaycastPattern(mask);
    }
    
    

    // @CLEAN UP
    Vector3 moveDir;
    Vector3 moveDirRel;
    // history of last 15 freams
    Annal<bool> space = new Annal<bool>(15, false);
    bool modifier;
    float dt;
    void UpdateInput()
    {
        moveDir.x = 0.0f;//Input.GetAxis("Horizontal");
        moveDir.y = 0.0f;
        moveDir.z = 0.0f;//Input.GetAxis("Vertical");

        moveDir.x += Input.GetKey(KeyCode.D) ? 1.0f : 0.0f;
        moveDir.x += Input.GetKey(KeyCode.A) ? -1.0f : 0.0f;
        moveDir.z += Input.GetKey(KeyCode.W) ? 1.0f : 0.0f;
        moveDir.z += Input.GetKey(KeyCode.S) ? -1.0f : 0.0f;

        moveDirRel = transform.rotation * moveDir;

        space.Record(Input.GetKeyDown(KeyCode.Space));
        modifier = Input.GetKey(KeyCode.LeftShift);
        dt = Time.deltaTime;
    }


    void UpdateMoveGround(float dt)
    {
        var rotOfPlane = Quaternion.FromToRotation(OnGround.up, Vector3.up);
        var revRotOfPlane = Quaternion.FromToRotation(Vector3.up, OnGround.up);

        // movement in the Y axis (jump), Grounded && space in the last few frames?
        if (grounded && space.Contains((v) => v))
        {
            SnapToGround(maxDistToFloor);
            OnGround.details.jumping.Record(true);
            OnGround.details.groundTouches.Wash(false);

            
            var relVel = rotOfPlane * myBody.velocity;
            var relVelXY = new Vector3(relVel.x, 0.0f, relVel.z);
            myBody.velocity = revRotOfPlane * relVelXY;

            myBody.AddForce(transform.up * OnGround.jumpForce);
        }

        // apply friction
        {
            myBody.velocity = myBody.velocity - (myBody.velocity * OnGround.friction * dt);
        }


        // apply move force to rigid body
        float redirectForce = CalcRedirectForceMultiplier(OnGround.redirectForce);
        ApplyMoveForce(OnGround.moveForce * redirectForce, dt, OnGround.up);
        
        // Clamp Velocity along horizontal plane
        {
            var maxSpeed = OnGround.maxSpeed;
            var relVel = rotOfPlane * myBody.velocity;
            var relVelXY = new Vector3(relVel.x, 0.0f, relVel.z);
            var relVelXYNorm = relVelXY.normalized;
            float horzontalSpeed = relVelXY.magnitude;

            float newSpeed = maxSpeed; // Mathf.Lerp(horzontalSpeed, maxSpeed, 0.8f * dt);
            if(horzontalSpeed > maxSpeed)
            {
                Vector3 newVel = new Vector3(relVelXYNorm.x * newSpeed, relVel.y, relVelXYNorm.z * newSpeed);
                myBody.velocity = revRotOfPlane * newVel;
            }
        }

        // Rotate based on mouse look
        if (cameraController.lookVec.x != 0)
        {
            float turnAmount = cameraController.lookVec.x;
            var rotation = Quaternion.AngleAxis(turnAmount, Vector3.up);
            transform.localRotation = transform.localRotation * rotation;
        }
    }
    void UpdateMoveAir(float dt)
    {

        // apply move force to rigid body
        float redirectForce =  CalcRedirectForceMultiplier(OnAir.redirectForce);
        ApplyMoveForce(OnAir.moveForce * redirectForce, dt, Vector3.up);

        // Clamp Velocity along horizontal plane
        {
            var maxSpeed = OnAir.maxSpeed;
            var velXZ = new Vector3(myBody.velocity.x, 0.0f, myBody.velocity.z);
            var velXZNorm = velXZ.normalized;
            float horzontalSpeed = velXZ.magnitude;
            
            if(horzontalSpeed > maxSpeed)
            {
                float newSpeed = maxSpeed; // Mathf.Lerp(horzontalSpeed, maxSpeed, 0.9f * dt);
                myBody.velocity = new Vector3(velXZNorm.x * newSpeed, myBody.velocity.y, velXZNorm.z * newSpeed);
            }
        }

        // apply friction
        {
            myBody.velocity = myBody.velocity - (myBody.velocity * OnAir.friction * dt);
        }

        //@TODO JumpStop
        // Stopped pressing jump && still moving upward
        if (!space && myBody.velocity.y > 0.0f)
        {
            Vector3 revJumpVel = OnAir.revJumpVel * myBody.velocity.y * -Vector3.up;
            myBody.velocity = myBody.velocity + revJumpVel;
        }

        // @Placeholder @DoubleJump/AirJump @Idea
        // movement in the Y axis (jump)
        //if (grounded && space)
        //{
        //    OnGround.details.groundTouches.Wash(false);
        //    myBody.AddForce(transform.up * OnGround.jumpForce);
        //}

        // Rotate based on mouse look
        if (cameraController.lookVec.x != 0)
        {
            float turnAmount = cameraController.lookVec.x;
            var rotation = Quaternion.AngleAxis(turnAmount, Vector3.up);
            transform.localRotation = transform.localRotation * rotation;
        }
        
    }

    

    // @TODO make this work with Vec2 for directional move input
    void UpdateRopeActions(bool up, bool down, bool left, bool right, bool space, bool modifier, float dt)
    {
        float pumpAmount = OnRope.pumpSpeed * dt;
        float climbAmount = OnRope.climbSpeed * dt;
        float rotateAmount = OnRope.rotateSpeed * dt;
        float leanAmount = OnRope.leanSpeed * dt;

        Vector3 leanVec = Vector3.zero;
        float climbVec = 0.0f;

        if (space)
        {
            RopePump(pumpAmount);
        }

        bool flipClimbMod = false;
        // going up
        if (up)
        {
            if (modifier == flipClimbMod)
                leanVec += new Vector3(0.0f, 0.0f, 1.0f);
            else
                climbVec += 1.0f;
        }
        // going down
        if (down)
        {
            if (modifier == flipClimbMod)
                leanVec += new Vector3(0.0f, 0.0f, -1.0f);
            else
                climbVec += -1.0f;
        }

        bool flipRotateMod = false;
        // going right
        if (right && !left)
        {
            if (modifier == flipRotateMod)
                leanVec += new Vector3(1.0f, 0.0f, 0.0f);
        }
        // going left
        if (left && !right)
        {
            if (modifier == flipRotateMod)
                leanVec += new Vector3(-1.0f, 0.0f, 0.0f);
        }

        // Pump
        if (space)
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
            float lookVec = cameraController.lookVec.x;
            float sensitivityRotate = Mathf.Abs(cameraController.cameraTurn / cameraController.maxTurnAngle);
            sensitivityRotate = sensitivityRotate * sensitivityRotate;

            float turnAmount = lookVec * sensitivityRotate;

            RopeRotateOn(-turnAmount * OnRope.rotateSpeed * dt);
        }

    }
    private void OnRopeChange(RopeChange e)
    {
        UpdateRope(e.dt);
    }
    void UpdateRope(float dt)
    {
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
    }

    #endregion

    #region helpers
    
    float distToFloor
    {
        get { return myCol.height * 0.5f; }
    }
    float maxDistToFloor
    {
        get { return distToFloor + OnGround.groundTouchHeight; }
    }

    void ApplyMoveForce(float magnitude, float dt, Vector3 upVec)
    {
        // used to counter dt so that inspector values are kinda simular for move/jump
        const float fps = 60.0f;
        // forceRot so that we can easily climb hills
        var forceRot = Quaternion.FromToRotation(Vector3.up, upVec.normalized);
        var velocity = myBody.velocity;

        if (moveDirRel != Vector3.zero)
        {
            // apply movement
            myBody.AddForce(forceRot * moveDirRel * magnitude * fps * dt, ForceMode.Acceleration);
        }
        else if (velocity != Vector3.zero) // no given movement direction, and have a velocity
        {
            // apply slowing movement
            if (!jumping && velocity.y > 0.0f) // when not jumping and going up
                myBody.velocity = Vector3.Lerp(velocity, new Vector3(0.0f, 0.0f, 0.0f), 0.9f);
            else
                myBody.velocity = Vector3.Lerp(velocity, new Vector3(0.0f, velocity.y, 0.0f), 0.9f);

        }
    }
    float CalcRedirectForceMultiplier(float redirectForceMultiplier)
    {
        // force multiplier for when going in a new direction (Improves responsivness)
        var redirectForce = 1.0f;
        if (myBody.velocity != Vector3.zero && moveDir != Vector3.zero)
        {
            float velDotDir = Vector3.Dot(myBody.velocity.normalized, moveDirRel.normalized);
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
                float distFromOrigin = ((float)(i + 1) / (float)layersOfRaycast) * (radius + OnGround.climbRadius);
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
        }
    }
    bool RaycastGroundCheck(Vector3 raycastOrigin, int mask, out RaycastHit hit, float dist)
    {
        Debug.DrawRay(raycastOrigin, Vector3.down, Color.yellow);
        if (Physics.Raycast(raycastOrigin, Vector3.down, out hit, dist, mask))
        {
            float angleFromUp = Vector3.Angle(-hit.normal.normalized, -Vector3.up);

            // Ground not too steep to be considered ground
            if (angleFromUp <= OnGround.maxSlopeAngle)
            {
                // Set up normal
                OnGround.up = hit.normal.normalized;
                OnGround.details.groundTouches.Record(true);
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

    void SetupOnRope()
    {
        SetVelocityRef(new FFRef<Vector3>(
            () => OnRope.rope.VelocityAtDistUpRope(OnRope.distUpRope),
            (v) => {} ));

        FFMessageBoard<RopeChange>.Connect(OnRopeChange, OnRope.rope.gameObject);
    }

    void DestroyOnRope()
    {
        if(OnRope.rope != null)
            FFMessageBoard<RopeChange>.Disconnect(OnRopeChange, OnRope.rope.gameObject);
    }
    
}
