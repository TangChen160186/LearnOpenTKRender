using System.Numerics;

namespace LearnOpenTKRender.Maths;

/// <summary>
/// 表示3D变换，包括位置、旋转和缩放，并支持父子级关系
/// 使用事件机制进行即时更新
/// </summary>
public class FTransform
{
    private Vector3 _localPosition;
    private Quaternion _localRotation;
    private Vector3 _localScale;

    private FTransform? _parent;
    private readonly List<FTransform> _children = new();

    // 存储计算后的世界变换矩阵
    private Matrix4x4 _localToWorldMatrix;

    #region 事件定义

    // 当本地变换发生变化时触发的事件
    public event Action<FTransform>? TransformChanged;

    // 当父级发生变化时触发的事件
    public event Action<FTransform, FTransform?>? ParentChanged;

    #endregion

    /// <summary>
    /// 本地位置（相对于父级）
    /// </summary>
    public Vector3 LocalPosition
    {
        get => _localPosition;
        set
        {
            if (_localPosition != value)
            {
                _localPosition = value;
                UpdateMatrices();
                NotifyTransformChanged();
            }
        }
    }

    /// <summary>
    /// 本地旋转（相对于父级，以四元数表示）
    /// </summary>
    public Quaternion LocalRotation
    {
        get => _localRotation;
        set
        {
            if (_localRotation != value)
            {
                _localRotation = Quaternion.Normalize(value);
                UpdateMatrices();
                NotifyTransformChanged();
            }
        }
    }

    /// <summary>
    /// 本地欧拉角旋转（以弧度表示，按YXZ顺序：Y=yaw, X=pitch, Z=roll）
    /// </summary>
    public Vector3 LocalEulerAngles
    {
        get => _localRotation.ToEulerAngles();
        set
        {
            LocalRotation = Quaternion.CreateFromYawPitchRoll(value.Y, value.X, value.Z);
        }
    }

    /// <summary>
    /// 本地缩放（相对于父级）
    /// </summary>
    public Vector3 LocalScale
    {
        get => _localScale;
        set
        {
            if (_localScale != value)
            {
                _localScale = value;
                UpdateMatrices();
                NotifyTransformChanged();
            }
        }
    }

    /// <summary>
    /// 全局位置（世界坐标）
    /// </summary>
    public Vector3 Position
    {
        get => _localToWorldMatrix.Translation;
        set
        {
            if (_parent != null)
            {
                // 计算相对于父级的本地位置
                LocalPosition = _parent.WorldToLocalPoint(value);
            }
            else
            {
                LocalPosition = value;
            }
        }
    }

    /// <summary>
    /// 全局旋转（世界坐标系中的四元数）
    /// </summary>
    public Quaternion Rotation
    {
        get
        {
            // 从变换矩阵中提取旋转
            return Quaternion.CreateFromRotationMatrix(_localToWorldMatrix);
        }
        set
        {
            if (_parent != null)
            {
                // 相对于父级的旋转
                Quaternion parentWorldRotationInv = Quaternion.Inverse(_parent.Rotation);
                LocalRotation = parentWorldRotationInv * value;
            }
            else
            {
                LocalRotation = value;
            }
        }
    }

    /// <summary>
    /// 全局欧拉角旋转（以弧度表示，按YXZ顺序）
    /// </summary>
    public Vector3 EulerAngles
    {
        get => Rotation.ToEulerAngles();
        set
        {
            Rotation = Quaternion.CreateFromYawPitchRoll(value.Y, value.X, value.Z);
        }
    }

    /// <summary>
    /// 全局缩放（世界坐标）
    /// </summary>
    public Vector3 Scale
    {
        get
        {
            // 从变换矩阵中提取缩放
            Vector3 scale = new Vector3(
                _localToWorldMatrix.M11,
                _localToWorldMatrix.M22,
                _localToWorldMatrix.M33
            );
            return scale;
        }
        set
        {
            if (_parent != null)
            {
                // 相对于父级的缩放
                Vector3 parentScale = _parent.Scale;
                if (parentScale.X != 0 && parentScale.Y != 0 && parentScale.Z != 0)
                {
                    LocalScale = new Vector3(
                        value.X / parentScale.X,
                        value.Y / parentScale.Y,
                        value.Z / parentScale.Z
                    );
                }
                else
                {
                    LocalScale = value;
                }
            }
            else
            {
                LocalScale = value;
            }
        }
    }

    /// <summary>
    /// 获取父级变换
    /// </summary>
    public FTransform? Parent => _parent;

    /// <summary>
    /// 获取子级变换列表（只读）
    /// </summary>
    public IReadOnlyList<FTransform> Children => _children;

    /// <summary>
    /// 获取本地到世界的变换矩阵（从本地空间到世界空间）
    /// </summary>
    public Matrix4x4 LocalToWorldMatrix => _localToWorldMatrix;

    /// <summary>
    /// 获取世界到本地的变换矩阵（从世界空间到本地空间）
    /// </summary>
    public Matrix4x4 WorldToLocalMatrix
    {
        get
        {
            Matrix4x4.Invert(_localToWorldMatrix, out Matrix4x4 inverse);
            return inverse;
        }
    }

    /// <summary>
    /// 创建默认变换（位置为原点，无旋转，缩放为1）
    /// </summary>
    public FTransform()
    {
        _localPosition = Vector3.Zero;
        _localRotation = Quaternion.Identity;
        _localScale = Vector3.One;
        _localToWorldMatrix = Matrix4x4.Identity;
        UpdateMatrices();
    }

    /// <summary>
    /// 创建指定参数的变换
    /// </summary>
    public FTransform(Vector3 position, Quaternion rotation, Vector3? scale = null)
    {
        _localPosition = position;
        _localRotation = rotation;
        _localScale = scale ?? Vector3.One;
        _localToWorldMatrix = Matrix4x4.Identity;
        UpdateMatrices();
    }

    /// <summary>
    /// 创建指定欧拉角参数的变换
    /// </summary>
    public FTransform(Vector3 position, Vector3 eulerAngles, Vector3? scale = null)
        : this(position, Quaternion.CreateFromYawPitchRoll(eulerAngles.Y, eulerAngles.X, eulerAngles.Z), scale)
    {
    }

    /// <summary>
    /// 当父变换发生变化时的处理
    /// </summary>
    private void OnParentTransformChanged(FTransform parent)
    {
        // 父级变化后立即更新自身矩阵
        UpdateMatrices();
        // 通知自身也发生了变化
        NotifyTransformChanged();
    }

    /// <summary>
    /// 更新本地到世界的变换矩阵
    /// </summary>
    private void UpdateMatrices()
    {
        // 计算本地矩阵（位置、旋转、缩放）
        Matrix4x4 scaleMatrix = Matrix4x4.CreateScale(_localScale);
        Matrix4x4 rotationMatrix = Matrix4x4.CreateFromQuaternion(_localRotation);
        Matrix4x4 translationMatrix = Matrix4x4.CreateTranslation(_localPosition);

        // 组合本地矩阵（先缩放，再旋转，最后平移）
        Matrix4x4 localMatrix = scaleMatrix * rotationMatrix * translationMatrix;

        // 如果有父级，组合父级的变换
        if (_parent != null)
        {
            _localToWorldMatrix = localMatrix * _parent.LocalToWorldMatrix;
        }
        else
        {
            _localToWorldMatrix = localMatrix;
        }
    }

    /// <summary>
    /// 通知变换发生变化
    /// </summary>
    private void NotifyTransformChanged()
    {
        TransformChanged?.Invoke(this);

        // 变换改变会级联到所有子物体
        foreach (var child in _children)
        {
            child.UpdateMatrices();
            child.NotifyTransformChanged();
        }
    }

    /// <summary>
    /// 将点从本地空间转换到世界空间
    /// </summary>
    public Vector3 LocalToWorldPoint(Vector3 point)
    {
        return Vector3.Transform(point, _localToWorldMatrix);
    }

    /// <summary>
    /// 将向量从本地空间转换到世界空间
    /// </summary>
    public Vector3 LocalToWorldVector(Vector3 vector)
    {
        return Vector3.TransformNormal(vector, _localToWorldMatrix);
    }

    /// <summary>
    /// 将点从世界空间转换到本地空间
    /// </summary>
    public Vector3 WorldToLocalPoint(Vector3 point)
    {
        return Vector3.Transform(point, WorldToLocalMatrix);
    }

    /// <summary>
    /// 将向量从世界空间转换到本地空间
    /// </summary>
    public Vector3 WorldToLocalVector(Vector3 vector)
    {
        return Vector3.TransformNormal(vector, WorldToLocalMatrix);
    }

    /// <summary>
    /// 将方向从本地空间转换到世界空间（不考虑缩放）
    /// </summary>
    public Vector3 LocalToWorldDirection(Vector3 direction)
    {
        // 使用旋转矩阵部分变换方向，忽略位置和缩放的影响
        return Vector3.Transform(direction, _localRotation);
    }

    /// <summary>
    /// 将方向从世界空间转换到本地空间（不考虑缩放）
    /// </summary>
    public Vector3 WorldToLocalDirection(Vector3 direction)
    {
        return Vector3.Transform(direction, Quaternion.Inverse(_localRotation));
    }

    /// <summary>
    /// 设置父级变换
    /// </summary>
    /// <param name="parent">新的父级变换</param>
    /// <param name="worldPositionStays">是否保持世界位置不变</param>
    public void SetParent(FTransform? parent, bool worldPositionStays = true)
    {
        if (_parent == parent)
            return;

        Vector3 worldPosition = Position;
        Quaternion worldRotation = Rotation;
        Vector3 worldScale = Scale;

        // 从旧父级移除
        if (_parent != null)
        {
            _parent.TransformChanged -= OnParentTransformChanged;
            _parent._children.Remove(this);
        }

        // 设置新父级
        FTransform? oldParent = _parent;
        _parent = parent;

        // 添加到新父级
        if (_parent != null)
        {
            _parent.TransformChanged += OnParentTransformChanged;
            if (!_parent._children.Contains(this))
                _parent._children.Add(this);
        }

        // 如果需要保持世界位置不变
        if (worldPositionStays)
        {
            Position = worldPosition;
            Rotation = worldRotation;
            Scale = worldScale;
        }
        else
        {
            // 更新矩阵以反映新的父子关系
            UpdateMatrices();
            NotifyTransformChanged();
        }

        // 触发父级变化事件
        ParentChanged?.Invoke(this, oldParent);
    }

    /// <summary>
    /// 添加子级变换
    /// </summary>
    public void AddChild(FTransform child)
    {
        child.SetParent(this);
    }

    /// <summary>
    /// 移除子级变换
    /// </summary>
    public void RemoveChild(FTransform child)
    {
        if (child._parent == this)
        {
            child.SetParent(null);
        }
    }

    /// <summary>
    /// 获取此变换的根变换
    /// </summary>
    public FTransform Root
    {
        get
        {
            FTransform current = this;
            while (current._parent != null)
            {
                current = current._parent;
            }
            return current;
        }
    }

    /// <summary>
    /// 在本地空间中平移
    /// </summary>
    public void Translate(Vector3 translation, Space relativeTo = Space.Self)
    {
        if (relativeTo == Space.Self)
        {
            // 相对于本地空间平移
            LocalPosition += LocalToWorldDirection(translation);
        }
        else
        {
            // 相对于世界空间平移
            Position += translation;
        }
    }

    /// <summary>
    /// 旋转指定欧拉角（弧度）
    /// </summary>
    public void Rotate(Vector3 eulerAngles, Space relativeTo = Space.Self)
    {
        Quaternion rotation = Quaternion.CreateFromYawPitchRoll(eulerAngles.Y, eulerAngles.X, eulerAngles.Z);

        if (relativeTo == Space.Self)
        {
            LocalRotation = LocalRotation * rotation;
        }
        else
        {
            Rotation = rotation * Rotation;
        }
    }

    /// <summary>
    /// 绕指定轴旋转指定角度（弧度）
    /// </summary>
    public void Rotate(Vector3 axis, float angleRadians, Space relativeTo = Space.Self)
    {
        Quaternion rotation = Quaternion.CreateFromAxisAngle(axis, angleRadians);

        if (relativeTo == Space.Self)
        {
            // 相对于本地空间旋转
            LocalRotation = LocalRotation * rotation;
        }
        else
        {
            // 相对于世界空间旋转
            Rotation = rotation * Rotation;
        }
    }

    /// <summary>
    /// 应用缩放
    /// </summary>
    public void ApplyScale(Vector3 scale)
    {
        LocalScale = new Vector3(
            LocalScale.X * scale.X,
            LocalScale.Y * scale.Y,
            LocalScale.Z * scale.Z
        );
    }

    /// <summary>
    /// 应用统一缩放
    /// </summary>
    public void ApplyScale(float uniformScale)
    {
        LocalScale = LocalScale * uniformScale;
    }

    /// <summary>
    /// 朝向一个目标点旋转
    /// </summary>
    public void LookAt(Vector3 target, Vector3 worldUp)
    {
        Vector3 forward = Vector3.Normalize(target - Position);

        // 避免前向量与上向量平行的情况
        if (Vector3.Dot(forward, worldUp) > 0.9999f)
        {
            worldUp = Vector3.UnitX;
        }

        Vector3 right = Vector3.Normalize(Vector3.Cross(worldUp, forward));
        Vector3 up = Vector3.Cross(forward, right);

        // 创建旋转矩阵
        Matrix4x4 rotationMatrix = new Matrix4x4(
            right.X, right.Y, right.Z, 0,
            up.X, up.Y, up.Z, 0,
            forward.X, forward.Y, forward.Z, 0,
            0, 0, 0, 1
        );

        Rotation = Quaternion.CreateFromRotationMatrix(rotationMatrix);
    }
    /// <summary>
    /// 全局正方向（Z轴正向）
    /// </summary>
    public Vector3 Forward => LocalToWorldDirection(Vector3.UnitZ);
    /// <summary>
    /// 全局后方向（Z轴负向）
    /// </summary>
    public Vector3 Backward => LocalToWorldDirection(-Vector3.UnitZ);

    /// <summary>
    /// 全局右方向（X轴正向）
    /// </summary>
    public Vector3 Right => LocalToWorldDirection(Vector3.UnitX);

    /// <summary>
    /// 全局左方向（X轴负向）
    /// </summary>
    public Vector3 Left => LocalToWorldDirection(-Vector3.UnitX);

    /// <summary>
    /// 全局上方向（Y轴正向）
    /// </summary>
    public Vector3 Up => LocalToWorldDirection(Vector3.UnitY);

    /// <summary>
    /// 全局下方向（Y轴负向）
    /// </summary>
    public Vector3 Down => LocalToWorldDirection(-Vector3.UnitY);
}

/// <summary>
/// 表示空间参考系统
/// </summary>
public enum Space
{
    /// <summary>
    /// 相对于物体自身的本地空间
    /// </summary>
    Self,

    /// <summary>
    /// 相对于世界空间
    /// </summary>
    World
}