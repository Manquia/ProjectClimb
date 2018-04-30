using UnityEngine;
using System.Collections;

//////////////////////////////////////////////////////
// Author: Micah Rust
// Data: 29/6/2015
// Purpose: A Base component to make using FFAction,
//      any other features you might
//      impliment easier to use. Basically is a base
//      component which has extended functionality
//      over Monobehavior. It is not required but is
//      very nice since inline-lamdas are quite complex
//      in C#.
//
///////////////////////////////////////////////////////

// Base component to add a bunch of getter/setters for
// misc uses
public class FFComponent : MonoBehaviour
{
    public FFAction action
    {
        get { return gameObject.GetOrAddComponent<FFAction>(); }
    }

    public FFRef<Vector3> ffposition
    {
        get { return new FFRef<Vector3>(() => transform.position, (v) => { transform.position = v; }); }
    }
    public FFRef<Vector3> ffrotation
    {
        get { return new FFRef<Vector3>(() => transform.eulerAngles, (v) => { transform.rotation = Quaternion.Euler(v); }); }
    }
    public FFRef<Vector3> ffscale
    {
        get { return new FFRef<Vector3>(() => transform.localScale, (v) => { transform.localScale = v; }); }
    }
    public FFRef<Color> ffSpriteColor
    {
        get { return new FFRef<Color>(() => GetComponent<SpriteRenderer>().color, (v) => { GetComponent<SpriteRenderer>().color = v; }); }
    }

}
