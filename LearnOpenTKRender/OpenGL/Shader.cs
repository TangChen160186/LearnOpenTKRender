using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace LearnOpenTKRender.OpenGL;

/// <summary>
/// Uniform信息
/// </summary>
public class UniformInfo
{
    public string Name { get; set; } = string.Empty;
    public int Location { get; set; }
    public ActiveUniformType Type { get; set; }
    public int Size { get; set; }
}

internal class Shader : IDisposable
{
    internal int Handle;
    private readonly Dictionary<string, UniformInfo> _uniforms = new();

    /// <summary>
    /// 获取所有uniform信息
    /// </summary>
    public IReadOnlyDictionary<string, UniformInfo> Uniforms => _uniforms;

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

        // 获取所有uniform信息
        ReflectUniforms();
    }

    /// <summary>
    /// 反射获取所有uniform信息
    /// </summary>
    private void ReflectUniforms()
    {
        _uniforms.Clear();

        GL.GetProgram(Handle, GetProgramParameterName.ActiveUniforms, out int uniformCount);

        for (int i = 0; i < uniformCount; i++)
        {
            GL.GetActiveUniform(Handle, i, 256, out int length, out int size, out ActiveUniformType type, out string name);

            // 跳过uniform blocks和数组的元素
            if (name.Contains('[') || name.StartsWith("gl_"))
                continue;

            int location = GL.GetUniformLocation(Handle, name);
            if (location != -1)
            {
                _uniforms[name] = new UniformInfo
                {
                    Name = name,
                    Location = location,
                    Type = type,
                    Size = size
                };
            }
        }
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

    /// <summary>
    /// 检查uniform是否存在
    /// </summary>
    /// <param name="name">uniform名称</param>
    /// <returns>是否存在</returns>
    public bool HasUniform(string name)
    {
        return _uniforms.ContainsKey(name);
    }

    /// <summary>
    /// 获取uniform信息
    /// </summary>
    /// <param name="name">uniform名称</param>
    /// <returns>uniform信息，如果不存在则返回null</returns>
    public UniformInfo? GetUniformInfo(string name)
    {
        return _uniforms.TryGetValue(name, out var info) ? info : null;
    }

    /// <summary>
    /// 获取uniform的OpenGL类型
    /// </summary>
    /// <param name="name">uniform名称</param>
    /// <returns>OpenGL类型，如果不存在则返回null</returns>
    public ActiveUniformType? GetUniformType(string name)
    {
        return _uniforms.TryGetValue(name, out var info) ? info.Type : null;
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

    public void SetUniform(string name, float value)
    {
        int location = GL.GetUniformLocation(Handle, name);
        if (location == -1)
            throw new Exception($"Uniform '{name}' not found in shader program.");
        GL.ProgramUniform1(Handle, location, value);
    }

    public void SetUniform(string name, int value)
    {
        int location = GL.GetUniformLocation(Handle, name);
        if (location == -1)
            throw new Exception($"Uniform '{name}' not found in shader program.");
        GL.ProgramUniform1(Handle, location, value);
    }

    public void SetUniform(string name, bool value)
    {
        int location = GL.GetUniformLocation(Handle, name);
        if (location == -1)
            throw new Exception($"Uniform '{name}' not found in shader program.");
        GL.ProgramUniform1(Handle, location, value ? 1 : 0);
    }

    public void SetUniform(string name, Vector2 value)
    {
        int location = GL.GetUniformLocation(Handle, name);
        if (location == -1)
            throw new Exception($"Uniform '{name}' not found in shader program.");
        GL.ProgramUniform2(Handle, location, value);
    }

    /// <summary>
    /// 尝试设置uniform，如果uniform不存在则忽略错误
    /// </summary>
    public bool TrySetUniform(string name, float value)
    {
        int location = GL.GetUniformLocation(Handle, name);
        if (location == -1)
            return false;
        GL.ProgramUniform1(Handle, location, value);
        return true;
    }

    public bool TrySetUniform(string name, int value)
    {
        int location = GL.GetUniformLocation(Handle, name);
        if (location == -1)
            return false;
        GL.ProgramUniform1(Handle, location, value);
        return true;
    }

    public bool TrySetUniform(string name, bool value)
    {
        int location = GL.GetUniformLocation(Handle, name);
        if (location == -1)
            return false;
        GL.ProgramUniform1(Handle, location, value ? 1 : 0);
        return true;
    }

    public bool TrySetUniform(string name, Vector2 value)
    {
        int location = GL.GetUniformLocation(Handle, name);
        if (location == -1)
            return false;
        GL.ProgramUniform2(Handle, location, value);
        return true;
    }

    public bool TrySetUniform(string name, Vector3 value)
    {
        int location = GL.GetUniformLocation(Handle, name);
        if (location == -1)
            return false;
        GL.ProgramUniform3(Handle, location, value);
        return true;
    }

    public bool TrySetUniform(string name, Vector4 value)
    {
        int location = GL.GetUniformLocation(Handle, name);
        if (location == -1)
            return false;
        GL.ProgramUniform4(Handle, location, value);
        return true;
    }

    public bool TrySetUniform(string name, Matrix4 value)
    {
        unsafe
        {
            int location = GL.GetUniformLocation(Handle, name);
            if (location == -1)
                return false;
            GL.ProgramUniformMatrix4(Handle, location, 1, false, &value.Row0.X);
            return true;
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