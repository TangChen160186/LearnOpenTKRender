namespace LearnOpenTKRender.OpenGL;

public enum ShaderDataType
{
    None = 0,
    Float, Float2, Float3, Float4,
    Int, Int2, Int3, Int4,
    Bool
}

public struct BufferElement
{
    public ShaderDataType Type;
    public int Size;
    public bool Normalized;

    public BufferElement(ShaderDataType type, bool normalized = false)
    {
        Type = type;
        Size = GetShaderDataTypeSize(type);
        Normalized = normalized;
    }

    public int ComponentCount => GetComponentCount(Type);

    private static int GetComponentCount(ShaderDataType type)
    {
        return type switch
        {
            ShaderDataType.Float => 1,
            ShaderDataType.Float2 => 2,
            ShaderDataType.Float3 => 3,
            ShaderDataType.Float4 => 4,
            ShaderDataType.Int => 1,
            ShaderDataType.Int2 => 2,
            ShaderDataType.Int3 => 3,
            ShaderDataType.Int4 => 4,
            ShaderDataType.Bool => 1,
            _ => throw new ArgumentException("Unknown ShaderDataType")
        };
    }

    private static int GetShaderDataTypeSize(ShaderDataType type)
    {
        return type switch
        {
            ShaderDataType.Float => 4,
            ShaderDataType.Float2 => 4 * 2,
            ShaderDataType.Float3 => 4 * 3,
            ShaderDataType.Float4 => 4 * 4,
            ShaderDataType.Int => 4,
            ShaderDataType.Int2 => 4 * 2,
            ShaderDataType.Int3 => 4 * 3,
            ShaderDataType.Int4 => 4 * 4,
            ShaderDataType.Bool => 1,
            _ => throw new ArgumentException("Unknown ShaderDataType")
        };
    }
}

public class BufferLayout
{
    private List<BufferElement> _elements = new();
    private int _stride = 0;

    public BufferLayout() { }

    public BufferLayout(List<BufferElement> elements)
    {
        _elements = elements;
        CalculateOffsetsAndStride();
    }

    public int Stride => _stride;
    public List<BufferElement> Elements => _elements;

    public void AddElement(BufferElement element)
    {
        _elements.Add(element);
        CalculateOffsetsAndStride();
    }

    private void CalculateOffsetsAndStride()
    {
        _stride = 0;
        for (int i = 0; i < _elements.Count; i++)
        {
            _stride += _elements[i].Size;
        }
    }
}