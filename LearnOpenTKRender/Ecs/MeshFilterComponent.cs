using LearnOpenTKRender.OpenGL;
using OpenTK.Graphics.OpenGL4;

namespace LearnOpenTKRender.Ecs;

/// <summary>
/// 网格过滤器组件 - 负责存储模型并允许选择特定的网格进行渲染
/// </summary>
internal class MeshFilterComponent : ComponentBase
{
    private Model? _model;
    private string _selectedMeshName = string.Empty;
    private StaticMesh? _currentMesh;

    /// <summary>
    /// 设置的模型
    /// </summary>
    public Model? Model
    {
        get => _model;
        set
        {
            _model = value;
            // 当设置新模型时，重置选择
            if (_model != null && _model.Meshes.Count > 0)
            {
                // 默认选择第一个mesh
                var firstMeshName = _model.GetMeshNames().FirstOrDefault();
                if (!string.IsNullOrEmpty(firstMeshName))
                {
                    SelectMesh(firstMeshName);
                }
            }
            else
            {
                _selectedMeshName = string.Empty;
                _currentMesh = null;
            }
        }
    }

    /// <summary>
    /// 当前选择的mesh名称
    /// </summary>
    public string SelectedMeshName
    {
        get => _selectedMeshName;
        set => SelectMesh(value);
    }

    /// <summary>
    /// 当前渲染的网格
    /// </summary>
    public StaticMesh? CurrentMesh => _currentMesh;

    /// <summary>
    /// 获取所有可用的mesh名称（用于UI下拉框）
    /// </summary>
    public IEnumerable<string> AvailableMeshNames
    {
        get
        {
            if (_model == null)
                return Enumerable.Empty<string>();
            return _model.GetMeshNames();
        }
    }

    /// <summary>
    /// 是否有有效的网格数据
    /// </summary>
    public bool HasMesh => _currentMesh != null;

    /// <summary>
    /// 获取当前网格的材质名称
    /// </summary>
    public string? MaterialName => _currentMesh?.MaterialName;

    /// <summary>
    /// 获取当前网格的索引数量
    /// </summary>
    public int IndexCount => _currentMesh?.IndexCount ?? 0;

    /// <summary>
    /// 模型中mesh的数量
    /// </summary>
    public int MeshCount => _model?.Meshes.Count ?? 0;

    public MeshFilterComponent()
    {
    }

    /// <summary>
    /// 选择要渲染的mesh
    /// </summary>
    /// <param name="meshName">mesh名称</param>
    /// <returns>是否选择成功</returns>
    public bool SelectMesh(string meshName)
    {
        if (_model == null || string.IsNullOrEmpty(meshName))
        {
            _selectedMeshName = string.Empty;
            _currentMesh = null;
            return false;
        }

        var mesh = _model.GetMeshByName(meshName);
        if (mesh != null)
        {
            _selectedMeshName = meshName;
            _currentMesh = mesh;
            return true;
        }

        return false;
    }

    /// <summary>
    /// 按索引选择mesh（用于向后兼容）
    /// </summary>
    /// <param name="index">mesh索引</param>
    /// <returns>是否选择成功</returns>
    public bool SelectMeshByIndex(int index)
    {
        if (_model == null || index < 0 || index >= _model.Meshes.Count)
            return false;

        var mesh = _model.GetMeshByIndex(index);
        if (mesh != null)
        {
            // 找到对应的名称
            var meshNames = _model.GetMeshNames().ToList();
            if (index < meshNames.Count)
            {
                _selectedMeshName = meshNames[index];
                _currentMesh = mesh;
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 绑定当前选择的网格用于渲染
    /// </summary>
    public void BindMesh()
    {
        _currentMesh?.Bind();
    }

    /// <summary>
    /// 清除所有数据
    /// </summary>
    public void Clear()
    {
        _model = null;
        _selectedMeshName = string.Empty;
        _currentMesh = null;
    }

    /// <summary>
    /// 获取mesh选择的调试信息
    /// </summary>
    /// <returns>调试信息字符串</returns>
    public string GetDebugInfo()
    {
        if (_model == null)
            return "No model loaded";

        var info = $"Model: {_model.Meshes.Count} meshes\n";
        info += $"Selected: {_selectedMeshName}\n";
        info += $"Available meshes: {string.Join(", ", AvailableMeshNames)}";
        return info;
    }
}

/// <summary>
/// 网格渲染器组件 - 负责渲染相关的材质和着色器信息
/// </summary>
internal class MeshRendererComponent : ComponentBase
{
    private Shader? _shader;
    private readonly Dictionary<string, Texture2D> _textures = new();
    private readonly Dictionary<string, object> _materialProperties = new();

    /// <summary>
    /// 使用的着色器
    /// </summary>
    public Shader? Shader
    {
        get => _shader;
        set => _shader = value;
    }

    /// <summary>
    /// 是否启用渲染
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// 渲染优先级（数值越小越先渲染）
    /// </summary>
    public int RenderPriority { get; set; } = 0;

    /// <summary>
    /// 是否投射阴影
    /// </summary>
    public bool CastShadows { get; set; } = true;

    /// <summary>
    /// 是否接收阴影
    /// </summary>
    public bool ReceiveShadows { get; set; } = true;

    /// <summary>
    /// 获取所有可用的uniform信息
    /// </summary>
    public IReadOnlyDictionary<string, UniformInfo>? AvailableUniforms => _shader?.Uniforms;

    public MeshRendererComponent()
    {
    }

    /// <summary>
    /// 检查uniform是否存在
    /// </summary>
    /// <param name="name">uniform名称</param>
    /// <returns>是否存在</returns>
    public bool HasUniform(string name)
    {
        return _shader?.HasUniform(name) ?? false;
    }

    /// <summary>
    /// 获取uniform信息
    /// </summary>
    /// <param name="name">uniform名称</param>
    /// <returns>uniform信息，如果不存在则返回null</returns>
    public UniformInfo? GetUniformInfo(string name)
    {
        return _shader?.GetUniformInfo(name);
    }

    /// <summary>
    /// 获取uniform的OpenGL类型
    /// </summary>
    /// <param name="name">uniform名称</param>
    /// <returns>OpenGL类型，如果不存在则返回null</returns>
    public ActiveUniformType? GetUniformType(string name)
    {
        return _shader?.GetUniformType(name);
    }

    /// <summary>
    /// 获取所有未设置的uniform列表
    /// </summary>
    /// <returns>未设置的uniform名称列表</returns>
    public List<string> GetUnsetUniforms()
    {
        if (_shader == null)
            return new List<string>();

        var unsetUniforms = new List<string>();
        foreach (var uniform in _shader.Uniforms)
        {
            if (!_materialProperties.ContainsKey(uniform.Key) && !_textures.ContainsKey(uniform.Key))
            {
                unsetUniforms.Add(uniform.Key);
            }
        }
        return unsetUniforms;
    }

    /// <summary>
    /// 设置纹理
    /// </summary>
    /// <param name="name">纹理名称/uniform名称</param>
    /// <param name="texture">纹理对象</param>
    public void SetTexture(string name, Texture2D texture)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("Texture name cannot be null or empty", nameof(name));

        if (texture == null)
            throw new ArgumentNullException(nameof(texture));

        _textures[name] = texture;
    }

    /// <summary>
    /// 从文件加载并设置纹理
    /// </summary>
    /// <param name="name">纹理名称</param>
    /// <param name="texturePath">纹理文件路径</param>
    /// <param name="format">像素格式</param>
    public void LoadTexture(string name, string texturePath, LearnOpenTKRender.OpenGL.PixelFormat format = LearnOpenTKRender.OpenGL.PixelFormat.R8G8B8A8UNorm)
    {
        var texture = TextureLoader.LoadTextureFromPath(texturePath, format);
        SetTexture(name, texture);
    }

    /// <summary>
    /// 获取纹理
    /// </summary>
    /// <param name="name">纹理名称</param>
    /// <returns>纹理对象，如果不存在则返回null</returns>
    public Texture2D? GetTexture(string name)
    {
        return _textures.TryGetValue(name, out var texture) ? texture : null;
    }

    /// <summary>
    /// 移除纹理
    /// </summary>
    /// <param name="name">纹理名称</param>
    public void RemoveTexture(string name)
    {
        _textures.Remove(name);
    }

    /// <summary>
    /// 设置材质属性
    /// </summary>
    /// <param name="name">属性名称</param>
    /// <param name="value">属性值</param>
    public void SetMaterialProperty(string name, object value)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("Property name cannot be null or empty", nameof(name));

        _materialProperties[name] = value;
    }

    /// <summary>
    /// 获取材质属性
    /// </summary>
    /// <param name="name">属性名称</param>
    /// <returns>属性值，如果不存在则返回null</returns>
    public T? GetMaterialProperty<T>(string name)
    {
        if (_materialProperties.TryGetValue(name, out var value) && value is T typedValue)
        {
            return typedValue;
        }
        return default(T);
    }

    /// <summary>
    /// 准备渲染（绑定着色器和纹理）
    /// </summary>
    public void PrepareRender()
    {
        if (!Enabled || _shader == null)
            return;

        _shader.Use();

        // 绑定纹理到对应的纹理单元
        int textureUnit = 0;
        foreach (var kvp in _textures)
        {
            kvp.Value.BindToUnit(textureUnit);
            textureUnit++;
        }
    }

    /// <summary>
    /// 应用材质属性到着色器
    /// </summary>
    public void ApplyMaterialProperties()
    {
        if (_shader == null)
            return;

        foreach (var kvp in _materialProperties)
        {
            var name = kvp.Key;
            var value = kvp.Value;

            switch (value)
            {
                case float floatValue:
                    _shader.TrySetUniform(name, floatValue);
                    break;
                case int intValue:
                    _shader.TrySetUniform(name, intValue);
                    break;
                case bool boolValue:
                    _shader.TrySetUniform(name, boolValue);
                    break;
                case System.Numerics.Vector2 vec2Value:
                    _shader.TrySetUniform(name, new OpenTK.Mathematics.Vector2(vec2Value.X, vec2Value.Y));
                    break;
                case System.Numerics.Vector3 vec3Value:
                    _shader.TrySetUniform(name, new OpenTK.Mathematics.Vector3(vec3Value.X, vec3Value.Y, vec3Value.Z));
                    break;
                case System.Numerics.Vector4 vec4Value:
                    _shader.TrySetUniform(name, new OpenTK.Mathematics.Vector4(vec4Value.X, vec4Value.Y, vec4Value.Z, vec4Value.W));
                    break;
                case System.Numerics.Matrix4x4 mat4Value:
                    // 转换为OpenTK.Mathematics.Matrix4
                    var openTKMatrix = new OpenTK.Mathematics.Matrix4(
                        mat4Value.M11, mat4Value.M12, mat4Value.M13, mat4Value.M14,
                        mat4Value.M21, mat4Value.M22, mat4Value.M23, mat4Value.M24,
                        mat4Value.M31, mat4Value.M32, mat4Value.M33, mat4Value.M34,
                        mat4Value.M41, mat4Value.M42, mat4Value.M43, mat4Value.M44);
                    _shader.TrySetUniform(name, openTKMatrix);
                    break;
                case OpenTK.Mathematics.Vector2 openTKVec2:
                    _shader.TrySetUniform(name, openTKVec2);
                    break;
                case OpenTK.Mathematics.Vector3 openTKVec3:
                    _shader.TrySetUniform(name, openTKVec3);
                    break;
                case OpenTK.Mathematics.Vector4 openTKVec4:
                    _shader.TrySetUniform(name, openTKVec4);
                    break;
                case OpenTK.Mathematics.Matrix4 openTKMat4:
                    _shader.TrySetUniform(name, openTKMat4);
                    break;
                default:
                    Console.WriteLine($"Unsupported material property type for '{name}': {value.GetType()}");
                    break;
            }
        }
    }

    /// <summary>
    /// 清理资源
    /// </summary>
    public override void OnRemovedFromEntity()
    {
        base.OnRemovedFromEntity();

        // 清理纹理资源
        foreach (var texture in _textures.Values)
        {
            texture?.Dispose();
        }
        _textures.Clear();

        // 清理着色器资源
        _shader?.Dispose();
        _shader = null;

        _materialProperties.Clear();
    }
}