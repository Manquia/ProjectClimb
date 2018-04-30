//////////////////////////////////////////////////////
// Author: Micah Rust
// Data: 12/10/2015
// Purpose: FFResouce is a working concept of managing
//      Resource locations.
//      This is useful for seperating like type resources.
//      This may not be great for large scale games,
//      but should work fine for smaller <10 people game
//      projects
//
///////////////////////////////////////////////////////


using UnityEngine;
using System.Collections;

public class FFResource
{
    static public GameObject Load_Prefab(string name)
    {
        return Resources.Load<GameObject>("Prefabs/" + name);
    }
    static public Material Load_Material(string name)
    {
        return Resources.Load<Material>("Materials/" + name);
    }

    static public AudioClip Load_AudioClip(string pathName)
    {
        return Resources.Load<AudioClip>("Audio/" + pathName);
    }
}
