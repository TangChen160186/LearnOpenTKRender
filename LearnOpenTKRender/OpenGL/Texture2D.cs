

using OpenTK.Graphics.OpenGL4;
using StbImageSharp;
using System.Drawing;
namespace LearnOpenTKRender.OpenGL;

public static class PixelFormatExtensions
{
    public static SizedInternalFormat ToOpenGL(this PixelFormat format)
    {
        switch (format)
        {
            case PixelFormat.R8UNorm:
                return SizedInternalFormat.R8;
                break;
            case PixelFormat.R8G8UNorm:
                return SizedInternalFormat.Rg8;
                break;
            case PixelFormat.R8G8B8A8UNorm:
                return SizedInternalFormat.Rgba8;
                break;
            case PixelFormat.R8G8B8A8UNormSRgb:
                return SizedInternalFormat.Srgb8Alpha8;
                break;
            case PixelFormat.R16Float:
                return SizedInternalFormat.R16f;
                break;
            case PixelFormat.R16G16Float:
                return SizedInternalFormat.Rg16f;
                break;

            case PixelFormat.R16G16B16A16Float:
                return SizedInternalFormat.Rgba16f;
                break;
            case PixelFormat.R32Float:
                return SizedInternalFormat.R32f;
                break;
            case PixelFormat.R32G32B32A32Float:
                return SizedInternalFormat.Rgba32f;
                break;
            case PixelFormat.D16UNorm:
                return SizedInternalFormat.DepthComponent16;
                break;
            case PixelFormat.D24UNormS8UInt:
                return SizedInternalFormat.Depth24Stencil8;
                break;
            case PixelFormat.D32Float:
                return SizedInternalFormat.DepthComponent32f;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(format), format, null);
        }
    }

    public static PixelType GetPixelType(this PixelFormat format)
    {
        return format switch
        {
            PixelFormat.R8UNorm or PixelFormat.R8G8UNorm or PixelFormat.R8G8B8A8UNorm or PixelFormat.R8G8B8A8UNormSRgb => PixelType.UnsignedByte,
            PixelFormat.R16Float or PixelFormat.R16G16Float or PixelFormat.R16G16B16A16Float => PixelType.HalfFloat,
            PixelFormat.R32Float or PixelFormat.R32G32B32A32Float => PixelType.Float,
            PixelFormat.D16UNorm => PixelType.UnsignedShort,
            PixelFormat.D24UNormS8UInt => PixelType.UnsignedInt248,
            PixelFormat.D32Float => PixelType.Float,
            _ => PixelType.UnsignedByte
        };
    }

    public static bool IsDepthFormat(this PixelFormat format)
    {
        return format is PixelFormat.D16UNorm or PixelFormat.D24UNormS8UInt or PixelFormat.D32Float;
    }

    public static OpenTK.Graphics.OpenGL4.PixelFormat GetGLPixelFormat(this PixelFormat format)
    {
        return format switch
        {
            PixelFormat.R8UNorm => OpenTK.Graphics.OpenGL4.PixelFormat.Red,
            PixelFormat.R8G8UNorm => OpenTK.Graphics.OpenGL4.PixelFormat.Rg,
            PixelFormat.R8G8B8A8UNorm or PixelFormat.R8G8B8A8UNormSRgb => OpenTK.Graphics.OpenGL4.PixelFormat.Rgba,
            PixelFormat.D16UNorm or PixelFormat.D32Float => OpenTK.Graphics.OpenGL4.PixelFormat.DepthComponent,
            PixelFormat.D24UNormS8UInt => OpenTK.Graphics.OpenGL4.PixelFormat.DepthStencil,
            _ => OpenTK.Graphics.OpenGL4.PixelFormat.Rgba
        };
    }
}

public class Texture2D : IDisposable
{
    private int _handle;
    public int Handle => _handle;
    public int Width { get; private set; }
    public int Height { get; private set; }
    public int MipLevels { get; private set; }
    public PixelFormat Format { get; private set; }
    public bool IsValid => Handle != 0 && !_disposed;

    private bool _disposed = false;

    // 1. 从描述符创建（最灵活）
    public Texture2D(int width, int height, PixelFormat format, bool generateMips = true)
    {
        Width = width;
        Height = height;
        Format = format;
        MipLevels = generateMips ? CalculateMipLevels(width, height) : 1;
        GL.CreateTextures(TextureTarget.Texture2D,1,out _handle);
        GL.TextureStorage2D(Handle, MipLevels, format.ToOpenGL(), Width, Height);
            
        SetDefaultParameters();
    }

    public void SetData(byte[] data, int mipLevel = 0)
    {
        if (data == null) throw new ArgumentNullException(nameof(data));

        int width = Math.Max(1, Width >> mipLevel);
        int height = Math.Max(1, Height >> mipLevel);

        GL.TextureSubImage2D(Handle, mipLevel, 0, 0, width, height,
            Format.GetGLPixelFormat(), Format.GetPixelType(), data);
    }


    public void BindToUnit(int unit)
    {
        GL.BindTextureUnit(unit, Handle);
    }

    // 参数设置
    public void SetFilterMode(TextureMinFilter minFilter, TextureMagFilter magFilter)
    {
        GL.TextureParameter(Handle, TextureParameterName.TextureMinFilter, (int)minFilter);
        GL.TextureParameter(Handle, TextureParameterName.TextureMagFilter, (int)magFilter);
    }

    public void SetWrapMode(TextureWrapMode wrapS, TextureWrapMode wrapT)
    {
        GL.TextureParameter(Handle, TextureParameterName.TextureWrapS, (int)wrapS);
        GL.TextureParameter(Handle, TextureParameterName.TextureWrapT, (int)wrapT);
    }

    public void SetAnisotropy(float level)
    {
        GL.TextureParameter(Handle, TextureParameterName.TextureMaxAnisotropy, level);
    }


    public static Texture2D CreateRenderTarget(int width, int height, PixelFormat format = PixelFormat.R8G8B8A8UNorm)
    {
        return new Texture2D(width, height, format, false);
    }

    public static Texture2D CreateDepthBuffer(int width, int height)
    {
        return new Texture2D(width, height, PixelFormat.D24UNormS8UInt, false);
    }

    // 私有辅助方法
    private void SetDefaultParameters()
    {
        if (Format.IsDepthFormat())
        {
            SetFilterMode(TextureMinFilter.Nearest, TextureMagFilter.Nearest);
            SetWrapMode(TextureWrapMode.ClampToEdge, TextureWrapMode.ClampToEdge);
        }
        else
        {
            var minFilter = MipLevels > 1 ? TextureMinFilter.LinearMipmapLinear : TextureMinFilter.Linear;
            SetFilterMode(minFilter, TextureMagFilter.Linear);
            SetWrapMode(TextureWrapMode.Repeat, TextureWrapMode.Repeat);
            //SetAnisotropy(16.0f);
        }
    }
    public void GenerateMipmaps()
    {
        if (MipLevels > 1)
            GL.GenerateTextureMipmap(Handle);
    }

    private static int CalculateMipLevels(int width, int height)
    {
        return (int)Math.Floor(Math.Log2(Math.Max(width, height))) + 1;
    }

    public void Dispose()
    {
        if (!_disposed && Handle != 0)
        {
            GL.DeleteTexture(Handle);
            _handle = 0;
            _disposed = true;
        }
    }
}
// 简化的格式枚举
public enum PixelFormat
{
    R8UNorm,
    R8G8UNorm,
    R8G8B8A8UNorm,
    R8G8B8A8UNormSRgb,
    R16Float,
    R16G16Float,
    R16G16B16A16Float,
    R32Float,
    R32G32B32A32Float,
    D16UNorm,
    D24UNormS8UInt,
    D32Float
}

public static class TextureLoader
{
    public static Texture2D LoadTextureFromPath(string path, PixelFormat format, bool generateMips = true)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException($"Texture file not found: {path}");

        // 根据格式决定加载通道数
        var colorComponents = GetColorComponents(format);

        // 使用 StbImageSharp 加载图像
        ImageResult result = ImageResult.FromStream(
            File.OpenRead(path),
            colorComponents);

        // 创建纹理并上传数据
        var texture = new Texture2D(result.Width, result.Height, format, generateMips);
        texture.SetData(result.Data);
        texture.GenerateMipmaps();

        return texture;
    }
    private static ColorComponents GetColorComponents(PixelFormat format)
    {
        return format switch
        {
            PixelFormat.R8UNorm => ColorComponents.Grey,
            PixelFormat.R8G8UNorm => ColorComponents.GreyAlpha,
            PixelFormat.R8G8B8A8UNorm or PixelFormat.R8G8B8A8UNormSRgb => ColorComponents.RedGreenBlueAlpha,
            _ => ColorComponents.RedGreenBlueAlpha // 默认 RGBA
        };
    }
    public static Texture2D LoadTextureWithSolidColor(int width, int height, Color color)
    {
        byte[] data = new byte[width * height * 4];
        for (int i = 0; i < data.Length; i += 4)
        {
            data[i] = color.R;
            data[i + 1] = color.G;
            data[i + 2] = color.B;
            data[i + 3] = color.A;
        }

        var texture = new Texture2D(width, height, PixelFormat.R8G8B8A8UNorm, false);
        texture.SetData(data);
        return texture;
    }
}