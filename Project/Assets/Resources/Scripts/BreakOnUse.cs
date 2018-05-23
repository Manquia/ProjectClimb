using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakOnUse : MonoBehaviour {

    public bool singleUse = true;
    bool used = false;

    public GameObject droppedObjPrefab;
    public GameObject brokenObjPrefab;
    public AudioClip breakSound;

    // effects should clean themself up!
    public GameObject[] breakEffects;
    

	// Use this for initialization
	void Start ()
    {
        FFMessageBoard<PlayerInteract.Use>.Connect(OnUse, gameObject);
    }
    private void OnDestroy()
    {
        FFMessageBoard<PlayerInteract.Use>.Disconnect(OnUse, gameObject);
    }

    private int OnUse(PlayerInteract.Use e)
    {
        if (singleUse && used)
            return 0;

        if (singleUse)
        {
            used = true;

            foreach (Transform child in transform)
            {
                child.gameObject.SetActive(false);
            }
        }

        // make it so that this object won't get hit by anything
        gameObject.layer = 0;
        if (GetComponent<Collider>() != null) Destroy(GetComponent<Collider>());

        // play audio source
        GetComponent<AudioSource>().PlayOneShot(breakSound);
        SpawnBrokenVersion();
        return 0;
    }
    
    void SpawnBrokenVersion()
    {
        // Create broken version, then parent old version to
        // new broken obj
        var brokenVer = Instantiate(brokenObjPrefab);
        brokenVer.transform.position = transform.position;
        transform.SetParent(brokenVer.transform);

        // if we drop an object, spawn it and parent to broken version
        if (droppedObjPrefab != null)
        {
            var droppedObj = Instantiate(droppedObjPrefab);
            droppedObj.transform.SetParent(brokenVer.transform, false);
            droppedObj.transform.localPosition = Vector3.zero;
        }

        foreach (var obj in breakEffects)
        {
            var effect = Instantiate(obj);
            effect.transform.position = transform.position;
        }
    }
}
