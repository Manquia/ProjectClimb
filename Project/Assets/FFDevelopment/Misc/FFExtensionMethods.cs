using UnityEngine;
using System.Collections;
using System.Reflection;
using System;

static class FFExtensionMethods
{
    // ExtensionMethods
    public static Color MakeClear(this Color _color)
    {
        return new Color(_color.r, _color.g, _color.b, 0.0f);
    }
    public static Color MakeOpaque(this Color _color)
    {
        return new Color(_color.r, _color.g, _color.b, 1.0f);

    }


    #region Gameobject
    public static void Destroy(this GameObject _go)
    {
        FFSystem.UnRegisterNetGameObject(_go, true);
    }
    public static bool isOwnedNetObject(this GameObject _go)
    {
        return FFSystem.OwnGameObject(_go);
    }
    public static T GetOrAddComponent<T>(this GameObject _go) where T : Component
    {
        T comp = _go.GetComponent<T>();
        if (comp != null)
            return comp;
        else
            return _go.AddComponent<T>();
    }
    #endregion
    
    #region Component

    public static T GetOrAddComponent<T>(this Component _mono) where T : Component
    {
        T comp = _mono.GetComponent<T>();
        if (comp != null)
            return comp;
        else
            return _mono.gameObject.AddComponent<T>();
    }
    // Componenet References
    public static FFAction action(this Component monoBehavior)
    {
        return monoBehavior.gameObject.GetOrAddComponent<FFAction>();
    }

    // Add FFRef creators functions below
    public static FFRef<Vector3> ffposition(this Component monoBehavior)
    {
        return new FFRef<Vector3>(() => monoBehavior.transform.position,
            (v) => { monoBehavior.transform.position = v; });
    }
    // @ TODO lerp for rotation for ffrotation
    public static FFRef<Vector3> ffscale(this Component monoBehavior)
    {
        return new FFRef<Vector3>(() => monoBehavior.transform.localScale, (v) => { monoBehavior.transform.localScale = v; });
    }
    public static FFRef<Color> ffSpriteColor(this Component _trans)
    {
        return new FFRef<Color>(() => _trans.GetComponent<SpriteRenderer>().color, (v) => { _trans.GetComponent<SpriteRenderer>().color = v; });
    }
    public static FFRef<Color> ffTextMeshColor(this Component _trans)
    {
        return new FFRef<Color>(() => _trans.GetComponent<TextMesh>().color, (v) => { _trans.GetComponent<TextMesh>().color = v; });
    }
    #endregion Component
    
    #region AnimationCurve
    public static float TimeToComplete(this AnimationCurve _curve)
    {
        if (_curve == null) throw new ArgumentNullException("_curve");
        else if (_curve.length < 1) { throw new Exception("_curve has less than 2 key points"); }
        else return _curve.keys[_curve.length - 1].time - _curve.keys[0].time;
    }


    #endregion

    #region Camera

    // Add FFRef creators functions below
    public static FFRef<float> fffieldofview(this Camera cam)
    {
        return new FFRef<float>(() => cam.fieldOfView,
            (v) => { cam.fieldOfView = v; });
    }
    #endregion
}
