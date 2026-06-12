using System.Runtime.CompilerServices;

namespace SmallFuturesLab.Core.Risk;

/// <summary>
/// 领域值对象的最小合法性校验工具。
///
/// 这个类只做简单、不含外部状态的参数检查，
/// 用于让值对象构造方法保持清晰。
/// </summary>
internal static class Ensure
{
    /// <summary>
    /// 确保数值大于 0。
    /// </summary>
    public static double Positive(double value, string message, [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        if (value <= 0)
        {
            throw new ArgumentOutOfRangeException(paramName, value, message);
        }

        return value;
    }

    /// <summary>
    /// 确保数值不小于 0。
    /// </summary>
    public static double NonNegative(double value, string message, [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        if (value < 0)
        {
            throw new ArgumentOutOfRangeException(paramName, value, message);
        }

        return value;
    }

    /// <summary>
    /// 确保整数不小于 0。
    /// </summary>
    public static int NonNegative(int value, string message, [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        if (value < 0)
        {
            throw new ArgumentOutOfRangeException(paramName, value, message);
        }

        return value;
    }

    /// <summary>
    /// 确保数值是 (0, 1] 范围内的比例。
    /// </summary>
    public static double Ratio(double value, string message, [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        if (value <= 0 || value > 1)
        {
            throw new ArgumentOutOfRangeException(paramName, value, message);
        }

        return value;
    }

    /// <summary>
    /// 确保整数不小于指定下限。
    /// </summary>
    public static int AtLeast(int value, int minimum, string message, [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        if (value < minimum)
        {
            throw new ArgumentOutOfRangeException(paramName, value, message);
        }

        return value;
    }
}
