using UnityEngine;
using System.Collections;
using System;

[Serializable]
public struct FFMatrix3X3
{
    #region Constructors
    public FFMatrix3X3(float m00, float m01, float m02,
                       float m10, float m11, float m12,
                       float m20, float m21, float m22)
    {
        this.m00 = m00; this.m01 = m01; this.m02 = m02;
        this.m10 = m10; this.m11 = m11; this.m12 = m12;
        this.m20 = m20; this.m21 = m21; this.m22 = m22;
    }

    #endregion Constructors

    #region StaticCodeInterface

    public static FFMatrix3X3 Scale(Vector3 vec)
    {
        return new FFMatrix3X3(vec.x, 0.0f, 0.0f,
                               0.0f, vec.y, 0.0f,
                               0.0f, 0.0f, vec.z);
    }

    public static Vector3 ScaleBy(Vector3 vec, Vector3 fx_fy_fz)
    {
        return new Vector3(vec.x * fx_fy_fz.x,
                           vec.y * fx_fy_fz.y,
                           vec.z * fx_fy_fz.z);
    }
    #endregion StaticCodeInterface

    #region Data
    public float m00, m01, m02;
    public float m10, m11, m12;
    public float m20, m21, m22;
    #endregion Data
}