using OpenTK.Graphics.OpenGL4;

namespace LearnOpenTKRender.OpenGL;

internal class VertexBuffer : IDisposable
{
    internal int Handle;

    public VertexBuffer(float[] data)
    {
        GL.CreateBuffers(1, out Handle);
        GL.NamedBufferData(Handle, data.Length * sizeof(float), data, BufferUsageHint.StaticDraw);
    }

    public void Dispose()
    {
        GL.DeleteBuffer(Handle);
    }
}