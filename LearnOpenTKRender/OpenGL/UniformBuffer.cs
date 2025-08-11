using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL4;

namespace LearnOpenTKRender.OpenGL;

class UniformBuffer : IDisposable
{
    internal int Handle;
    public UniformBuffer(int bindingPoint, int size)
    {
        GL.CreateBuffers(1, out Handle);
        GL.NamedBufferStorage(Handle, size, IntPtr.Zero, BufferStorageFlags.DynamicStorageBit);
        GL.BindBufferBase(BufferRangeTarget.UniformBuffer, bindingPoint, Handle);
    }
    public void UpdateData<T>(ref T data) where T : struct
    {
        GL.NamedBufferSubData(Handle, IntPtr.Zero, Marshal.SizeOf<T>(), ref data);
    }
    public void Dispose()
    {
        GL.DeleteBuffer(Handle);
    }
}