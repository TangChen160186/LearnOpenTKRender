using System.Drawing;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace LearnOpenTKRender.OpenGL;

internal class Framebuffer : IDisposable
{
    private readonly int _width;
    private readonly int _height;
    private readonly int _handle;
    public int Handle => _handle;

    public Framebuffer(int width, int height)
    {
        _width = width;
        _height = height;
        GL.CreateFramebuffers(1, out _handle);
    }

    public void Bind()
    {
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, _handle);
        GL.Viewport(0, 0, _width, _height);
    }


    public void Unbind()
    {
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
    }

    public void AttachColorTexture(Texture2D texture, int attachment = 0)
    {
        GL.NamedFramebufferTexture(_handle,
            (FramebufferAttachment)((uint)FramebufferAttachment.ColorAttachment0 + attachment), texture.Handle, 0);
    }

    public void AttachDepthTexture(Texture2D texture)
    {
        GL.NamedFramebufferTexture(_handle, FramebufferAttachment.DepthAttachment, texture.Handle, 0);
    }

    public void AttachDepthStencilTexture(Texture2D texture)
    {
        GL.NamedFramebufferTexture(_handle, FramebufferAttachment.DepthStencilAttachment, texture.Handle, 0);
    }

    public void Clear(Color4 color, float depth = 1.0f, int stencil = 0)
    {
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, _handle);
        GL.ClearColor(color);
        GL.ClearDepth(depth);
        GL.ClearStencil(stencil);

        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
    }

    public bool IsComplete()
    {
        return GL.CheckNamedFramebufferStatus(_handle, FramebufferTarget.Framebuffer)
               == FramebufferStatus.FramebufferComplete;
    }

    // DSA 方式设置绘制缓冲区
    public void SetDrawBuffers(params DrawBuffersEnum[] buffers)
    {
        GL.NamedFramebufferDrawBuffers(_handle, buffers.Length, buffers);
    }
    public void BliTo(Framebuffer target, int srcX0, int srcY0, int srcX1, int srcY1,
        int dstX0, int dstY0, int dstX1, int dstY1, bool color = true, bool depth = false)
    {
        ClearBufferMask mask = ClearBufferMask.None;
        if (color)
            mask |= ClearBufferMask.ColorBufferBit;
        if (depth)
            mask |= ClearBufferMask.DepthBufferBit;

        GL.BlitNamedFramebuffer(_handle, target.Handle, srcX0, srcY0, srcX1, srcY1,
            dstX0, dstY0, dstX1, dstY1, mask, BlitFramebufferFilter.Nearest);
    }

    public void Dispose()
    {
        GL.DeleteFramebuffer(_handle);
    }
}
