public enum Trinary
{
    True,
    False,
    Null,
}

public class Annal<Type>
{
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
}
