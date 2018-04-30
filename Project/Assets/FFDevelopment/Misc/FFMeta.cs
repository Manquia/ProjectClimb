using UnityEngine;
using System.Collections;
using System;

//////////////////////////////////////////////////////
// Author: Micah Rust
// Data: 10/9/2015
// Purpose: FFMeta is used by FFAction but could be
//      used to get the same functionality as
//      value reference in C++ with the added
//      functionality of code routines in getter/setters.
//
// Usage: FFRef for class/value references, and FFVar for
//      any variable you want to come with a reference by
//      default.
//
///////////////////////////////////////////////////////

// "value references"/Property Delegates in C#
// where f is a variable (float in this case)
// FFRef<float> myRef = new FFRef<float>(() => f, v => { f = v; });

[Serializable]
public class FFRef<Type>
{
    public delegate Type del_get();
    public delegate void del_set(Type obj);

    static public implicit operator Type(FFRef<Type> ffref)
    {
        return ffref.Val;
    }

    protected del_get mGetter;
    protected del_set mSetter;
    public del_get Getter { get { return mGetter; } }
    public del_set Setter { get { return mSetter; } }
    public FFRef(del_get getter, del_set setter)
    {
        mGetter = getter;
        mSetter = setter;
    }
    protected FFRef(){}
    public Type Val
    {
        get { return mGetter(); }
        set { mSetter(value); }
    }
}

// value wrapper with default getter and setter
[Serializable]
public sealed class FFVar<Type> : FFRef<Type>
{
    private Type myvalue;

    public FFVar()
    {
        mGetter = () => myvalue;
        mSetter = v => { myvalue = v; };
    }

    // if on a class type be sure to assign the value a class
    // aka. FFVar<myclass> itemclass = new FFVar<myclass>(new myclass());
    public FFVar(Type value)
    {
        myvalue = value;
        mGetter = () => myvalue;
        mSetter = v => { myvalue = v; };
    }

    new public Type Val
    {
        get { return myvalue; }
        set { myvalue = value; }
    }
}

// Extra FFRefs for complex setters. Commented out to reduce clutter,
// however these should work just fine if the need should arise.
/*
public class FFRef<GetType, SetType>
{
    public delegate GetType del_get();
    public delegate void del_set(SetType obj);

    static public implicit operator GetType(FFRef<GetType, SetType> ffref)
    {
        return ffref.Get();
    }

    protected del_get mGetter;
    protected del_set mSetter;
    public del_get Getter { get { return mGetter; } }
    public del_set Setter { get { return mSetter; } }
    public FFRef(del_get getter, del_set setter)
    {
        mGetter = getter;
        mSetter = setter;
    }
    protected FFRef() { }
    public GetType Get()
    {
        return mGetter();
    }
    public void Set(SetType obj)
    {
        mSetter(obj);
    }
}

public class FFRef<GetType, SetType0, SetType1>
{
    public delegate GetType del_get();
    public delegate void del_set(SetType0 obj0, SetType1 obj1);

    static public implicit operator GetType(FFRef<GetType, SetType0, SetType1> ffref)
    {
        return ffref.Get();
    }

    protected del_get mGetter;
    protected del_set mSetter;
    public del_get Getter { get { return mGetter; } }
    public del_set Setter { get { return mSetter; } }
    public FFRef(del_get getter, del_set setter)
    {
        mGetter = getter;
        mSetter = setter;
    }
    protected FFRef() { }
    public GetType Get()
    {
        return mGetter(); 
    }
    public void Set(SetType0 obj0, SetType1 obj1)
    {
        mSetter(obj0, obj1);
    }
}

public class FFRef<GetType, SetType0, SetType1, SetType2>
{
    public delegate GetType del_get();
    public delegate void del_set(SetType0 obj0, SetType1 obj1, SetType2 obj2);

    static public implicit operator GetType(FFRef<GetType, SetType0, SetType1, SetType2> ffref)
    {
        return ffref.Get();
    }

    protected del_get mGetter;
    protected del_set mSetter;

    public del_get Getter { get { return mGetter; } }
    public del_set Setter { get { return mSetter; } }
    public FFRef(del_get getter, del_set setter)
    {
        mGetter = getter;
        mSetter = setter;
    }
    protected FFRef() { }
    public GetType Get()
    {
        return mGetter();
    }
    public void Set(SetType0 obj0, SetType1 obj1, SetType2 obj2)
    {
        mSetter(obj0, obj1, obj2);
    }
}
*/
