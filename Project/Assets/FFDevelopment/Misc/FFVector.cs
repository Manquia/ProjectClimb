using UnityEngine;
using System.Collections;
using System;

//////////////////////////////////////////////////////
// Author: Micah Rust
// Data: 29/6/2015
// Purpose: structs which represent (data wise) Unity
// types but can be serialized for network purposes.
///////////////////////////////////////////////////////


// Created to avoid the data limiations of Class Vector4/Quaternion/Color/Color32
// for networking, non-dynamic memory allocation
[Serializable]
public struct FFVector4
{
    public float x, y, z, w;
    
    // Constructors
    public FFVector4(float _x, float _y, float _z, float _w)
    {
        this.x = _x;
        this.y = _y;
        this.z = _z;
        this.w = _w;
    }
    public FFVector4(Vector4 vec)
    {
        this.x = vec.x;
        this.y = vec.y;
        this.z = vec.z;
        this.w = vec.w;
    }
    public FFVector4(Quaternion quat)
    {
        this.x = quat.x;
        this.y = quat.y;
        this.z = quat.z;
        this.w = quat.w;
    }
    public FFVector4(Color color)
    {
        this.x = color.r;
        this.y = color.g;
        this.z = color.b;
        this.w = color.a;
    }
    public FFVector4(Color32 color32)
    {
        this.x = color32.r;
        this.y = color32.g;
        this.z = color32.b;
        this.w = color32.a;
    }

    // implicit casts
    static public implicit operator FFVector4(Vector4 vec)
    {
        return new FFVector4(vec);
    }
    static public implicit operator FFVector4(Quaternion quat)
    {
        return new FFVector4(quat);
    }
    static public implicit operator FFVector4(Color color)
    {
        return new FFVector4(color);
    }
    static public implicit operator FFVector4(Color32 color32)
    {
        return new FFVector4(color32);
    }

    static public implicit operator Vector4(FFVector4 vec)
    {
        return new Vector4(vec.x, vec.y, vec.z, vec.w);
    }
    static public implicit operator Quaternion(FFVector4 vec)
    {
        return new Quaternion(vec.x, vec.y, vec.z, vec.w);
    }
    static public implicit operator Color(FFVector4 vec)
    {
        return new Color(vec.x, vec.y, vec.z, vec.w);
    }
    static public implicit operator Color32(FFVector4 vec)
    {
        return new Color32((byte)vec.x, (byte)vec.y, (byte)vec.z, (byte)vec.w);
    }


    // Misc
    static public Vector4 VecMaxValue
    {
        get { return new Vector4(float.MaxValue, float.MaxValue, float.MaxValue, float.MaxValue); }
    }
    static public FFVector4 FFVecMaxValue
    {
        get { return new FFVector4(float.MaxValue, float.MaxValue, float.MaxValue, float.MaxValue); }
    }
}

// Created to avoid the data limiations of Class Vector3
// for networking, non-dynamic memory allocation
[Serializable]
public struct FFVector3
{
    public float x, y, z;

    // Constructors
    public FFVector3(float _x, float _y, float _z)
    {
        this.x = _x;
        this.y = _y;
        this.z = _z;
    }
    public FFVector3(Vector3 vec)
    {
        this.x = vec.x;
        this.y = vec.y;
        this.z = vec.z;
    }

    // Implicit casts
    static public implicit operator FFVector3(Vector3 vec)
    {
        return new FFVector3(vec);
    }
    static public implicit operator Vector3(FFVector3 vec)
    {
        return new Vector3(vec.x, vec.y, vec.z);
    }

    // Misc
    static public Vector3 VecMaxValue
    {
        get { return new Vector3(float.MaxValue, float.MaxValue, float.MaxValue); }
    }
    static public FFVector3 FFVecMaxValue
    {
        get { return new FFVector3(float.MaxValue, float.MaxValue, float.MaxValue); }
    }
}

// Created to avoid the data limiations of Class Vector2
// for networking, non-dynamic memory allocation
[Serializable]
public struct FFVector2
{
    public float x, y;

    // Constructors
    public FFVector2(float _x, float _y)
    {
        this.x = _x;
        this.y = _y;
    }
    public FFVector2(Vector2 vec)
    {
        this.x = vec.x;
        this.y = vec.y;
    }

    // casts
    static public implicit operator FFVector2(Vector2 vec)
    {
        return new FFVector2(vec);
    }
    static public implicit operator Vector2(FFVector2 vec)
    {
        return new Vector2(vec.x, vec.y);
    }


    // Misc
    static public Vector2 VecMaxValue
    {
        get { return new Vector2(float.MaxValue, float.MaxValue); }
    }
    static public FFVector2 FFVecMaxValue
    {
        get { return new FFVector2(float.MaxValue, float.MaxValue); }
    }
}
