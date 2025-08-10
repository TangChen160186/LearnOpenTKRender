using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Drawing;
using System.Runtime.InteropServices;

namespace LearnOpenTKRender.OpenTK
{
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

    class Shader : IDisposable
    {
        internal int Handle;

        public Shader(string vertexShaderSource, string fragmentShaderSource)
        {
            int vertexShader = CompileShader(ShaderType.VertexShader, vertexShaderSource);
            int fragmentShader = CompileShader(ShaderType.FragmentShader, fragmentShaderSource);

            Handle = GL.CreateProgram();
            GL.AttachShader(Handle, vertexShader);
            GL.AttachShader(Handle, fragmentShader);
            GL.LinkProgram(Handle);
            GL.GetProgram(Handle, GetProgramParameterName.LinkStatus, out int success);
            if (success == 0)
            {
                GL.GetProgramInfoLog(Handle, out var infoLog);
                throw new Exception($"Program linking failed: {infoLog}");
            }

            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);
        }

        private int CompileShader(ShaderType type, string source)
        {
            var shader = GL.CreateShader(type);
            GL.ShaderSource(shader, source);
            GL.CompileShader(shader);
            GL.GetShader(shader, ShaderParameter.CompileStatus, out int success);

            if (success == 0)
            {
                GL.GetShaderInfoLog(shader, out var infoLog);
                throw new Exception($"Error compiling shader of type {type}: {infoLog}");
            }

            return shader;
        }

        public void Use()
        {
            GL.UseProgram(Handle);
        }


        public void SetUniform(string name, Vector4 value)
        {
            int location = GL.GetUniformLocation(Handle, name);
            if (location == -1)
                throw new Exception($"Uniform '{name}' not found in shader program.");
            GL.ProgramUniform4(Handle, location, value);
        }

        public void SetUniform(string name, Matrix4 value)
        {
            unsafe
            {
                int location = GL.GetUniformLocation(Handle, name);
                if (location == -1)
                    throw new Exception($"Uniform '{name}' not found in shader program.");
                GL.ProgramUniformMatrix4(Handle, location,1,false, &value.Row0.X);
            }
        }

        
        public void Dispose()
        {
            GL.DeleteProgram(Handle);
        }
    }

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
    class ShaderLoader
    {
        public static Shader Load(string vertexShaderPath, string fragmentShaderPath)
        {
            if (!File.Exists(vertexShaderPath))
                throw new FileNotFoundException($"Shader file not found: {vertexShaderPath}");
            if (!File.Exists(fragmentShaderPath))
                throw new FileNotFoundException($"Shader file not found: {fragmentShaderPath}");

            var vertexSource = File.ReadAllText(vertexShaderPath);
            var fragmentSource = File.ReadAllText(fragmentShaderPath);
            return new Shader(vertexSource, fragmentSource);
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

        // 2. 从文件加载（最常用）
        public Texture2D(string filePath, bool sRGB = true)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Texture file not found: {filePath}");

            using var bitmap = new Bitmap(filePath);
            Width = bitmap.Width;
            Height = bitmap.Height;
            Format = sRGB ? PixelFormat.R8G8B8A8_UNorm_sRGB : PixelFormat.R8G8B8A8_UNorm;
            MipLevels = CalculateMipLevels(Width, Height);

            Handle = GL.CreateTexture(TextureTarget.Texture2D);
            GL.TextureStorage2D(Handle, MipLevels, Format.ToOpenGL(), Width, Height);

            UploadBitmapData(bitmap);
            SetDefaultParameters();
            GL.GenerateTextureMipmap(Handle);
        }

        // 3. 从数据创建（程序生成）
        public Texture2D(int width, int height, byte[] data, PixelFormat format = PixelFormat.R8G8B8A8_UNorm)
        {
            Width = width;
            Height = height;
            Format = format;
            MipLevels = 1; // 数据纹理通常不需要mip

            Handle = GL.CreateTexture(TextureTarget.Texture2D);
            GL.TextureStorage2D(Handle, MipLevels, format.ToOpenGL(), Width, Height);

            SetData(data);
            SetDefaultParameters();
        }

        // 核心方法
        public void SetData(byte[] data, int mipLevel = 0)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            int width = Math.Max(1, Width >> mipLevel);
            int height = Math.Max(1, Height >> mipLevel);

            GL.TextureSubImage2D(Handle, mipLevel, 0, 0, width, height,
                Format.GetPixelFormat(), Format.GetPixelType(), data);
        }

        public void Bind(int textureUnit = 0)
        {
            GL.ActiveTexture(TextureUnit.Texture0 + textureUnit);
            GL.BindTexture(TextureTarget.Texture2D, Handle);
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
            GL.TextureParameter(Handle, (TextureParameterName)ExtTextureFilterAnisotropic.TextureMaxAnisotropyExt, level);
        }

        // 便捷静态方法
        public static Texture2D CreateSolid(int width, int height, Color color)
        {
            byte[] data = new byte[width * height * 4];
            for (int i = 0; i < data.Length; i += 4)
            {
                data[i] = color.R;
                data[i + 1] = color.G;
                data[i + 2] = color.B;
                data[i + 3] = color.A;
            }
            return new Texture2D(width, height, data);
        }

        public static Texture2D CreateRenderTarget(int width, int height, PixelFormat format = PixelFormat.R8G8B8A8_UNorm)
        {
            return new Texture2D(width, height, format, false);
        }

        public static Texture2D CreateDepthBuffer(int width, int height)
        {
            return new Texture2D(width, height, PixelFormat.D24_UNorm_S8_UInt, false);
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
                SetAnisotropy(16.0f);
            }
        }

        private void UploadBitmapData(Bitmap bitmap)
        {
            var bitmapData = bitmap.LockBits(new Rectangle(0, 0, Width, Height),
                ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            try
            {
                GL.TextureSubImage2D(Handle, 0, 0, 0, Width, Height,
                    OpenTK.Graphics.OpenGL4.PixelFormat.Bgra, PixelType.UnsignedByte, bitmapData.Scan0);
            }
            finally
            {
                bitmap.UnlockBits(bitmapData);
            }
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
                Handle = 0;
                _disposed = true;
            }
        }
    }

    // 简化的格式枚举
    public enum PixelFormat
    {
        R8_UNorm,
        R8G8_UNorm,
        R8G8B8A8_UNorm,
        R8G8B8A8_UNorm_sRGB,
        R16_Float,
        R16G16_Float,
        R16G16B16A16_Float,
        R32_Float,
        R32G32B32A32_Float,
        D16_UNorm,
        D24_UNorm_S8_UInt,
        D32_Float
    }
}
