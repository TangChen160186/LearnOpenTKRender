using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

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

        public IndexBuffer(uint[] data)
        {
            GL.CreateBuffers(1, out Handle);
            GL.NamedBufferData(Handle, data.Length * sizeof(uint), data, BufferUsageHint.StaticDraw);
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
            GL.DeleteShader(Handle);
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
}
