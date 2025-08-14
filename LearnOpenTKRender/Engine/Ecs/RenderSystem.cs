using LearnOpenTKRender.Ecs;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace LearnOpenTKRender.Engine.Ecs;

/// <summary>
/// ��Ⱦϵͳ - ������Ⱦ���о���MeshFilter��MeshRenderer�����ʵ��
/// </summary>
internal class RenderSystem
{
    private readonly List<Entity> _entities = new();

    /// <summary>
    /// ע��ʵ�嵽��Ⱦϵͳ
    /// </summary>
    /// <param name="entity">Ҫע���ʵ��</param>
    public void RegisterEntity(Entity entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        // ���ʵ���Ƿ���б�Ҫ�����
        if (entity.HasComponent<MeshFilterComponent>() && entity.HasComponent<MeshRendererComponent>())
        {
            if (!_entities.Contains(entity))
            {
                _entities.Add(entity);
            }
        }
    }

    /// <summary>
    /// ����Ⱦϵͳ��ע��ʵ��
    /// </summary>
    /// <param name="entity">Ҫע����ʵ��</param>
    public void UnregisterEntity(Entity entity)
    {
        _entities.Remove(entity);
    }

    /// <summary>
    /// ��Ⱦ����ע���ʵ��
    /// </summary>
    /// <param name="viewMatrix">��ͼ����</param>
    /// <param name="projectionMatrix">ͶӰ����</param>
    public void Render(Matrix4 viewMatrix, Matrix4 projectionMatrix)
    {
        // ����Ⱦ���ȼ�����
        var sortedEntities = _entities
            .Where(e => e.GetComponent<MeshRendererComponent>()?.Enabled == true)
            .OrderBy(e => e.GetComponent<MeshRendererComponent>()?.RenderPriority ?? 0)
            .ToList();

        foreach (var entity in sortedEntities)
        {
            RenderEntity(entity, viewMatrix, projectionMatrix);
        }
    }

    /// <summary>
    /// ��Ⱦ����ʵ��
    /// </summary>
    /// <param name="entity">Ҫ��Ⱦ��ʵ��</param>
    /// <param name="viewMatrix">��ͼ����</param>
    /// <param name="projectionMatrix">ͶӰ����</param>
    private void RenderEntity(Entity entity, Matrix4 viewMatrix, Matrix4 projectionMatrix)
    {
        var meshFilter = entity.GetComponent<MeshFilterComponent>();
        var meshRenderer = entity.GetComponent<MeshRendererComponent>();
        var transform = entity.GetComponent<TransformComponent>();

        if (meshFilter == null || meshRenderer == null || !meshFilter.HasMesh)
            return;

        // ׼����Ⱦ״̬
        meshRenderer.PrepareRender();

        // ���ñ任����
        Matrix4 modelMatrix = Matrix4.Identity;
        if (transform != null)
        {
            // ת��System.Numerics.Matrix4x4��OpenTK.Mathematics.Matrix4
            var localToWorld = transform.LocalToWorldMatrix;
            modelMatrix = new Matrix4(
                localToWorld.M11, localToWorld.M12, localToWorld.M13, localToWorld.M14,
                localToWorld.M21, localToWorld.M22, localToWorld.M23, localToWorld.M24,
                localToWorld.M31, localToWorld.M32, localToWorld.M33, localToWorld.M34,
                localToWorld.M41, localToWorld.M42, localToWorld.M43, localToWorld.M44);
        }

        // ���ñ�׼uniform
        if (meshRenderer.Shader != null)
        {
            meshRenderer.Shader.TrySetUniform("uModel", modelMatrix);
            meshRenderer.Shader.TrySetUniform("uView", viewMatrix);
            meshRenderer.Shader.TrySetUniform("uProjection", projectionMatrix);
            meshRenderer.Shader.TrySetUniform("uMVP", modelMatrix * viewMatrix * projectionMatrix);
        }

        // Ӧ�ò�������
        meshRenderer.ApplyMaterialProperties();

        // ��������Ⱦ
        meshFilter.BindMesh();
        GL.DrawElements(PrimitiveType.Triangles, meshFilter.IndexCount, DrawElementsType.UnsignedInt, 0);
    }

    /// <summary>
    /// ��������ʵ��
    /// </summary>
    public void Clear()
    {
        _entities.Clear();
    }

    /// <summary>
    /// ��ȡ��ǰע���ʵ������
    /// </summary>
    public int EntityCount => _entities.Count;

    /// <summary>
    /// ��ȡ����ע���ʵ�壨ֻ����
    /// </summary>
    public IReadOnlyList<Entity> Entities => _entities.AsReadOnly();
}

/// <summary>
/// ��Ⱦϵͳ������ - ����ģʽ
/// </summary>
internal class RenderSystemManager
{
    private static RenderSystemManager? _instance;
    private readonly RenderSystem _renderSystem;

    private RenderSystemManager()
    {
        _renderSystem = new RenderSystem();
    }

    public static RenderSystemManager Instance
    {
        get
        {
            _instance ??= new RenderSystemManager();
            return _instance;
        }
    }

    public RenderSystem RenderSystem => _renderSystem;

    /// <summary>
    /// �Զ�ע��ʵ�壨������б�Ҫ�����
    /// </summary>
    /// <param name="entity">ʵ��</param>
    public void AutoRegisterEntity(Entity entity)
    {
        _renderSystem.RegisterEntity(entity);
    }

    /// <summary>
    /// ע��ʵ��
    /// </summary>
    /// <param name="entity">ʵ��</param>
    public void UnregisterEntity(Entity entity)
    {
        _renderSystem.UnregisterEntity(entity);
    }

    /// <summary>
    /// ��Ⱦ����ʵ��
    /// </summary>
    /// <param name="viewMatrix">��ͼ����</param>
    /// <param name="projectionMatrix">ͶӰ����</param>
    public void Render(Matrix4 viewMatrix, Matrix4 projectionMatrix)
    {
        _renderSystem.Render(viewMatrix, projectionMatrix);
    }

    /// <summary>
    /// ��������ʵ��
    /// </summary>
    public void Clear()
    {
        _renderSystem.Clear();
    }
}
