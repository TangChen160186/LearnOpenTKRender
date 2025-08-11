using OpenTK.Graphics.OpenGL4;

namespace LearnOpenTKRender.OpenGL;

internal class VertexArray : IDisposable
{
    private int _vertexAttribIndex = 0;
    internal int Handle;

    public VertexArray()
    {
        GL.CreateVertexArrays(1, out Handle);
    }

    public void AddVertexBuffer(VertexBuffer buffer, BufferLayout layout)
    {
        int offset = 0;
        foreach (var element in layout.Elements)
        {
            GL.VertexArrayVertexBuffer(Handle, _vertexAttribIndex, buffer.Handle, offset, layout.Stride);

            GL.EnableVertexArrayAttrib(Handle, _vertexAttribIndex);
            GL.VertexArrayAttribFormat(Handle, _vertexAttribIndex, element.ComponentCount, GetOpenGLBaseType(element.Type),
                element.Normalized, 0);


            offset += element.Size;
            _vertexAttribIndex++;
        }
    }

    public void AddIndexBuffer(IndexBuffer buffer)
    {
        GL.VertexArrayElementBuffer(Handle, buffer.Handle);
    }

    public void Bind()
    {
        GL.BindVertexArray(Handle);
    }

    public void Dispose()
    {
        GL.DeleteVertexArray(Handle);
    }

    private VertexAttribType GetOpenGLBaseType(ShaderDataType type)
    {
        return type switch
        {
            ShaderDataType.Float => VertexAttribType.Float,
            ShaderDataType.Float2 => VertexAttribType.Float,
            ShaderDataType.Float3 => VertexAttribType.Float,
            ShaderDataType.Float4 => VertexAttribType.Float,
            ShaderDataType.Int => VertexAttribType.Int,
            ShaderDataType.Int2 => VertexAttribType.Int,
            ShaderDataType.Int3 => VertexAttribType.Int,
            ShaderDataType.Int4 => VertexAttribType.Int,
            ShaderDataType.Bool => VertexAttribType.Byte,
            _ => throw new ArgumentException($"Unknown ShaderDataType {type}")
        };
    }

    private int GetSizeOfShaderDataType(ShaderDataType type)
    {
        return type switch
        {
            ShaderDataType.Float => 4,
            ShaderDataType.Float2 => 4,
            ShaderDataType.Float3 => 4,
            ShaderDataType.Float4 => 4,
            ShaderDataType.Int => 4,
            ShaderDataType.Int2 => 4,
            ShaderDataType.Int3 => 4,
            ShaderDataType.Int4 => 4,
            ShaderDataType.Bool => 1,
            _ => throw new ArgumentException($"Unknown ShaderDataType {type}")
        };
    }

}