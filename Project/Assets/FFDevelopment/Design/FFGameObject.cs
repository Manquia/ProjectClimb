using UnityEngine;
using System.Collections;
using System;


//////////////////////////////////////////////////////
// Author: Micah Rust
// Data: 29/6/2015
// Purpose: Wrapper with data which is used by all
//      FFComponents, likely better to just use GameObject
//      in the case whereby you want a reference since that
//      will be null upon the destruction of the game object
//      whereas this won't if the GameObject is destroy.
//      If you do use FFGameObject wrapper, use its
//      Destroy functions which are FFActionSystem compatable over
//      GameObject.Destroy.
//
///////////////////////////////////////////////////////

/// CLOSED, TODO BEFORE LAUNCH: DELETE
/*
public class FFGameObject
{
    private GameObject _go;
    public GameObject gameobject
    {
        get { return _go; }
    }

    // Transform Property Delegates
    private FFRef<Vector3> _FFposition;
    private FFRef<Vector3> _FFrotation;
    private FFRef<Vector3> _FFScale;

    public FFRef<Vector3> FFposition
    {
        get { return _FFposition; }
    }
    public FFRef<Vector3> FFrotation
    {
        get { return _FFrotation; }
    }
    public FFRef<Vector3> FFScale
    {
        get { return _FFScale; }
    }

    // Component Getters
    public FFAction Action
    {
        get
        {
            FFAction ffAction = _go.GetComponent<FFAction>();
            if(ffAction != null)
            {
                return ffAction;
            }
            else
            {
                return ffAction = _go.AddComponent<FFAction>();
            }
        }
    }
    public FFPath Path
    {
        get
        {
            
            FFPath ffPath = _go.GetComponent<FFPath>();
            if(ffPath != null)
            {
                return ffPath;
            }
            else
            {
                return ffPath = _go.AddComponent<FFPath>();
            }
        }
    }

    // Misc
    /// <summary>
    /// Registered object must be destroyed via this function
    /// </summary>
    public void Destroy()
    {
        FFSystem.UnRegisterNetGameObject(_go, true);
    }
    public void Destroy(float timeTillDestroy)
    {
        var seq = Action.Sequence();
        seq.Delay(timeTillDestroy);
        seq.Sync();
        seq.Call(Destroy);
    }
    public static void Destroy(object ffgo)
    {
        FFGameObject.Destroy((FFGameObject)ffgo);
    }
    public static void Destroy(FFGameObject ffgo)
    {
        ffgo.Destroy();
    }
    public static void Destroy(object ffgo, float timeTillDestroy)
    {
        FFGameObject.Destroy((FFGameObject)ffgo, timeTillDestroy);
    }
    public static void Destroy(FFGameObject ffgo, float timeTillDestroy)
    {
        ffgo.Destroy(timeTillDestroy);
    }

    
    public T GetOrAddComponent<T>() where T : Component
    {
        T comp = _go.gameObject.GetComponent<T>();
        if (comp != null)
            return comp;
        else
            return _go.gameObject.AddComponent<T>();
    }

    // Constructors/setup
    public FFGameObject()
    {
        _go = new GameObject();
        InitFFGameObject();
    }
    public FFGameObject(GameObject go)
    {
        if(go != null)
        {
            _go = go;
        }
        else
        {
            throw new ArgumentNullException("go");
        }
        InitFFGameObject();
    }
    public FFGameObject(string name)
    {
        _go = new GameObject(name);
        InitFFGameObject();
    }
    public FFGameObject(string name, Type[] components)
    {
        _go = new GameObject(name, components);
        InitFFGameObject();
    }
    
    private void InitFFGameObject()
    {
        SetPropertyDelegates();
    }
    private void SetPropertyDelegates()
    {
        _FFposition = new FFRef<Vector3>(() => _go.transform.position, (v) => { _go.transform.position = v; });
        _FFrotation = new FFRef<Vector3>(() => _go.transform.eulerAngles, (v) => { _go.transform.rotation = Quaternion.Euler(v); });
        _FFScale = new FFRef<Vector3>(() => _go.transform.localScale, (v) => { _go.transform.localScale = v; });
    }
}
*/
