using System.Numerics;

namespace LearnOpenTKRender.Maths;

public static class MathHelper
{
    #region 数学常量

    /// <summary>
    /// π的值 (3.14159265358979...)
    /// </summary>
    public const float Pi = MathF.PI;

    /// <summary>
    /// 2π的值
    /// </summary>
    public const float TwoPi = 2f * Pi;

    /// <summary>
    /// π/2的值
    /// </summary>
    public const float HalfPi = Pi / 2f;

    /// <summary>
    /// π/4的值
    /// </summary>
    public const float QuarterPi = Pi / 4f;

    /// <summary>
    /// 1/π的值
    /// </summary>
    public const float InversePi = 1f / Pi;

    /// <summary>
    /// 自然对数的底数 e (2.71828...)
    /// </summary>
    public const float E = MathF.E;

    /// <summary>
    /// 黄金比例 (1.61803398875...)
    /// </summary>
    public const float GoldenRatio = 1.61803398875f;

    /// <summary>
    /// 弧度到角度的转换因子
    /// </summary>
    public const float RadToDeg = 180f / Pi;

    /// <summary>
    /// 角度到弧度的转换因子
    /// </summary>
    public const float DegToRad = Pi / 180f;

    /// <summary>
    /// 表示一个很小的浮点数，用于浮点比较
    /// </summary>
    public const float Epsilon = 1e-5f;

    #endregion




    #region 角度和弧度操作

    /// <summary>
    /// 将角度转换为弧度
    /// </summary>
    public static float ToRadians(float degrees) => degrees * DegToRad;

    /// <summary>
    /// 将弧度转换为角度
    /// </summary>
    public static float ToDegrees(float radians) => radians * RadToDeg;

    /// <summary>
    /// 将角度规范化到[-180, 180]范围
    /// </summary>
    public static float NormalizeAngleDegrees(float degrees)
    {
        degrees = degrees % 360f;
        if (degrees > 180f) degrees -= 360f;
        else if (degrees <= -180f) degrees += 360f;
        return degrees;
    }

    /// <summary>
    /// 将弧度规范化到[-π, π]范围
    /// </summary>
    public static float NormalizeAngleRadians(float radians)
    {
        radians = radians % TwoPi;
        if (radians > Pi) radians -= TwoPi;
        else if (radians <= -Pi) radians += TwoPi;
        return radians;
    }

    /// <summary>
    /// 计算两个角度（以度为单位）之间的最短差值
    /// </summary>
    public static float AngleDifferenceRadians(float a, float b)
    {
        float diff = NormalizeAngleRadians(b - a);
        return diff;
    }

    /// <summary>
    /// 计算两个角度（以度为单位）之间的最短差值
    /// </summary>
    public static float AngleDifferenceDegrees(float a, float b)
    {
        float diff = NormalizeAngleDegrees(b - a);
        return diff;
    }

    #endregion



    #region 比较函数

    /// <summary>
    /// 检查两个浮点数是否近似相等
    /// </summary>
    public static bool Approximately(float a, float b)
    {
        return MathF.Abs(a - b) < Epsilon;
    }

    /// <summary>
    /// 检查浮点数是否近似为零
    /// </summary>
    public static bool ApproximatelyZero(float a)
    {
        return MathF.Abs(a) < Epsilon;
    }

    /// <summary>
    /// 比较两个浮点数大小，考虑浮点精度
    /// </summary>
    /// <returns>
    /// -1 如果a明显小于b
    /// 0 如果a近似等于b
    /// 1 如果a明显大于b
    /// </returns>
    public static int Compare(float a, float b)
    {
        if (Approximately(a, b)) return 0;
        return a < b ? -1 : 1;
    }

    #endregion

    #region 随机数生成

    private static readonly Random _random = new Random();

    /// <summary>
    /// 返回[0,1)范围内的随机浮点数
    /// </summary>
    public static float Random() => (float)_random.NextDouble();

    /// <summary>
    /// 返回[0,max)范围内的随机浮点数
    /// </summary>
    public static float Random(float max) => (float)_random.NextDouble() * max;

    /// <summary>
    /// 返回[min,max)范围内的随机浮点数
    /// </summary>
    public static float Random(float min, float max) => min + (float)_random.NextDouble() * (max - min);

    /// <summary>
    /// 返回[min,max]范围内的随机整数
    /// </summary>
    public static int RandomInt(int min, int max) => _random.Next(min, max + 1);

    /// <summary>
    /// 返回[0,max)范围内的随机整数
    /// </summary>
    public static int RandomInt(int max) => _random.Next(max);

    /// <summary>
    /// 返回一个随机布尔值
    /// </summary>
    public static bool RandomBool() => _random.NextDouble() >= 0.5;

    /// <summary>
    /// 根据给定概率返回布尔值
    /// </summary>
    /// <param name="probability">返回true的概率[0,1]</param>
    public static bool RandomChance(float probability) => _random.NextDouble() < probability;

    /// <summary>
    /// 返回一个随机方向的二维单位向量
    /// </summary>
    public static Vector2 RandomDirection2D()
    {
        float angle = Random(0, TwoPi);
        return new Vector2(MathF.Cos(angle), MathF.Sin(angle));
    }

    /// <summary>
    /// 返回一个[0,1)×[0,1)范围内的随机二维向量
    /// </summary>
    public static Vector2 RandomVector2() => new(Random(), Random());

    /// <summary>
    /// 返回一个[-1,1)×[-1,1)范围内的随机二维向量
    /// </summary>
    public static Vector2 RandomVector2Signed() => new(Random(-1f, 1f), Random(-1f, 1f));

    /// <summary>
    /// 返回一个在圆内的随机点
    /// </summary>
    /// <param name="radius">圆的半径</param>
    /// <param name="minDistance">到圆心的最小距离（用于环形分布）</param>
    public static Vector2 RandomPointInCircle(float radius, float minDistance = 0f)
    {
        float r = MathF.Sqrt(Random(minDistance * minDistance, radius * radius));
        float theta = Random(0, TwoPi);
        return new Vector2(r * MathF.Cos(theta), r * MathF.Sin(theta));
    }

    #endregion




    #region 限制和包装函数

    /// <summary>
    /// 将值限制在[min, max]范围内
    /// </summary>
    public static float Clamp(float value, float min, float max)
    {
        if (value < min) return min;
        if (value > max) return max;
        return value;
    }

    /// <summary>
    /// 将值限制在[0, 1]范围内
    /// </summary>
    public static float Clamp01(float value)
    {
        if (value < 0f) return 0f;
        if (value > 1f) return 1f;
        return value;
    }

    /// <summary>
    /// 将值重复映射到[min, max)范围
    /// </summary>
    public static float Repeat(float value, float min, float max)
    {
        float range = max - min;
        if (range == 0f) return min;

        value = (value - min) % range;
        if (value < 0f) value += range;
        return value + min;
    }

    /// <summary>
    /// 在给定范围内进行Ping-Pong映射（从min到max再回到min）
    /// </summary>
    public static float PingPong(float value, float min, float max)
    {
        float range = max - min;
        if (range == 0f) return min;

        value = Repeat(value - min, 0f, range * 2f);
        return min + (value < range ? value : 2f * range - value);
    }

    /// <summary>
    /// 在[0,max]范围内进行Ping-Pong映射
    /// </summary>
    public static float PingPong(float value, float max) => PingPong(value, 0f, max);

    #endregion
}