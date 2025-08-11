using OpenTK.Graphics.OpenGL4;

namespace LearnOpenTKRender.OpenGL;

internal class IndexBuffer : IDisposable
{
    internal int Handle;
    public int Count { get; private set; }
    public IndexBuffer(uint[] data)
    {
        GL.CreateBuffers(1, out Handle);
        GL.NamedBufferData(Handle, data.Length * sizeof(uint), data, BufferUsageHint.StaticDraw);
        Count = data.Length;
    }

    public void Dispose()
    {
        GL.DeleteBuffer(Handle);
    }
}