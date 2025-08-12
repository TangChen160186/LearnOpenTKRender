using System.Numerics;
using LearnOpenTKRender.Maths;

namespace LearnOpenTKRender.Ecs;

internal class TransformComponent : ComponentBase
{
    public FTransform Transform { get; set; }

    public TransformComponent(Entity entity) : base(entity)
    {
        Transform = new FTransform();
    }

    #region 位置快捷访问

    /// <summary>
    /// 本地位置（相对于父级）
    /// </summary>
    public Vector3 LocalPosition
    {
        get => Transform.LocalPosition;
        set => Transform.LocalPosition = value;
    }

    /// <summary>
    /// 世界位置
    /// </summary>
    public Vector3 Position
    {
        get => Transform.Position;
        set => Transform.Position = value;
    }

    #endregion

    #region 旋转快捷访问

    /// <summary>
    /// 本地旋转（四元数）
    /// </summary>
    public Quaternion LocalRotation
    {
        get => Transform.LocalRotation;
        set => Transform.LocalRotation = value;
    }

    /// <summary>
    /// 本地欧拉角（弧度）
    /// </summary>
    public Vector3 LocalEulerAngles
    {
        get => Transform.LocalEulerAngles;
        set => Transform.LocalEulerAngles = value;
    }

    /// <summary>
    /// 世界旋转（四元数）
    /// </summary>
    public Quaternion Rotation
    {
        get => Transform.Rotation;
        set => Transform.Rotation = value;
    }

    /// <summary>
    /// 世界欧拉角（弧度）
    /// </summary>
    public Vector3 EulerAngles
    {
        get => Transform.EulerAngles;
        set => Transform.EulerAngles = value;
    }

    #endregion

    #region 缩放快捷访问

    /// <summary>
    /// 本地缩放
    /// </summary>
    public Vector3 LocalScale
    {
        get => Transform.LocalScale;
        set => Transform.LocalScale = value;
    }

    /// <summary>
    /// 世界缩放
    /// </summary>
    public Vector3 Scale
    {
        get => Transform.Scale;
        set => Transform.Scale = value;
    }

    #endregion

    #region 方向向量快捷访问

    /// <summary>
    /// 前方向（世界空间）
    /// </summary>
    public Vector3 Forward => Transform.Forward;

    /// <summary>
    /// 后方向（世界空间）
    /// </summary>
    public Vector3 Backward => Transform.Backward;

    /// <summary>
    /// 右方向（世界空间）
    /// </summary>
    public Vector3 Right => Transform.Right;

    /// <summary>
    /// 左方向（世界空间）
    /// </summary>
    public Vector3 Left => Transform.Left;

    /// <summary>
    /// 上方向（世界空间）
    /// </summary>
    public Vector3 Up => Transform.Up;

    /// <summary>
    /// 下方向（世界空间）
    /// </summary>
    public Vector3 Down => Transform.Down;

    #endregion

    #region 矩阵快捷访问

    /// <summary>
    /// 本地到世界变换矩阵
    /// </summary>
    public Matrix4x4 LocalToWorldMatrix => Transform.LocalToWorldMatrix;

    /// <summary>
    /// 世界到本地变换矩阵
    /// </summary>
    public Matrix4x4 WorldToLocalMatrix => Transform.WorldToLocalMatrix;

    #endregion

    #region 层级关系快捷访问

    /// <summary>
    /// 父级变换
    /// </summary>
    public FTransform? Parent => Transform.Parent;

    /// <summary>
    /// 子级变换列表
    /// </summary>
    public IReadOnlyList<FTransform> Children => Transform.Children;

    /// <summary>
    /// 根变换
    /// </summary>
    public FTransform Root => Transform.Root;

    #endregion

    #region 变换操作快捷方法

    /// <summary>
    /// 平移
    /// </summary>
    public void Translate(Vector3 translation, Space relativeTo = Space.Self)
    {
        Transform.Translate(translation, relativeTo);
    }

    /// <summary>
    /// 旋转（欧拉角，弧度）
    /// </summary>
    public void Rotate(Vector3 eulerAngles, Space relativeTo = Space.Self)
    {
        Transform.Rotate(eulerAngles, relativeTo);
    }

    /// <summary>
    /// 绕轴旋转
    /// </summary>
    public void Rotate(Vector3 axis, float angleRadians, Space relativeTo = Space.Self)
    {
        Transform.Rotate(axis, angleRadians, relativeTo);
    }

    /// <summary>
    /// 应用缩放
    /// </summary>
    public void ApplyScale(Vector3 scale)
    {
        Transform.ApplyScale(scale);
    }

    /// <summary>
    /// 应用统一缩放
    /// </summary>
    public void ApplyScale(float uniformScale)
    {
        Transform.ApplyScale(uniformScale);
    }

    /// <summary>
    /// 朝向目标
    /// </summary>
    public void LookAt(Vector3 target, Vector3 worldUp)
    {
        Transform.LookAt(target, worldUp);
    }

    /// <summary>
    /// 朝向目标（使用默认上方向）
    /// </summary>
    public void LookAt(Vector3 target)
    {
        Transform.LookAt(target, Vector3.UnitY);
    }

    #endregion

    #region 空间转换快捷方法

    /// <summary>
    /// 点从本地空间转换到世界空间
    /// </summary>
    public Vector3 TransformPoint(Vector3 point)
    {
        return Transform.LocalToWorldPoint(point);
    }

    /// <summary>
    /// 向量从本地空间转换到世界空间
    /// </summary>
    public Vector3 TransformVector(Vector3 vector)
    {
        return Transform.LocalToWorldVector(vector);
    }

    /// <summary>
    /// 方向从本地空间转换到世界空间 
    /// </summary>
    public Vector3 TransformDirection(Vector3 direction)
    {
        return Transform.LocalToWorldDirection(direction);
    }

    /// <summary>
    /// 点从世界空间转换到本地空间
    /// </summary>
    public Vector3 InverseTransformPoint(Vector3 point)
    {
        return Transform.WorldToLocalPoint(point);
    }

    /// <summary>
    /// 向量从世界空间转换到本地空间
    /// </summary>
    public Vector3 InverseTransformVector(Vector3 vector)
    {
        return Transform.WorldToLocalVector(vector);
    }

    /// <summary>
    /// 方向从世界空间转换到本地空间
    /// </summary>
    public Vector3 InverseTransformDirection(Vector3 direction)
    {
        return Transform.WorldToLocalDirection(direction);
    }

    #endregion

    #region 层级管理快捷方法

    /// <summary>
    /// 设置父级
    /// </summary>
    public void SetParent(FTransform? parent, bool worldPositionStays = true)
    {
        Transform.SetParent(parent, worldPositionStays);
    }

    /// <summary>
    /// 设置父级（通过 TransformComponent）
    /// </summary>
    public void SetParent(TransformComponent? parentComponent, bool worldPositionStays = true)
    {
        Transform.SetParent(parentComponent?.Transform, worldPositionStays);
    }

    /// <summary>
    /// 添加子级
    /// </summary>
    public void AddChild(FTransform child)
    {
        Transform.AddChild(child);
    }

    /// <summary>
    /// 添加子级（通过 TransformComponent）
    /// </summary>
    public void AddChild(TransformComponent childComponent)
    {
        Transform.AddChild(childComponent.Transform);
    }

    /// <summary>
    /// 移除子级
    /// </summary>
    public void RemoveChild(FTransform child)
    {
        Transform.RemoveChild(child);
    }

    /// <summary>
    /// 移除子级（通过 TransformComponent）
    /// </summary>
    public void RemoveChild(TransformComponent childComponent)
    {
        Transform.RemoveChild(childComponent.Transform);
    }

    #endregion

    #region 事件订阅

    /// <summary>
    /// 变换改变事件
    /// </summary>
    public event Action<FTransform>? TransformChanged
    {
        add => Transform.TransformChanged += value;
        remove => Transform.TransformChanged -= value;
    }

    /// <summary>
    /// 父级改变事件
    /// </summary>
    public event Action<FTransform, FTransform?>? ParentChanged
    {
        add => Transform.ParentChanged += value;
        remove => Transform.ParentChanged -= value;
    }

    #endregion
}