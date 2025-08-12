using Assimp;
using System.Numerics;

namespace LearnOpenTKRender.OpenGL;

internal class Model
{
    public List<StaticMesh> Meshes { get; private set; }


    public Model(List<StaticMesh> meshes)
    {
        Meshes = meshes;
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
class Material
{
    
}
    




struct Vertex(Vector3 position, Vector3 normal, Vector2 texCoord, Vector3 tangent, Vector3 bitangent)
{
    public Vector3 Position = position;
    public Vector3 Normal = normal;
    public Vector2 TexCoord = texCoord;
    public Vector3 Tangent = tangent;
    public Vector3 Bitangent = bitangent;
}
