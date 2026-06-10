namespace SmallFuturesLab.Core;

/// <summary>
/// 表示本次品种过滤测算条件。
/// </summary>
public sealed class FilterCondition
{
    /// <summary>
    /// 本次测算假设的止损距离，单位 tick。必须大于 0。
    /// </summary>
    public int StopTicks { get; set; }

    /// <summary>
    /// 本次测算假设的总滑点，单位 tick。必须大于等于 0。
    /// </summary>
    public int SlippageTicks { get; set; }

    /// <summary>
    /// 测算手数。默认 1 手。必须大于 0。
    /// </summary>
    public int Lots { get; set; } = 1;
}
