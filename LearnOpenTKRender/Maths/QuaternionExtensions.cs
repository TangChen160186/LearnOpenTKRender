using System.Numerics;

namespace LearnOpenTKRender.Maths;

public static class QuaternionExtensions
{
    public static Vector3 ToEulerAngles(this Quaternion q)
    {
        Vector3 angles = new();

        // yaw / y (绕Y轴旋转)
        float siny_cosp = 2 * (q.W * q.Y + q.Z * q.X);
        float cosy_cosp = 1 - 2 * (q.Y * q.Y + q.Z * q.Z);
        angles.Y = MathF.Atan2(siny_cosp, cosy_cosp);

        // pitch / x (绕X轴旋转)
        float sinp = 2 * (q.W * q.X - q.Y * q.Z);
        if (MathF.Abs(sinp) >= 1)
        {
            angles.X = MathF.CopySign(MathF.PI / 2, sinp);
        }
        else
        {
            angles.X = MathF.Asin(sinp);
           
        }

        // roll / z (绕Z轴旋转)
        float sinr_cosp = 2 * (q.W * q.Z + q.X * q.Y);
        float cosr_cosp = 1 - 2 * (q.X * q.X + q.Z * q.Z);
        angles.Z = MathF.Atan2(sinr_cosp, cosr_cosp);

        return angles;
    }

    /// <summary>
    /// 从欧拉角创建四元数（YXZ顺序）
    /// </summary>
    public static Quaternion CreateFromEulerAngles(Vector3 eulerAngles)
    {
        return Quaternion.CreateFromYawPitchRoll(eulerAngles.Y, eulerAngles.X, eulerAngles.Z);
    }

    /// <summary>
    /// 创建朝向指定方向的四元数
    /// </summary>
    public static Quaternion CreateLookRotation(Vector3 forward, Vector3 up)
    {
        forward = Vector3.Normalize(forward);
        Vector3 right = Vector3.Normalize(Vector3.Cross(up, forward));
        up = Vector3.Cross(forward, right);

        Matrix4x4 rotationMatrix = new Matrix4x4(
            right.X, right.Y, right.Z, 0,
            up.X, up.Y, up.Z, 0,
            forward.X, forward.Y, forward.Z, 0,
            0, 0, 0, 1
        );

        return Quaternion.CreateFromRotationMatrix(rotationMatrix);
    }

    /// <summary>
    /// 创建朝向指定方向的四元数（使用默认上方向）
    /// </summary>
    public static Quaternion CreateLookRotation(Vector3 forward)
    {
        return CreateLookRotation(forward, Vector3.UnitY);
    }

    /// <summary>
    /// 获取四元数的前方向向量
    /// </summary>
    public static Vector3 GetForward(this Quaternion q)
    {
        return Vector3.Transform(Vector3.UnitZ, q);
    }

    /// <summary>
    /// 获取四元数的右方向向量
    /// </summary>
    public static Vector3 GetRight(this Quaternion q)
    {
        return Vector3.Transform(Vector3.UnitX, q);
    }

    /// <summary>
    /// 获取四元数的上方向向量
    /// </summary>
    public static Vector3 GetUp(this Quaternion q)
    {
        return Vector3.Transform(Vector3.UnitY, q);
    }

    /// <summary>
    /// 计算两个四元数之间的角度差（弧度）
    /// </summary>
    public static float AngleBetween(this Quaternion a, Quaternion b)
    {
        float dot = Quaternion.Dot(a, b);
        return MathF.Acos(MathF.Min(MathF.Abs(dot), 1.0f)) * 2.0f;
    }

    /// <summary>
    /// 绕任意轴旋转四元数
    /// </summary>
    public static Quaternion RotateAround(this Quaternion q, Vector3 axis, float angleRadians)
    {
        Quaternion rotation = Quaternion.CreateFromAxisAngle(axis, angleRadians);
        return rotation * q;
    }

    /// <summary>
    /// 朝向目标的旋转（从当前方向到目标方向）
    /// </summary>
    public static Quaternion LookTowards(Vector3 from, Vector3 to)
    {
        Vector3 forward = Vector3.Normalize(to - from);
        return CreateLookRotation(forward);
    }

    /// <summary>
    /// 创建从一个方向到另一个方向的旋转
    /// </summary>
    public static Quaternion FromToRotation(Vector3 fromDirection, Vector3 toDirection)
    {
        fromDirection = Vector3.Normalize(fromDirection);
        toDirection = Vector3.Normalize(toDirection);

        float dot = Vector3.Dot(fromDirection, toDirection);

        // 如果方向相同
        if (dot >= 1.0f - MathHelper.Epsilon)
            return Quaternion.Identity;

        // 如果方向相反
        if (dot <= -1.0f + MathHelper.Epsilon)
        {
            Vector3 axis = Vector3.Cross(Vector3.UnitX, fromDirection);
            if (axis.LengthSquared() < MathHelper.Epsilon)
                axis = Vector3.Cross(Vector3.UnitY, fromDirection);
            return Quaternion.CreateFromAxisAngle(Vector3.Normalize(axis), MathF.PI);
        }

        Vector3 cross = Vector3.Cross(fromDirection, toDirection);
        float w = MathF.Sqrt((1.0f + dot) * 2.0f);
        float invW = 1.0f / w;

        return new Quaternion(cross * invW, w * 0.5f);
    }

    /// <summary>
    /// 球面线性插值（更精确的实现）
    /// </summary>
    public static Quaternion SlerpUnclamped(Quaternion a, Quaternion b, float t)
    {
        float dot = Quaternion.Dot(a, b);

        // 选择最短路径
        if (dot < 0.0f)
        {
            b = new Quaternion(-b.X, -b.Y, -b.Z, -b.W);
            dot = -dot;
        }

        if (dot > 0.9995f)
        {
            // 线性插值（避免除零）
            return Quaternion.Normalize(Quaternion.Lerp(a, b, t));
        }

        float theta = MathF.Acos(dot);
        float sinTheta = MathF.Sin(theta);
        float wa = MathF.Sin((1.0f - t) * theta) / sinTheta;
        float wb = MathF.Sin(t * theta) / sinTheta;

        return new Quaternion(
            wa * a.X + wb * b.X,
            wa * a.Y + wb * b.Y,
            wa * a.Z + wb * b.Z,
            wa * a.W + wb * b.W
        );
    }

    /// <summary>
    /// 检查四元数是否有效（非零且有限）
    /// </summary>
    public static bool IsValid(this Quaternion q)
    {
        return float.IsFinite(q.X) && float.IsFinite(q.Y) &&
               float.IsFinite(q.Z) && float.IsFinite(q.W) &&
               q.LengthSquared() > MathHelper.Epsilon;
    }

    /// <summary>
    /// 获取四元数的共轭
    /// </summary>
    public static Quaternion GetConjugate(this Quaternion q)
    {
        return new Quaternion(-q.X, -q.Y, -q.Z, q.W);
    }

    /// <summary>
    /// 限制四元数的旋转角度
    /// </summary>
    public static Quaternion ClampRotation(this Quaternion q, float maxAngleRadians)
    {
        float angle = 2.0f * MathF.Acos(MathF.Abs(q.W));
        if (angle <= maxAngleRadians)
            return q;

        float t = maxAngleRadians / angle;
        return Quaternion.Slerp(Quaternion.Identity, q, t);
    }
}