
using UnityEngine;

public enum Trinary
{
    True,
    False,
    Null,
}

public class Annal<Type>
{
    public delegate Type constructor();
    private Type[] data;
    // Index of the most recent record
    public int index { get; private set; }
    // Total Records in the Annal
    public int size { get { return data.Length; } }
    // Construct an Annal with a size
    public Annal(uint annalSize, Type defaultValue)
    {
        data = new Type[annalSize];
        index = 0;
        for (int i = 0; i < annalSize; ++i) data[i] = defaultValue;
    }

    public Annal(uint annalSize, constructor constructor)
    {
        data = new Type[annalSize];
        index = 0;
        for (int i = 0; i < annalSize; ++i) data[i] = constructor();
    }
    public static implicit operator Type(Annal<Type> d)
    {
        return d.data[d.index];
    }
    public void Record(Type annal)
    {
        index = ++index % size;
        data[index] = annal;
    }
    public Type Recall(uint recordOffset)
    {
        return data[(size + index - recordOffset) % size];
    }
    // Reset all history to this wash types
    public void Wash(Type value)
    {
        for (int i = 0; i < data.Length; ++i) data[i] = value;
    }
    public bool Contains(System.Predicate<Type> pred)
    {
        for (uint i = 0; i < size; ++i)
        {
            if (pred(Recall(i)))
                return true;
        }
        return false;
    }

}

public struct KeyState
{
    public static KeyState Constructor()
    {
        KeyState ks;
        ks.upState = true;
        ks.downState = false;
        return ks;
    }

    private bool upState;
    private bool downState;

    // 10 OR 00
    public bool up()
    {
        return upState || (!downState && !upState);
    }
    // 01 OR 11
    public bool down()
    {
        return downState;
    }
    // 11
    public bool pressed()
    {
        return downState && upState;
    }
    // 00
    public bool released()
    {
        return !downState && !upState;
    }
    public KeyState GetUpdatedState()
    {
        KeyState newKs;
        
        if(this.pressed())
        {
            newKs.upState = false;
            newKs.downState = true;
        }
        else if(this.released())
        {
            newKs.upState = true;
            newKs.downState = false;
        }
        else
        {
            newKs = this;
        }

        return newKs;
    }
    
    static public KeyState GetKeyState(KeyCode code) { bool down = Input.GetKey(code);  KeyState ks; ks.upState = !down; ks.downState = down; return ks; }
    static public KeyState GetUpKeyState()       { KeyState ks; ks.upState = true; ks.downState = false;  return ks;}
    static public KeyState GetDownKeyState()     { KeyState ks; ks.upState = false; ks.downState = true;  return ks;}
    static public KeyState GetPressedKeyState()  { KeyState ks; ks.upState = true; ks.downState = true;   return ks;}
    static public KeyState GetReleasedKeyState() { KeyState ks; ks.upState = false; ks.downState = false; return ks;}
}