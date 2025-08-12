using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace LearnOpenTKRender.OpenGL;

internal class Shader : IDisposable
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
    public void SetUniform(string name, Vector3 value)
    {
        unsafe
        {
            int location = GL.GetUniformLocation(Handle, name);
            if (location == -1)
                throw new Exception($"Uniform '{name}' not found in shader program.");
            GL.ProgramUniform3(Handle, location, value);
        }
    }

    public void Dispose()
    {
        GL.DeleteProgram(Handle);
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