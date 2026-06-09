namespace SmallFuturesLab.Core.Filtering;

/// <summary>
/// 品种过滤测算场景。
/// </summary>
public sealed record FilterScenario
{
    /// <summary>测算手数。当前小资金阶段通常为 1 手。</summary>
    public int Lots { get; init; } = 1;

    /// <summary>止损距离，单位为 tick。</summary>
    public int StopTicks { get; init; }

    /// <summary>预估总滑点，单位为 tick。</summary>
    public int SlippageTicks { get; init; }
}
