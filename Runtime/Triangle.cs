using Unity.Mathematics;

public struct Triangle
{
    public int3 indices;

    public Triangle(int3 indices) => this.indices = indices;

    public Triangle(int a, int b, int c) => this.indices = new int3(a, b, c);

    public Triangle(int s) => this.indices = new int3(s, s + 1, s + 2);

    public int this[int index]
    {
        get => indices[index];
        set => indices[index] = value;
    }

    public static Triangle operator + (Triangle lhs, int rhs)
        => new Triangle(lhs.indices + rhs);

    public static Triangle operator + (Triangle lhs, int3 rhs)
        => new Triangle(lhs.indices + rhs);

    public static Triangle operator + (Triangle lhs, Triangle rhs)
        => new Triangle(lhs.indices + rhs.indices);

    public static Triangle operator - (Triangle lhs, int rhs)
        => new Triangle(lhs.indices - rhs);

    public static Triangle operator - (Triangle lhs, int3 rhs)
        => new Triangle(lhs.indices - rhs);

    public static Triangle operator - (Triangle lhs, Triangle rhs)
        => new Triangle(lhs.indices - rhs.indices);

    public static Triangle operator * (Triangle lhs, int rhs)
        => new Triangle(lhs.indices * rhs);

    public static Triangle operator * (Triangle lhs, int3 rhs)
        => new Triangle(lhs.indices * rhs);

    public static Triangle operator * (Triangle lhs, Triangle rhs)
        => new Triangle(lhs.indices * rhs.indices);

    public static Triangle operator / (Triangle lhs, int rhs)
        => new Triangle(lhs.indices / rhs);

    public static Triangle operator / (Triangle lhs, int3 rhs)
        => new Triangle(lhs.indices / rhs);
        
    public static Triangle operator / (Triangle lhs, Triangle rhs)
        => new Triangle(lhs.indices / rhs.indices);
}
