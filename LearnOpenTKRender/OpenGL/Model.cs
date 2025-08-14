using Assimp;
using System.Numerics;

namespace LearnOpenTKRender.OpenGL;

internal class Model
{
    public List<StaticMesh> Meshes { get; private set; }
    private readonly Dictionary<string, StaticMesh> _meshByName = new();

    public Model(List<StaticMesh> meshes)
    {
        Meshes = meshes;

        // 建立名称到网格的映射
        for (int i = 0; i < meshes.Count; i++)
        {
            var mesh = meshes[i];
            var meshName = !string.IsNullOrEmpty(mesh.MaterialName) ? mesh.MaterialName : $"Mesh_{i}";
            _meshByName[meshName] = mesh;
        }
    }

    /// <summary>
    /// 根据名称获取网格
    /// </summary>
    /// <param name="name">网格名称或材质名称</param>
    /// <returns>网格对象，如果不存在则返回null</returns>
    public StaticMesh? GetMeshByName(string name)
    {
        return _meshByName.TryGetValue(name, out var mesh) ? mesh : null;
    }

    /// <summary>
    /// 获取所有网格名称
    /// </summary>
    /// <returns>网格名称列表</returns>
    public IEnumerable<string> GetMeshNames()
    {
        return _meshByName.Keys;
    }

    /// <summary>
    /// 根据索引获取网格
    /// </summary>
    /// <param name="index">网格索引</param>
    /// <returns>网格对象，如果索引无效则返回null</returns>
    public StaticMesh? GetMeshByIndex(int index)
    {
        if (index < 0 || index >= Meshes.Count)
            return null;
        return Meshes[index];
    }
}

class StaticMesh
{
    public VertexArray VAO { get; private set; }
    public VertexBuffer VBO { get; private set; }
    public IndexBuffer IBO { get; private set; }
    public string MaterialName { get; private set; }
    public int IndexCount { get; private set; }
    public StaticMesh(Vertex[] vertices, uint[] indices,string materialName)
    {
        MaterialName = materialName;
        IndexCount = indices.Length;

        // 转换Vertex数组为float数组
        var vertexData = new float[vertices.Length * 14]; // 3+3+2+3+3 = 14 floats per vertex
        for (int i = 0; i < vertices.Length; i++)
        {
            int offset = i * 14;
            var vertex = vertices[i];

            // Position
            vertexData[offset + 0] = vertex.Position.X;
            vertexData[offset + 1] = vertex.Position.Y;
            vertexData[offset + 2] = vertex.Position.Z;

            // Normal
            vertexData[offset + 3] = vertex.Normal.X;
            vertexData[offset + 4] = vertex.Normal.Y;
            vertexData[offset + 5] = vertex.Normal.Z;

            // TexCoord
            vertexData[offset + 6] = vertex.TexCoord.X;
            vertexData[offset + 7] = vertex.TexCoord.Y;

            // Tangent
            vertexData[offset + 8] = vertex.Tangent.X;
            vertexData[offset + 9] = vertex.Tangent.Y;
            vertexData[offset + 10] = vertex.Tangent.Z;

            // Bitangent
            vertexData[offset + 11] = vertex.Bitangent.X;
            vertexData[offset + 12] = vertex.Bitangent.Y;
            vertexData[offset + 13] = vertex.Bitangent.Z;
        }

        // 创建OpenGL对象
        VAO = new VertexArray();
        VBO = new VertexBuffer(vertexData);
        IBO = new IndexBuffer(indices);

        VAO.AddVertexBuffer(VBO, new BufferLayout([
            new BufferElement(ShaderDataType.Float3), // Position
            new BufferElement(ShaderDataType.Float3), // Normal
            new BufferElement(ShaderDataType.Float2), // TexCoord
            new BufferElement(ShaderDataType.Float3), // Tangent
            new BufferElement(ShaderDataType.Float3), // Bitangent
        ]));
        VAO.AddIndexBuffer(IBO);

    }

    public void Bind()
    {
        VAO.Bind();
    }
}

 class ModelLoader
{
    private static readonly AssimpContext _context = new AssimpContext();

    public static Model LoadModel(string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException($"Model file not found: {path}");

        var scene = _context.ImportFile(path,
            PostProcessSteps.Triangulate |
            PostProcessSteps.FlipUVs |
            PostProcessSteps.CalculateTangentSpace |
            PostProcessSteps.GenerateNormals);

        if (scene == null || scene.SceneFlags.HasFlag(SceneFlags.Incomplete) || scene.RootNode == null)
            throw new Exception($"Failed to load model: {path}");

        var meshes = new List<StaticMesh>();
        var directory = Path.GetDirectoryName(path);

        ProcessNode(scene.RootNode, scene, meshes, directory);

        return new Model(meshes);
    }


    private static void ProcessNode(Node node, Scene scene, List<StaticMesh> meshes, string directory)
    {
        // 处理当前节点的所有网格
        for (int i = 0; i < node.MeshCount; i++)
        {
            var mesh = scene.Meshes[node.MeshIndices[i]];
            meshes.Add(ProcessMesh(mesh, scene, directory));
        }

        // 递归处理子节点
        for (int i = 0; i < node.ChildCount; i++)
        {
            ProcessNode(node.Children[i], scene, meshes, directory);
        }
    }
    private static StaticMesh ProcessMesh(Mesh mesh, Scene scene, string directory)
    {
        var vertices = new List<Vertex>();
        var indices = new List<uint>();

        // 处理顶点
        for (int i = 0; i < mesh.VertexCount; i++)
        {
            var vertex = new Vertex(
                position: new Vector3(mesh.Vertices[i].X, mesh.Vertices[i].Y, mesh.Vertices[i].Z),
                normal: mesh.HasNormals ? new Vector3(mesh.Normals[i].X, mesh.Normals[i].Y, mesh.Normals[i].Z) : Vector3.Zero,
                texCoord: mesh.HasTextureCoords(0) ? new Vector2(mesh.TextureCoordinateChannels[0][i].X, mesh.TextureCoordinateChannels[0][i].Y) : Vector2.Zero,
                tangent: mesh.HasTangentBasis ? new Vector3(mesh.Tangents[i].X, mesh.Tangents[i].Y, mesh.Tangents[i].Z) : Vector3.Zero,
                bitangent: mesh.HasTangentBasis ? new Vector3(mesh.BiTangents[i].X, mesh.BiTangents[i].Y, mesh.BiTangents[i].Z) : Vector3.Zero
            );
            vertices.Add(vertex);
        }

        // 处理索引
        for (int i = 0; i < mesh.FaceCount; i++)
        {
            var face = mesh.Faces[i];
            for (int j = 0; j < face.IndexCount; j++)
            {
                indices.Add((uint)face.Indices[j]);
            }
        }

        // 获取材质名称
        string materialName = "";
        if (mesh.MaterialIndex >= 0 && mesh.MaterialIndex < scene.MaterialCount)
        {
            var material = scene.Materials[mesh.MaterialIndex];
            materialName = material.Name ?? $"Material_{mesh.MaterialIndex}";
        }

        return new StaticMesh(vertices.ToArray(), indices.ToArray(), materialName);
    }

}
/// <summary>
/// 材质类 - 封装着色器和纹理的组合
/// </summary>
class Material : IDisposable
{
    private readonly Dictionary<string, Texture2D> _textures = new();
    private readonly Dictionary<string, object> _properties = new();
    private bool _disposed = false;

    /// <summary>
    /// 材质名称
    /// </summary>
    public string Name { get; set; } = "DefaultMaterial";

    /// <summary>
    /// 关联的着色器
    /// </summary>
    public Shader? Shader { get; set; }

    /// <summary>
    /// 设置纹理
    /// </summary>
    /// <param name="name">纹理名称</param>
    /// <param name="texture">纹理对象</param>
    public void SetTexture(string name, Texture2D texture)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("Texture name cannot be null or empty", nameof(name));

        ArgumentNullException.ThrowIfNull(texture);
        _textures[name] = texture;
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
    /// 设置材质属性
    /// </summary>
    /// <param name="name">属性名称</param>
    /// <param name="value">属性值</param>
    public void SetProperty(string name, object value)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("Property name cannot be null or empty", nameof(name));

        _properties[name] = value;
    }

    /// <summary>
    /// 获取材质属性
    /// </summary>
    /// <param name="name">属性名称</param>
    /// <returns>属性值，如果不存在则返回默认值</returns>
    public T? GetProperty<T>(string name)
    {
        if (_properties.TryGetValue(name, out var value) && value is T typedValue)
        {
            return typedValue;
        }
        return default;
    }

    /// <summary>
    /// 应用材质到渲染管线
    /// </summary>
    public void Apply()
    {
        if (Shader == null)
            return;

        Shader.Use();

        // 绑定纹理
        int textureUnit = 0;
        foreach (var kvp in _textures)
        {
            kvp.Value.BindToUnit(textureUnit);
            textureUnit++;
        }

        // 应用属性
        foreach (var kvp in _properties)
        {
            ApplyProperty(kvp.Key, kvp.Value);
        }
    }

    private void ApplyProperty(string name, object value)
    {
        if (Shader == null)
            return;

        switch (value)
        {
            case float floatValue:
                Shader.TrySetUniform(name, floatValue);
                break;
            case int intValue:
                Shader.TrySetUniform(name, intValue);
                break;
            case bool boolValue:
                Shader.TrySetUniform(name, boolValue);
                break;
            case System.Numerics.Vector2 vec2Value:
                Shader.TrySetUniform(name, new OpenTK.Mathematics.Vector2(vec2Value.X, vec2Value.Y));
                break;
            case System.Numerics.Vector3 vec3Value:
                Shader.TrySetUniform(name, new OpenTK.Mathematics.Vector3(vec3Value.X, vec3Value.Y, vec3Value.Z));
                break;
            case System.Numerics.Vector4 vec4Value:
                Shader.TrySetUniform(name, new OpenTK.Mathematics.Vector4(vec4Value.X, vec4Value.Y, vec4Value.Z, vec4Value.W));
                break;
            case OpenTK.Mathematics.Vector2 openTKVec2:
                Shader.TrySetUniform(name, openTKVec2);
                break;
            case OpenTK.Mathematics.Vector3 openTKVec3:
                Shader.TrySetUniform(name, openTKVec3);
                break;
            case OpenTK.Mathematics.Vector4 openTKVec4:
                Shader.TrySetUniform(name, openTKVec4);
                break;
            case OpenTK.Mathematics.Matrix4 openTKMat4:
                Shader.TrySetUniform(name, openTKMat4);
                break;
        }
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        foreach (var texture in _textures.Values)
        {
            texture?.Dispose();
        }
        _textures.Clear();

        Shader?.Dispose();
        Shader = null;

        _properties.Clear();
        _disposed = true;
    }
}
    




struct Vertex(Vector3 position, Vector3 normal, Vector2 texCoord, Vector3 tangent, Vector3 bitangent)
{
    public Vector3 Position = position;
    public Vector3 Normal = normal;
    public Vector2 TexCoord = texCoord;
    public Vector3 Tangent = tangent;
    public Vector3 Bitangent = bitangent;
}
