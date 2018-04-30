using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;

//////////////////////////////////////////////////////
// Author: Micah Rust
// Data: 29/6/2015
// Purpose: A path script to interpolate between
//      points. Transfer betweena point in space to a
//      Line/curve. I have tried to make this as
//      flexable as possible. Sky's the limit. :)
//
//      The only added functionaity
//      you may want to add if changing the points from
//      Vector3[] to FFRef<Vector3>[] which will allow
//      for moving points on a path at a minor performance
//      cost. This would take some doing, but might be
//      more useful then seting points manually.
//
// Usage: AI enemy movement, Camera movement, anything
//      which needs to move along a line/curve
//
// Notes: Cannot have more than one FFPath per object
// because Unity doesn't seem to call awake on all
// FFPaths to setup the LinearDistanceAlongPath. Therefore,
// if you do this you will need to call SetupPointData
// yourself
///////////////////////////////////////////////////////

[ExecuteInEditMode]
public class FFPath : MonoBehaviour
{
#region DebugConstants

    //////////////////// Stats when Unselected ////////////////////
    // point radius and color
    private const float DebugPointsRadius = 0.1f;
    private Color DebugPointsColorUnselected = Color.blue;
    // Lower value to increase debug line precision
    private const float DebugLineLengthUnselected = 0.2f;
    private Color DebugLineColorUnselected = Color.blue;


    //////////////////// Stats when Selected ////////////////////
    // point radius and color
    private Color DebugPointsColorSelected = Color.yellow;
    // Lower value to increase debug line precision
    private const float DebugLineLengthSelected = DebugLineLengthUnselected * 0.3f;
    private Color DebugLineColorSelected = Color.yellow;
#endregion DebugConstants

#region Setup

    void Awake()
    {
        SetupPointData();
    }
    /// <summary>
    /// This function will setup the path for any
    /// changes made. Will automatically be called
    /// if path is dynamic each time the path is used.
    /// 
    /// For performance reasons, call this manually
    /// instead of using dynamic path if your path
    /// isn't changing everytime you use it.
    /// </summary>
    /// <returns></returns>
    public bool SetupPointData()
    {
        if ((points.Length + 1) != linearDistanceAlongPath.Length)
        {
            linearDistanceAlongPath = new float[points.Length + 1];
            linearDistanceAlongPath[0] = 0.0f;
        }

        if (points.Length > 1)
        {
            for (int i = 1; i < points.Length; ++i)
            {
                // Setup all distances along path for log2(n) lookup(s)
                linearDistanceAlongPath[i] =
                    Vector3.Distance(points[i - 1], points[i])
                    + linearDistanceAlongPath[i - 1];
            }

            //  setup last distance value first circuits
            linearDistanceAlongPath[points.Length] = Vector3.Distance(points[0], points[points.Length - 1]) + linearDistanceAlongPath[points.Length - 1];
            return true;
        }

        return false;
    }
#endregion Setup

#region Destroy

    void OnDestroy()
    {
#if UNITY_EDITOR
        DestroyModifiablePoints();
#endif // UNITY_EDTIOR
    }
#endregion Destroy

#region DebugDraw

#if UNITY_EDITOR

    bool inEditorModifyAndDraw = false; // to see if we changed modify
    void Start()
    {
        inEditorModifyAndDraw = ModifyAndDraw;
        if (ModifyAndDraw)
        {
            SetupModifyablePoints();
            UpdateModifyablePoints();
            TransferPointData();
        }
    }
    // Do drawing of points
    void Update()
    {
        // Change to ModifyAndDraw
        if (inEditorModifyAndDraw != ModifyAndDraw)
        {
            //Debug.Log("Changed ModifyAndDraw");
            inEditorModifyAndDraw = ModifyAndDraw;
            if (ModifyAndDraw)
                SetupModifyablePoints();
            else
                DestroyModifiablePoints();
        }

        // if we are are modifying points
        if(ModifyAndDraw)
        {
            //Debug.Log("FirstPoint: " + points[0]);
            UpdateModifyablePoints();
            TransferPointData();
        }
    }
    void OnDisable()
    {
        ModifyAndDraw = false;
        DestroyModifiablePoints();
    }


    // Needed to insert objects into the path
    private GameObject editor_activeObject;
    private string editor_FFPathObjectName;
    // Path Drawing

    void OnDrawGizmos() // unselected
    {
        Update();
        DrawDebugLinesGizmo(DebugLineColorUnselected, DebugLineLengthUnselected);
    }
    void OnDrawGizmosSelected()
    {
        Update();
        DrawDebugLinesGizmo(DebugLineColorSelected, DebugLineLengthSelected);
    }
    



    private string FFPathDebugPointsPoolName(string objName)
    {
        return "[" + objName + "] FFPathDebugPointsPool";
    }
    private string FFPathDebugPointName(string objName)
    {
        return "[" + objName + "] FFPathDebugPoint";
    }



    private GameObject MakeDebugPoint(Transform parent, Vector3 point)
    {
        var debugPoint = new GameObject(FFPathDebugPointName(gameObject.name));
        debugPoint.transform.position = point;
        var debugSphere = debugPoint.AddComponent<FFDebugDrawSphere>();
        debugSphere.radius = DebugPointsRadius;
        debugSphere.DrawColor = DebugPointsColorUnselected;
        debugSphere.DrawColorSelected = DebugPointsColorSelected;
        debugPoint.transform.SetParent(parent, false);

        // TODO: figure out why this is buggy for Don't save in editor/build
        debugPoint.hideFlags =
            HideFlags.DontSaveInEditor;
        return debugPoint;
    }
    // returns if the points need to be setup.

    void SetupModifyablePoints()
    {
        var poolobj = transform.Find(FFPathDebugPointsPoolName(gameObject.name));
        if(poolobj == null)
            poolobj = new GameObject(FFPathDebugPointsPoolName(gameObject.name)).transform;

        var poolTrans = poolobj.transform;

        // Save the current gameobjects name so we can verify that it has changed at
        // a later time.
        editor_FFPathObjectName = gameObject.name;
        
        // Set Pivot point to average position of each point
        if(points.Length > 0)
        {
            var avePosition = Vector3.zero;
            foreach(var point in this.points)
                avePosition += point;

            avePosition /= points.Length;
            poolTrans.position = avePosition;
        }
        else // no points
        {
            poolTrans.position = new Vector3(0, 0, 0);
        }
        poolTrans.SetParent(this.gameObject.transform, false);

        //poolobj.hideFlags =
        //    HideFlags.HideInInspector;

        // Make Children to mimic points
        foreach (var point in this.points)
        {
            MakeDebugPoint(poolTrans, point);
        }
    }
    public void UpdateModifyablePoints()
    {
        bool setupNeeded = false;

        // FFPath's object name may have been changed so we want to modify the
        // sub nodes to match if it has changed
        if (editor_FFPathObjectName != FFPathDebugPointsPoolName(gameObject.name))
        {
            // rename object to new name
            var pointPool = transform.Find(editor_FFPathObjectName);
            if(pointPool != null) // if we found it
            {
                pointPool.name = FFPathDebugPointsPoolName(gameObject.name); // rename it to new name
                // rename its children as well
                for(int i = 0; i < pointPool.childCount; ++i)
                {
                    pointPool.GetChild(i).name = FFPathDebugPointName(gameObject.name);
                }

                // Save new name of pool
                editor_FFPathObjectName = FFPathDebugPointsPoolName(gameObject.name);
            }
        }

        // Get pool
        var pool = transform.Find(FFPathDebugPointsPoolName((gameObject.name)));
        
        // Update on object count on changes
        var childCount = pool.childCount;
        var pointCount = points.Length;
        var pointDelta = pointCount - childCount;

        if (pointDelta > 0) // delta is positive: DebugObject(s) destroyed OR Point(s) Added
        {
            setupNeeded = true;
            if(editor_activeObject != null && // has selection
               editor_activeObject.GetComponent<FFPath>() != null ) // FFPath selected
            {
                // Point(s) Added: Points will overwrite debugObjects
                //Debug.Log("Point(s) Added: Points will overwrite debugObjects");

                for (int i = 0; i < pointDelta; ++i)
                {
                    Vector3 vecInsertToPrev = points[Math.Max(0, childCount - 2)] - points[Math.Max(0, childCount - 1)];
                    if(vecInsertToPrev == Vector3.zero)
                        vecInsertToPrev = new Vector3(0.0f,1.0f,0.0f);

                    var point = points[Math.Max(0, childCount - 1)] + (-vecInsertToPrev * (float)(i + 1));
                    MakeDebugPoint(pool, point);
                    points[childCount + i] = point; // new points get the vector of the last points
                }
            }
            else
            {
                // debugObject(s) destroyed: Debug objects will overwrite points
                //Debug.Log("debugObject(s) destroyed: debug objects will overwrite points");
                var newPointsArray = new Vector3[childCount];
                for (int i = 0; i < childCount; ++i)
                {
                    newPointsArray[i] = pool.GetChild(i).position; // point found
                }
                points = newPointsArray; // set to new array
            }
        }
        else if(pointDelta < 0) // delta is negative: DebugObject(s) created OR point(s) Removed
        {
            setupNeeded = true;

            if (editor_activeObject != null && // has selection
                editor_activeObject.GetComponent<FFPath>() != null) // FFPath selected
            {
                // Point(s) Removed: Point(s) overwrite DebugObject(s)
                //Debug.Log("Point(s) Removed: Point(s) overwrite DebugObject(s)");

                for(int i = childCount - 1; i > pointCount - 1; --i)
                {
                    DestroyImmediate(pool.GetChild(i).gameObject);
                }

            }
            else
            {
                // DebugObject(s) created: debug objects will overwrite points
                // Inserts newly created objects
                //Debug.Log("DebugObject(s) created: debug objects will overwrite points");

                int indexOfInsertion;
                // no selection OR FFPath selected
                if (editor_activeObject == null || editor_activeObject.GetComponent<FFPath>() != null) // no selection, last element of selection deleted, 
                {
                    // insert back
                    //Debug.Log("Insert Back");
                    indexOfInsertion = pointCount;
                }
                else
                {
                    indexOfInsertion = editor_activeObject.transform.GetSiblingIndex();
                    //Debug.Log("Insert: " + indexOfInsertion);
                }

                //shift index of selection
                var selection = UnityEditor.Selection.gameObjects;
                for (int i = 0; i < selection.Length; ++i)
                {
                    selection[i].transform.SetSiblingIndex(indexOfInsertion + 1 + i);
                }

                var newPointsArray = new Vector3[childCount];
                for (int i = 0; i < childCount; ++i)
                {
                    newPointsArray[i] = pool.GetChild(i).position; // point found
                }
                points = newPointsArray; // set to new array
            }
	    }

        // Save last frames selection
        //editorSelection = UnityEditor.Selection.gameObjects;
        editor_activeObject = UnityEditor.Selection.activeGameObject;

        if (setupNeeded)
            SetupPointData();

        return;
    }
    public void TransferPointData()
    {
        var pool = transform.Find(FFPathDebugPointsPoolName(gameObject.name));

        Debug.Assert(pool != null, "Pool object not found");
        Debug.Assert(pool.childCount == points.Length, "ERROR: PointCount should equal DebugObject count");
        // Setup will ensure that point count
        
        if(editor_activeObject != null && // has selection
           editor_activeObject.GetComponent<FFPath>() != null) // FFPath selected
        {

            //Debug.Log("Transfer Data: FFPath --> Points");
            // Point(s) could be modified: overwrite DebugObject(s)
            var pointCount = points.Length;
            for(int i = 0; i < pointCount; ++i)
            {
                pool.GetChild(i).position = transform.TransformPoint(points[i]);
            }
        }
        else // other objects selected
        {
            //Debug.Log("Transfer Data: Points --> FFPath");
            // DebugObject(s) could be modified: Overwrite Point(s)
            var pointCount = points.Length;
            for (int i = 0; i < pointCount; ++i)
            {
                points[i] = transform.InverseTransformPoint(pool.GetChild(i).position);
            }
        }
    }
    public void DestroyModifiablePoints()
    {
        var pool = transform.Find(FFPathDebugPointsPoolName(gameObject.name));
        if(pool != null)
        {
            Debug.Log("Destroyed Pool");
            DestroyImmediate(pool.gameObject);
        }
    }
    public void DrawDebugLinesGizmo(Color drawColor, float lineDensity)
    {
        if (points.Length > 1)
        {
            // Save DynamicPath property
            bool dynamicPathTemp = DynamicPath;
            // Save Gizmo color
            Color gizmoColorTemp = Gizmos.color;
            Gizmos.color = drawColor;

            DynamicPath = false;
            SetupPointData();

            Vector3 n1, p1;
            n1 = transform.TransformPoint(points[0]);

            for (float f = 0; f < PathLength - lineDensity; f += lineDensity)
            {
                p1 = PointAlongPath(f + lineDensity);
                Gizmos.DrawLine(n1, p1);
                n1 = p1;
            }
            p1 = PointAlongPath(PathLength);
            Gizmos.DrawLine(n1, p1);

            // reset DynamicPath Property
            DynamicPath = dynamicPathTemp;
            // reset Gizmo color
            Gizmos.color = gizmoColorTemp;
                
        }
    }
    public void DrawDebugPoints(Color drawColor, float radius)
    {
        var pos = transform.position;
        // Save DynamicPath property
        bool dynamicPathTemp = DynamicPath;
        // Save Gizmo color
        Color gizmoColorTemp = Gizmos.color;
        Gizmos.color = drawColor;

        DynamicPath = false;
        SetupPointData();
                
        for (int i = 0; i < points.Length; ++i)
        {
            Gizmos.DrawSphere(transform.TransformPoint(points[i]), radius);
        }

        // reset DynamicPath Property
        DynamicPath = dynamicPathTemp;
        // reset Gizmo color
        Gizmos.color = gizmoColorTemp;
    }
#endif // UNITY_EDITOR

#endregion DebugDraw

#region Data

    // Types of Paths
    public enum PathInterpolator
    {
        Linear,
        Curved,
    }

    [SerializeField]
    public PathInterpolator InterpolationType;
    [SerializeField]
    public bool Circuit = false;
    [SerializeField]
    public bool DynamicPath = false;
    [SerializeField]
    public bool SmoothBetweenPoints = false;
    [SerializeField]
    public bool ModifyAndDraw = true;
    [SerializeField]
    public Vector3[] points =
    { new Vector3(0, 0, 0), new Vector3(0,1,0), new Vector3(0,2,0) };

    [HideInInspector]
    public float[] linearDistanceAlongPath = { 0,1,2 };
    [HideInInspector]
    public int PointCount
    {
        get
        {
            if (Circuit)
                return points.Length + 1;
            else
                return points.Length;
        }
    }
    [HideInInspector]
    public float PathLength
    {
        get
        {
            if(points.Length > 1) // atleast 2 points
            {
                if (Circuit)
                {
                    return linearDistanceAlongPath[points.Length];
                    // exception here at run time, usually means you should enable Dynamic Path
                }
                else
                {
                    return linearDistanceAlongPath[points.Length - 1];
                }
            }
            return 0.0f; // only 1 point
        }
    }

#endregion Data

#region CodeInterface


    public Vector3 PositionAtPoint(int index)
    {
        return transform.TransformPoint(points[index]);
    }
    /// <summary>
    /// Returns whether the path is valid to use.
    /// to use.
    /// </summary>
    public bool IsValidPath()
    {
        // must have 2 or more points
        if(points.Length < 2)
        {
            //Debug.LogError("ERROR: Path is not valid to use. Must have 2 or more points");
            return false;
        }
        // Points must not share the exact same position
        if(linearDistanceAlongPath[linearDistanceAlongPath.Length - 1] == 0)
        {
            //Debug.LogError("ERROR: FFPath is not valid to use. Points much be some distance from eachother");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Returns the distance along the path to a given point index.
    /// the index may exceed the point count and will return the
    /// the length as if it were looping.
    /// </summary>
    public float LengthAlongPathToPoint(int pointIndex)
    {
        var currentPoint = pointIndex % points.Length;
        var numberOfLoops = pointIndex / points.Length;
        return numberOfLoops * PathLength + linearDistanceAlongPath[currentPoint];
    }

    /// <summary>
    /// Returns a Vector3 of the position which
    /// corresponds to the distance along the path
    /// </summary>
    public Vector3 PointAlongPath(float distanceAlongPath)
    {
        float distmod = distanceAlongPath % PathLength;
        float distNegEqualZero = (distmod + PathLength) % PathLength;
        float distPos = distmod > 0 ? distPos = distmod : distPos = PathLength;

        
        if (distanceAlongPath <= 0) // given negative/zero distance
            distanceAlongPath = distNegEqualZero;
        else   // given positive distance
            distanceAlongPath = distPos;
        
        if ((!DynamicPath && IsValidPath()) || (SetupPointData() && IsValidPath()))
        {
            switch (InterpolationType)
            {
                case PathInterpolator.Linear:
                    if (SmoothBetweenPoints)
                        return transform.TransformPoint(InterpolateLinearSmoothPositionAlongPath(distanceAlongPath));
                    else
                        return transform.TransformPoint(InterpolateLinearPositionAlongPath(distanceAlongPath));
                case PathInterpolator.Curved:
                    if (SmoothBetweenPoints)
                        return transform.TransformPoint(InterpolateCatmullRomSmoothPositionAlongPath(distanceAlongPath));
                    else
                        return transform.TransformPoint(InterpolateCatmullRomPositionAlongPath(distanceAlongPath));
                default:
                    Debug.LogError("Unhandled Interpolation Type");
                    return new Vector3(0, 0, 0);
            }
        }
        else
        {
            Debug.LogError("PositionAlongLine failed");
            return new Vector3(0, 0, 0);
        }
    }

    /// <summary>
    /// returns the nearest point along path to the givenPoint and the distance along
    /// the path at which this point is. There are inaccuracies for curved paths.
    /// </summary>
    public Vector3 NearestPointAlongPath(Vector3 givenPoint, out float distAlongPathToNearestPoint)
    {
        distAlongPathToNearestPoint = -1.0f;
        Vector3 pos;
        if (transform != null) { pos = transform.position; }
        else { pos = new Vector3(0, 0, 0); }

        if ((!DynamicPath && IsValidPath()) || (SetupPointData() && IsValidPath()))
        {
            // Rotate relative point to match path's rotation, and / the scale
            givenPoint = FFMatrix3X3.ScaleBy(Quaternion.Inverse(transform.rotation) * (givenPoint - pos),
                new Vector3(1.0f/transform.lossyScale.x,1.0f/transform.lossyScale.y,1.0f/transform.lossyScale.z));

            float closestDist = float.MaxValue;


            //check if first point is closest
            Vector3 nearestPoint = givenPoint - points[0];
            if (nearestPoint.sqrMagnitude < closestDist)
            {
                closestDist = nearestPoint.sqrMagnitude;
                distAlongPathToNearestPoint = linearDistanceAlongPath[0];
            }

            var pointCount = PointCount;
            for (int i = 1; i < pointCount; ++i)
            {
                Vector3 nextPoint = points[i % points.Length];
                Vector3 prevPoint = points[i - 1];
                Vector3 vecToGivenPoint = givenPoint - prevPoint;
                Vector3 vecToNextPoint = nextPoint - prevPoint;
                Vector3 vecToClosestPointOnLine = Vector3.Project(vecToGivenPoint, vecToNextPoint);
                Vector3 vecToPointOnLine = (vecToClosestPointOnLine + prevPoint) - givenPoint;

                nearestPoint = givenPoint - nextPoint;
                if (nearestPoint.sqrMagnitude < closestDist)
                {
                    closestDist = nearestPoint.sqrMagnitude;
                    distAlongPathToNearestPoint = linearDistanceAlongPath[i] * linearDistanceAlongPath[i];
                }

                if (vecToClosestPointOnLine.sqrMagnitude > vecToNextPoint.sqrMagnitude ||
                    Vector3.Dot(vecToGivenPoint, vecToNextPoint) < 0)
                {
                    continue;
                }

                if (vecToPointOnLine.sqrMagnitude < closestDist)
                {
                    closestDist = vecToPointOnLine.sqrMagnitude;
                    distAlongPathToNearestPoint = vecToClosestPointOnLine.magnitude + linearDistanceAlongPath[i - 1];
                    distAlongPathToNearestPoint = distAlongPathToNearestPoint * distAlongPathToNearestPoint;

                    // Debug draw
                    // vecToGivenPoint
                    //Debug.DrawLine(points[i - 1], vecToGivenPoint + points[i - 1], Color.yellow); 
                    // vecToNextPoint
                    //Debug.DrawLine(points[i - 1], vecToNextPoint + points[i - 1], Color.red);  
                    // Projection of vecToGivenPoint onto vecToNextPoint
                    //Debug.DrawLine(points[i - 1], vecToClosestPointOnLine + points[i - 1], Color.magenta);
                }
            }

            // Since we already ran the SetupPointData fuction above
            // we shouldn't need to do it again in PointAlongPath, also
            // turning off smoothBetweenPoints increases accuracy
            bool tempDynamicPath = DynamicPath;
            bool tempSmoothBetweenPoints = SmoothBetweenPoints;
            DynamicPath = false;            // Optimization
            SmoothBetweenPoints = false;    // Accuracy
            distAlongPathToNearestPoint = Mathf.Sqrt(distAlongPathToNearestPoint); // sqrt to get size right
            Vector3 nearestPointAlongPath = PointAlongPath(distAlongPathToNearestPoint);
            DynamicPath = tempDynamicPath;
            SmoothBetweenPoints = tempSmoothBetweenPoints;
            return nearestPointAlongPath;
        }
        else
        {
            Debug.LogError("NearestPointAlongPath failed to setup");
            return new Vector3(0,0,0);
        }
    }

    /// <summary>
    /// returns the nearest point in the path to the givenPoint
    /// @@TODO: Dynamic path stuff
    /// </summary>
    public Vector3 NearestPoint(Vector3 givenPoint)
    {
        Vector3 pos;
        if (transform != null) { pos = transform.position; }
        else { pos = new Vector3(0, 0, 0); }
        
        if ((!DynamicPath && IsValidPath()) || (SetupPointData() && IsValidPath()))
        {
            // offset, rotate and scale point to get it in the same world space as path
            givenPoint = FFMatrix3X3.ScaleBy(Quaternion.Inverse(transform.rotation) * (givenPoint - pos),
                new Vector3(1.0f / transform.lossyScale.x, 1.0f / transform.lossyScale.y, 1.0f / transform.lossyScale.z));

            Vector3 nearestPoint = FFVector3.VecMaxValue;
            float nearestDist = float.MaxValue;

            foreach (Vector3 point in points)
            {
                float distToPoint = (point - givenPoint).sqrMagnitude;
                if (distToPoint < nearestDist)
                {
                    nearestDist = distToPoint;
                    nearestPoint = point;
                }
            }
            return pos + (transform.rotation * FFMatrix3X3.ScaleBy(nearestPoint, transform.lossyScale));
        }
        else
        {
            Debug.LogError("NearestPoint failed to setup");
            return new Vector3(0,0,0);
        }
    }

    /// <summary>
    /// returns the nearest point to a distance along the path
    /// TODO: Mod Needed in end?
    /// </summary>
    public Vector3 NearestPoint(float distanceAlongPath)
    {
        float distmod = distanceAlongPath % PathLength;
        float distNegEqualZero = (distmod + PathLength) % PathLength;
        float distPos = distmod > 0 ? distPos = distmod : distPos = PathLength;

        if (distanceAlongPath <= 0) // given negative/zero distance
            distanceAlongPath = distNegEqualZero;
        else   // given positive distance
            distanceAlongPath = distPos;

        Vector3 pos; // TODO: not sure if this is an edge case or not
        if (transform != null) { pos = transform.position; }
        else { pos = new Vector3(0, 0, 0); }



        if ((!DynamicPath && IsValidPath()) || (SetupPointData() && IsValidPath()))
        {
            int i = 0;
            int first = 1;
            int middle = PointCount / 2;
            int last = PointCount - 1;


            while (first <= last)
            {
                if (distanceAlongPath > (linearDistanceAlongPath[middle])) // greater than
                {
                    first = middle + 1;
                }
                else if (distanceAlongPath >= (linearDistanceAlongPath[middle - 1]) // equal to
                    && distanceAlongPath <= (linearDistanceAlongPath[middle]))
                {
                    i = middle;
                    break;
                }
                else // less than (dist < linearDistanceAlongPath[middle - 1])
                {
                    last = middle - 1;
                }

                middle = (first + last) / 2;
            }
            distanceAlongPath -= linearDistanceAlongPath[i - 1];
            float halfLengthBetweenPoints = (linearDistanceAlongPath[i] - linearDistanceAlongPath[i - 1])/2;
            if (distanceAlongPath > halfLengthBetweenPoints)
                return pos + (transform.rotation * FFMatrix3X3.ScaleBy(points[i % points.Length], transform.lossyScale)); // if we are more than halfway through line
            else
                return pos + (transform.rotation * FFMatrix3X3.ScaleBy(points[i - 1], transform.lossyScale));  // if we less than or equal to than halfway through line
        }
        Debug.LogError("Error, Path failed to setup");
        return new FFVector3(0, 0, 0);
    }

    /// <summary>
    /// returns the next point in the Path
    /// </summary>
    public Vector3 NextPoint(float distanceAlongPath)
    {
        float distmod = distanceAlongPath % PathLength;
        float distNegEqualZero = (distmod + PathLength) % PathLength;
        float distPos = distmod > 0 ? distPos = distmod : distPos = PathLength;

        if (distanceAlongPath <= 0) // given negative/zero distance
            distanceAlongPath = distNegEqualZero;
        else   // given positive distance
            distanceAlongPath = distPos;

        Vector3 pos; // TODO: not sure if this is an edge case or not
        if (transform != null) { pos = transform.position; }
        else { pos = new Vector3(0, 0, 0); }


        if ((!DynamicPath && IsValidPath()) || (SetupPointData() && IsValidPath()))
        {
            int i = 0;
            int first = 1;
            int middle = PointCount / 2;
            int last = PointCount - 1;

            // do not mod unless >, so that we can move to the end of the path
            if (distanceAlongPath > PathLength)
                distanceAlongPath = distanceAlongPath % PathLength;

            while (first <= last)
            {
                if (distanceAlongPath > (linearDistanceAlongPath[middle])) // greater than
                {
                    first = middle + 1;
                }
                else if (distanceAlongPath >= (linearDistanceAlongPath[middle - 1]) // equal to
                    && distanceAlongPath <= (linearDistanceAlongPath[middle]))
                {
                    i = middle;
                    break;
                }
                else // less than (dist < linearDistanceAlongPath[middle - 1])
                {
                    last = middle - 1;
                }

                middle = (first + last) / 2;
            }
            return pos + (transform.rotation * FFMatrix3X3.ScaleBy(points[i % points.Length], transform.lossyScale));
        }

        Debug.LogError("Error, Path failed to setup");
        return new FFVector3(0, 0, 0);
    }
    
    /// <summary>
    /// Returns the previous point on the path relative to the 
    /// given distance along the path
    /// </summary>
    public Vector3 PrevPoint(float distanceAlongPath)
    {
        float distmod = distanceAlongPath % PathLength;
        float distNegEqualZero = (distmod + PathLength) % PathLength;
        float distPos = distmod > 0 ? distPos = distmod : distPos = PathLength;

        if (distanceAlongPath <= 0) // given negative/zero distance
            distanceAlongPath = distNegEqualZero;
        else   // given positive distance
            distanceAlongPath = distPos;

        Vector3 pos; // TODO: not sure if this is an edge case or not
        if (transform != null) { pos = transform.position; }
        else { pos = new Vector3(0, 0, 0); }

        if ((!DynamicPath && IsValidPath()) || (SetupPointData() && IsValidPath()))
        {
            int i = 0;
            int first = 1;
            int middle = PointCount / 2;
            int last = PointCount - 1;

            // do not mod unless >, so that we can move to the end of the path
            if (distanceAlongPath > PathLength)
                distanceAlongPath = distanceAlongPath % PathLength;

            while (first <= last)
            {
                if (distanceAlongPath > (linearDistanceAlongPath[middle])) // greater than
                {
                    first = middle + 1;
                }
                else if (distanceAlongPath >= (linearDistanceAlongPath[middle - 1]) // equal to
                    && distanceAlongPath <= (linearDistanceAlongPath[middle]))
                {
                    i = middle;
                    break;
                }
                else // less than (dist < linearDistanceAlongPath[middle - 1])
                {
                    last = middle - 1;
                }

                middle = (first + last) / 2;
            }
            return pos + (transform.rotation * FFMatrix3X3.ScaleBy(points[i - 1], transform.lossyScale));
        }

        Debug.LogError("Error, Path failed to setup");
        return new FFVector3(0, 0, 0);
    }

    /// <summary>
    /// Returns a random point along the path
    /// </summary>
    public Vector3 RandomPoint()
    {
        return PointAlongPath(UnityEngine.Random.Range(0, PathLength));
    }
#endregion CodeInterface

#region InterpolationMethods
    private void GetData(float dist, out float mu, out FFVector3 n1, out FFVector3 p1)
    {
        dist = Mathf.Abs(dist);
        if (points.Length > 1)
        {
            int i = 0;
            int first = 1;
            int middle = PointCount / 2;
            int last = PointCount - 1;

            // do not mod unless >, so that we can move to the end of the path
            if (dist > PathLength)
                dist = dist % PathLength;

            while (first <= last)
            {
                if (dist > (linearDistanceAlongPath[middle])) // greater than
                {
                    first = middle + 1;
                }
                else if (dist >= (linearDistanceAlongPath[middle - 1]) // equal to
                    && dist <= (linearDistanceAlongPath[middle]))
                {
                    i = middle;
                    break;
                }
                else // less than (dist < linearDistanceAlongPath[middle - 1])
                {
                    last = middle - 1;
                }

                middle = (first + last) / 2;
            }

            int n1Index = i - 1; // TODO remove?
            int p1Index = i % points.Length;

            n1.x = points[n1Index].x;
            n1.y = points[n1Index].y;
            n1.z = points[n1Index].z;

            p1.x = points[p1Index].x;
            p1.y = points[p1Index].y;
            p1.z = points[p1Index].z;

            float lengthBetweenPoints = linearDistanceAlongPath[i] - linearDistanceAlongPath[i - 1];
            if(lengthBetweenPoints > 0.0f)
            {
                mu = (dist - linearDistanceAlongPath[i - 1]) // dist's length into Interval of points
                    / (lengthBetweenPoints); // length of interval between points
            }
            else // zero distance between points
            {
                mu = 0.0f;
            }
            return;
        }

        p1.x = -0;
        p1.y = -0;
        p1.z = -0;
        n1 = p1;
        mu = 0;
    }
    private void GetData(float dist, out float mu, out FFVector3 n1, out FFVector3 n2, out FFVector3 p1, out FFVector3 p2)
    {
        dist = Mathf.Abs(dist);
        if (points.Length > 1)
        {
            int i = 0;
            int first = 1;
            int middle = PointCount / 2;
            int last = PointCount - 1;

            // do not mod unless >, so that we can move to the end of the path
            if (dist > PathLength)
                dist = dist % PathLength;

            while (first <= last)
            {
                if (dist > (linearDistanceAlongPath[middle])) // greater than
                {
                    first = middle + 1;
                }
                else if (dist >= (linearDistanceAlongPath[middle - 1]) // equal to
                    && dist <= (linearDistanceAlongPath[middle]))
                {
                    i = middle;
                    break;
                }
                else // less than (dist < linearDistanceAlongPath[middle - 1])
                {
                    last = middle - 1;
                }

                middle = (first + last) / 2;
            }




            if (Circuit) // line loops back to first point from the last point
            {
                int n2Index = i - 2 < 0 ? points.Length - 1 : i - 2;
                int n1Index = (i - 1) % points.Length;
                int p1Index = i % points.Length;
                int p2Index = (i + 1) % points.Length;

                n2.x = points[n2Index].x;
                n2.y = points[n2Index].y;
                n2.z = points[n2Index].z;

                n1.x = points[n1Index].x;
                n1.y = points[n1Index].y;
                n1.z = points[n1Index].z;

                p1.x = points[p1Index].x;
                p1.y = points[p1Index].y;
                p1.z = points[p1Index].z;

                p2.x = points[p2Index].x;
                p2.y = points[p2Index].y;
                p2.z = points[p2Index].z;


                mu = (dist - linearDistanceAlongPath[i - 1]) // dist's length into Interval of points
                    / (linearDistanceAlongPath[i] - linearDistanceAlongPath[i - 1]); // length of interval between points

                return;
            }
            else   // Line ends and then begins again...
            {
                int n2Index = i - 2 < 0 ? points.Length - 1 : i - 2;
                int n1Index = (i - 1);
                int p1Index = i;
                int p2Index = (i + 1) % points.Length;

                n2.x = points[n2Index].x;
                n2.y = points[n2Index].y;
                n2.z = points[n2Index].z;

                n1.x = points[n1Index].x;
                n1.y = points[n1Index].y;
                n1.z = points[n1Index].z;

                p1.x = points[p1Index].x;
                p1.y = points[p1Index].y;
                p1.z = points[p1Index].z;

                p2.x = points[p2Index].x;
                p2.y = points[p2Index].y;
                p2.z = points[p2Index].z;


                float lengthBetweenPoints = linearDistanceAlongPath[i] - linearDistanceAlongPath[i - 1];
                if (lengthBetweenPoints > 0.0f)
                {
                    mu = (dist - linearDistanceAlongPath[i - 1]) // dist's length into Interval of points
                        / (lengthBetweenPoints); // length of interval between points
                }
                else // zero distance between points
                {
                    mu = 0.0f;
                }
                return;
                        
            }
        }

        p1.x = -0.1f;
        p1.y = -0.1f;
        p1.z = -0.1f;
        n1 = n2 = p2 = p1;
        mu = 0;
    }
        
    private Vector3 InterpolateLinearPositionAlongPath(float dist)
    {
        float mu;
        FFVector3 n1;
        FFVector3 p1;
        GetData(dist, out mu, out n1, out p1);
        //PrintData(dist, mu, n1, p1);
            
        return new Vector3(n1.x * (1 - mu) + p1.x * mu,
                            n1.y * (1 - mu) + p1.y * mu,
                            n1.z * (1 - mu) + p1.z * mu);
    }
    private Vector3 InterpolateLinearSmoothPositionAlongPath(float dist)
    {
        float mu;
        float mu2;
        FFVector3 n1;
        FFVector3 p1;
        GetData(dist, out mu, out n1, out p1);
        //PrintData(dist, mu, n1, p1);

        // Smoothing
        // f(x) = -(x)^2 (x -1.5) * 2;
        mu2 = -(mu * mu) * (mu - 1.5f) * 2;

        return new Vector3(n1.x * (1 - mu2) + p1.x * mu2,
                            n1.y * (1 - mu2) + p1.y * mu2,
                            n1.z * (1 - mu2) + p1.z * mu2);
    }
    private Vector3 InterpolateCatmullRomPositionAlongPath(float dist)
    {
        float mu;
        FFVector3 n2, n1, p1, p2;
        GetData(dist, out mu, out n1, out n2, out p1, out p2);
        //PrintData(dist, mu, n1, n2, p1, p2);

        //Catmull-Rom Splines
#region Catmull-Rom Splines

        FFVector3 a0, a1, a2, a3;
        float mu2;

        mu2 = mu * mu;
        a0.x = (-0.5f * n2.x) + (1.5f * n1.x) + (-1.5f * p1.x) + (0.5f * p2.x);
        a0.y = (-0.5f * n2.y) + (1.5f * n1.y) + (-1.5f * p1.y) + (0.5f * p2.y);
        a0.z = (-0.5f * n2.z) + (1.5f * n1.z) + (-1.5f * p1.z) + (0.5f * p2.z);

        a1.x = (n2.x) + (-2.5f * n1.x) + (2.0f * p1.x) + (-0.5f * p2.x);
        a1.y = (n2.y) + (-2.5f * n1.y) + (2.0f * p1.y) + (-0.5f * p2.y);
        a1.z = (n2.z) + (-2.5f * n1.z) + (2.0f * p1.z) + (-0.5f * p2.z);

        a2.x = (-0.5f * n2.x) + (0.5f * p1.x);
        a2.y = (-0.5f * n2.y) + (0.5f * p1.y);
        a2.z = (-0.5f * n2.z) + (0.5f * p1.z);

        a3.x = n1.x;
        a3.y = n1.y;
        a3.z = n1.z;

        return new Vector3((a0.x * mu * mu2) + (a1.x * mu2) + (a2.x * mu) + (a3.x),
                            (a0.y * mu * mu2) + (a1.y * mu2) + (a2.y * mu) + (a3.y),
                            (a0.z * mu * mu2) + (a1.z * mu2) + (a2.z * mu) + (a3.z));
#endregion Catmull-Rom Splines

        // Cubic
#region CubicOld
        /*
        float mu2;
        float a0x, a0y, a0z;
        float a1x, a1y, a1z;
        float a2x, a2y, a2z;
        float a3x, a3y, a3z;
            
        mu2 = mu * mu;
        //
        a0x = p2.x - p1.x - n1.x + n2.x;
        a0y = p2.y - p1.y - n1.y + n2.y;
        a0z = p2.z - p1.z - n1.z + n2.z;
        //
        a1x = n2.x - n1.x - p1.x;
        a1y = n2.y - n1.y - p1.y;
        a1z = n2.z - n1.z - p1.z;
        //
        a2x = p1.x - n2.x;
        a2y = p1.y - n2.y;
        a2z = p1.z - n2.z;
        //
        a3x = n1.x;
        a3y = n1.y;
        a3z = n1.z;

        return new Vector3((a0x * mu * mu2) + (a1x * mu2) + (a2x + mu) + (a3x),
                            (a0y * mu * mu2) + (a1y * mu2) + (a2y + mu) + (a3y),
                            (a0z * mu * mu2) + (a1z * mu2) + (a2z + mu) + (a3z));
            */
#endregion CubicOld
    }
    private Vector3 InterpolateCatmullRomSmoothPositionAlongPath(float dist)
    {
        float mu;
        FFVector3 n2, n1, p1, p2;
        GetData(dist, out mu, out n1, out n2, out p1, out p2);

        mu = -(mu * mu) * (mu - 1.5f) * 2;
        //PrintData(dist, mu, n1, n2, p1, p2);

        //Catmull-Rom Splines
#region Catmull-Rom Splines

        FFVector3 a0, a1, a2, a3;
        float mu2;

        mu2 = mu * mu;
        a0.x = (-0.5f * n2.x) + (1.5f * n1.x) + (-1.5f * p1.x) + (0.5f * p2.x);
        a0.y = (-0.5f * n2.y) + (1.5f * n1.y) + (-1.5f * p1.y) + (0.5f * p2.y);
        a0.z = (-0.5f * n2.z) + (1.5f * n1.z) + (-1.5f * p1.z) + (0.5f * p2.z);

        a1.x = (n2.x) + (-2.5f * n1.x) + (2.0f * p1.x) + (-0.5f * p2.x);
        a1.y = (n2.y) + (-2.5f * n1.y) + (2.0f * p1.y) + (-0.5f * p2.y);
        a1.z = (n2.z) + (-2.5f * n1.z) + (2.0f * p1.z) + (-0.5f * p2.z);

        a2.x = (-0.5f * n2.x) + (0.5f * p1.x);
        a2.y = (-0.5f * n2.y) + (0.5f * p1.y);
        a2.z = (-0.5f * n2.z) + (0.5f * p1.z);

        a3.x = n1.x;
        a3.y = n1.y;
        a3.z = n1.z;

        return new Vector3((a0.x * mu * mu2) + (a1.x * mu2) + (a2.x * mu) + (a3.x),
                            (a0.y * mu * mu2) + (a1.y * mu2) + (a2.y * mu) + (a3.y),
                            (a0.z * mu * mu2) + (a1.z * mu2) + (a2.z * mu) + (a3.z));
#endregion Catmull-Rom Splines

        // Cubic
#region CubicOld
        /*
        float mu2;
        float a0x, a0y, a0z;
        float a1x, a1y, a1z;
        float a2x, a2y, a2z;
        float a3x, a3y, a3z;
            
        mu2 = mu * mu;
        //
        a0x = p2.x - p1.x - n1.x + n2.x;
        a0y = p2.y - p1.y - n1.y + n2.y;
        a0z = p2.z - p1.z - n1.z + n2.z;
        //
        a1x = n2.x - n1.x - p1.x;
        a1y = n2.y - n1.y - p1.y;
        a1z = n2.z - n1.z - p1.z;
        //
        a2x = p1.x - n2.x;
        a2y = p1.y - n2.y;
        a2z = p1.z - n2.z;
        //
        a3x = n1.x;
        a3y = n1.y;
        a3z = n1.z;

        return new Vector3((a0x * mu * mu2) + (a1x * mu2) + (a2x + mu) + (a3x),
                            (a0y * mu * mu2) + (a1y * mu2) + (a2y + mu) + (a3y),
                            (a0z * mu * mu2) + (a1z * mu2) + (a2z + mu) + (a3z));
            */
#endregion CubicOld
    }

    //Debug print for GetData
    private void PrintData(float dist, float mu, FFVector3 n1, FFVector3 p1)
    {
        Debug.Log("Dist: " + dist);
        Debug.Log("mu: " + mu);
        Debug.Log("n1: " + "(" + n1.x + "," + n1.y + "," + n1.z + ")");
        Debug.Log("p1: " + "(" + p1.x + "," + p1.y + "," + p1.z + ")");
    }
    //Debug print for GetData
    private void PrintData(float dist, float mu, FFVector3 n1, FFVector3 n2, FFVector3 p1, FFVector3 p2)
    {
        Debug.Log("Dist: " + dist);
        Debug.Log("mu: " + mu);
        Debug.Log("n2: " + "(" + n2.x + "," + n2.y + "," + n2.z + ")");
        Debug.Log("n1: " + "(" + n1.x + "," + n1.y + "," + n1.z + ")");
        Debug.Log("p1: " + "(" + p1.x + "," + p1.y + "," + p1.z + ")");
        Debug.Log("p2: " + "(" + p2.x + "," + p2.y + "," + p2.z + ")");
    }
#endregion InterpolationMethods
}