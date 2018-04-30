using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class GroundCreatorWindow : EditorWindow
{
    private bool gradientInitialized = false;
    public int seed = 123;
    [Range(16, 512)]
    public int dimensions = 128;
    [Range(0.01f, 0.8f)]
    public float noiseScale = 0.5f;
    [Range(1.0f, 50.0f)]
    public float heightDelta = 25.0f;
    public GameObject groundPrefab;
    public GameObject planePrefab;
    public string groundPrefabName;
    public string planePrefabName;

    public void SetSeed(int seed)
    {
        gradientInitialized = false;
        this.seed = seed;
    }

    [MenuItem("Tools/GroundCreator")]
    static void Init()
    {
        // Get existing open window or if none, make a new one:
        GroundCreatorWindow window = (GroundCreatorWindow)EditorWindow.GetWindow(typeof(GroundCreatorWindow));
        window.gradientInitialized = false;
        window.Show();
    }

    void OnGUI()
    {
        GUILayout.Label("Ground Creator", EditorStyles.boldLabel);

        int tempSeed = EditorGUILayout.IntField("Seed", seed);
        if (seed != tempSeed)
            SetSeed(tempSeed);

        dimensions = EditorGUILayout.IntSlider("Dimensions", dimensions, 16, 256);
        noiseScale = EditorGUILayout.Slider("Noise Scale", noiseScale, 0.01f, 0.75f);
        heightDelta = EditorGUILayout.Slider("Height Delta", heightDelta, 0.5f, 50.0f);

        string groundPrefabNameTemp = EditorGUILayout.TextField("groundPrefabPath (Resources//...)", groundPrefabName);
        if(groundPrefabNameTemp != groundPrefabName ||
            groundPrefab == null)
        {
            groundPrefabName = groundPrefabNameTemp;
            groundPrefab = FFResource.Load_Prefab(groundPrefabNameTemp);
        }

        string planePrefabNameTemp = EditorGUILayout.TextField("planePrefabPath (Resources//...)", planePrefabName);
        if (planePrefabNameTemp != planePrefabName ||
            planePrefab == null)
        {
            planePrefabName = planePrefabNameTemp;
            planePrefab = FFResource.Load_Prefab(planePrefabNameTemp);
        }

        bool createGround = false;
        createGround = EditorGUILayout.Toggle("CreateGroundButton", createGround);

        if (createGround)
            Create();
    }
    

    public void Create()
    {
        if (!gradientInitialized ||
            gradient == null)
            CalculateGradient();

        DestroyImmediate(GameObject.Find("GroundRoot"));
        Transform groundRoot = new GameObject("GroundRoot").transform;

        // Sloped with cubes
        GenerateGround(groundRoot, dimensions, noiseScale);

        //  Horizon
        CreatePlane();

        // Walls (colliders)
        CreateWalls();
    }
    

    #region Ground

    void GenerateGround(Transform parent, int dimensions, float noiseScale)
    {
        Vector2 samplePos = Vector2.zero;
        Vector3 objPosition = Vector3.zero;

        float halfGroundWidth = dimensions * 0.5f;

        for (int i = 0; i < dimensions * dimensions; ++i)
        {
            var go = Instantiate(groundPrefab);
            var trans = go.transform;

            samplePos.Set(i % dimensions, (i / dimensions));
            objPosition.Set(
                samplePos.x - halfGroundWidth,
                Perlin(samplePos.x * noiseScale, samplePos.y * noiseScale) * heightDelta,
                samplePos.y - halfGroundWidth);

            trans.SetParent(parent, false);
            trans.localPosition = objPosition;
        }
    }

    void CalculateGradient()
    {
        gradient = new Vector2[IYMAX, IXMAX];

        UnityEngine.Random.InitState(seed);

        // randomize vectors
        Vector2 value = Vector2.zero;
        for (int iy = 0; iy < IYMAX; ++iy)
        {
            for (int ix = 0; ix < IXMAX; ++ix)
            {
                value.Set(UnityEngine.Random.Range(0.0f, 3.0f),
                          UnityEngine.Random.Range(-0.2f, 0.9f));
                //value = value.normalized;

                gradient[iy, ix] = value;
            }
        }
        gradientInitialized = true; // seed has created gradient
    }

    const int IYMAX = 256;
    const int IXMAX = 256;
    Vector2[,] gradient;

    // Computes the dot product of the distance and gradient vectors.
    float DotGridGradient(int ix, int iy, float x, float y)
    {
        // Compute the distance vector
        float dx = x - (float)ix;
        float dy = y - (float)iy;

        // Compute the dot-product
        return (dx * gradient[iy, ix].x + dy * gradient[iy, ix].y);
    }

    // Compute Perlin noise at coordinates x, y
    float Perlin(float x, float y)
    {
        // Determine grid cell coordinates
        int x0 = (int)(x);
        int x1 = x0 + 1;
        int y0 = (int)(y);
        int y1 = y0 + 1;

        // Determine interpolation weights
        // Could also use higher order polynomial/s-curve here
        float sx = x - (float)x0;
        float sy = y - (float)y0;

        // Interpolate between grid point gradients
        float n0, n1, ix0, ix1, value;
        n0 = DotGridGradient(x0, y0, x, y);
        n1 = DotGridGradient(x1, y0, x, y);
        ix0 = Mathf.Lerp(n0, n1, sx);
        n0 = DotGridGradient(x0, y1, x, y);
        n1 = DotGridGradient(x1, y1, x, y);
        ix1 = Mathf.Lerp(n0, n1, sx);

        value = Mathf.Lerp(ix0, ix1, sy);

        return value;
    }

    #endregion

    #region plane
    void CreatePlane()
    {
        const int planes = 1;

        float groundWidth = dimensions;
        float groundHalfWidth = groundWidth / 2.0f;

        Transform groundRoot = GameObject.Find("GroundRoot").transform;

        Vector3 planeScale = new Vector3(1.0f, 0.001f, 1.0f);
        // Horizontal planes
        for(int j = 0; j < planes *3; ++j)
        {
            Transform plane0 = Instantiate(planePrefab).transform; // left
            Transform plane1 = Instantiate(planePrefab).transform; // right
                
            plane0.SetParent(groundRoot, false);
            plane1.SetParent(groundRoot, false);

            plane0.localPosition = new Vector3( groundWidth, 0.0f, ((planes*3) - (j+1)) * groundWidth - ((float)(planes)* groundWidth));
            plane1.localPosition = new Vector3(-groundWidth, 0.0f, ((planes*3) - (j+1)) * groundWidth - ((float)(planes)* groundWidth));

            plane0.localScale = planeScale * groundWidth;
            plane1.localScale = planeScale * groundWidth;
        }

        // vertical planes
        {
            Transform plane0 = Instantiate(planePrefab).transform; // top
            Transform plane1 = Instantiate(planePrefab).transform; // bot

            plane0.SetParent(groundRoot, false);
            plane1.SetParent(groundRoot, false);

            plane0.localPosition = new Vector3(0.0f, 0.0f,  groundWidth);
            plane1.localPosition = new Vector3(0.0f, 0.0f, -groundWidth);

            plane0.localScale = planeScale * groundWidth;
            plane1.localScale = planeScale * groundWidth;
        }
    }

    #endregion

    #region grounds

    private void CreateWalls()
    {

    }
    #endregion
}
