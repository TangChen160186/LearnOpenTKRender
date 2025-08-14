using LearnOpenTKRender.Ecs;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace LearnOpenTKRender.Engine.Ecs;

/// <summary>
/// 渲染系统 - 负责渲染所有具有MeshFilter和MeshRenderer组件的实体
/// </summary>
internal class RenderSystem
{
    private readonly List<Entity> _entities = new();

    /// <summary>
    /// 注册实体到渲染系统
    /// </summary>
    /// <param name="entity">要注册的实体</param>
    public void RegisterEntity(Entity entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        // 检查实体是否具有必要的组件
        if (entity.HasComponent<MeshFilterComponent>() && entity.HasComponent<MeshRendererComponent>())
        {
            if (!_entities.Contains(entity))
            {
                _entities.Add(entity);
            }
        }
    }

    /// <summary>
    /// 从渲染系统中注销实体
    /// </summary>
    /// <param name="entity">要注销的实体</param>
    public void UnregisterEntity(Entity entity)
    {
        _entities.Remove(entity);
    }

    /// <summary>
    /// 渲染所有注册的实体
    /// </summary>
    /// <param name="viewMatrix">视图矩阵</param>
    /// <param name="projectionMatrix">投影矩阵</param>
    public void Render(Matrix4 viewMatrix, Matrix4 projectionMatrix)
    {
        // 按渲染优先级排序
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
    /// 渲染单个实体
    /// </summary>
    /// <param name="entity">要渲染的实体</param>
    /// <param name="viewMatrix">视图矩阵</param>
    /// <param name="projectionMatrix">投影矩阵</param>
    private void RenderEntity(Entity entity, Matrix4 viewMatrix, Matrix4 projectionMatrix)
    {
        var meshFilter = entity.GetComponent<MeshFilterComponent>();
        var meshRenderer = entity.GetComponent<MeshRendererComponent>();
        var transform = entity.GetComponent<TransformComponent>();

        if (meshFilter == null || meshRenderer == null || !meshFilter.HasMesh)
            return;

        // 准备渲染状态
        meshRenderer.PrepareRender();

        // 设置变换矩阵
        Matrix4 modelMatrix = Matrix4.Identity;
        if (transform != null)
        {
            // 转换System.Numerics.Matrix4x4到OpenTK.Mathematics.Matrix4
            var localToWorld = transform.LocalToWorldMatrix;
            modelMatrix = new Matrix4(
                localToWorld.M11, localToWorld.M12, localToWorld.M13, localToWorld.M14,
                localToWorld.M21, localToWorld.M22, localToWorld.M23, localToWorld.M24,
                localToWorld.M31, localToWorld.M32, localToWorld.M33, localToWorld.M34,
                localToWorld.M41, localToWorld.M42, localToWorld.M43, localToWorld.M44);
        }

        // 设置标准uniform
        if (meshRenderer.Shader != null)
        {
            meshRenderer.Shader.TrySetUniform("uModel", modelMatrix);
            meshRenderer.Shader.TrySetUniform("uView", viewMatrix);
            meshRenderer.Shader.TrySetUniform("uProjection", projectionMatrix);
            meshRenderer.Shader.TrySetUniform("uMVP", modelMatrix * viewMatrix * projectionMatrix);
        }

        // 应用材质属性
        meshRenderer.ApplyMaterialProperties();

        // 绑定网格并渲染
        meshFilter.BindMesh();
        GL.DrawElements(PrimitiveType.Triangles, meshFilter.IndexCount, DrawElementsType.UnsignedInt, 0);
    }

    /// <summary>
    /// 清理所有实体
    /// </summary>
    public void Clear()
    {
        _entities.Clear();
    }

    /// <summary>
    /// 获取当前注册的实体数量
    /// </summary>
    public int EntityCount => _entities.Count;

    /// <summary>
    /// 获取所有注册的实体（只读）
    /// </summary>
    public IReadOnlyList<Entity> Entities => _entities.AsReadOnly();
}

/// <summary>
/// 渲染系统管理器 - 单例模式
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
    /// 自动注册实体（如果具有必要组件）
    /// </summary>
    /// <param name="entity">实体</param>
    public void AutoRegisterEntity(Entity entity)
    {
        _renderSystem.RegisterEntity(entity);
    }

    /// <summary>
    /// 注销实体
    /// </summary>
    /// <param name="entity">实体</param>
    public void UnregisterEntity(Entity entity)
    {
        _renderSystem.UnregisterEntity(entity);
    }

    /// <summary>
    /// 渲染所有实体
    /// </summary>
    /// <param name="viewMatrix">视图矩阵</param>
    /// <param name="projectionMatrix">投影矩阵</param>
    public void Render(Matrix4 viewMatrix, Matrix4 projectionMatrix)
    {
        _renderSystem.Render(viewMatrix, projectionMatrix);
    }

    /// <summary>
    /// 清理所有实体
    /// </summary>
    public void Clear()
    {
        _renderSystem.Clear();
    }
}
