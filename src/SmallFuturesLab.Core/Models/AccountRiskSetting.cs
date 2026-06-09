namespace SmallFuturesLab.Core.Models;

/// <summary>
/// 小资金账户测算参数。
/// </summary>
public record AccountRiskSetting
{
    /// <summary>账户权益。</summary>
    public double AccountEquity { get; init; } = 10_000;

    /// <summary>常规单笔风险上限。</summary>
    public double NormalRiskRate { get; init; } = 0.010;

    /// <summary>极限单笔风险上限。</summary>
    public double ExtremeRiskRate { get; init; } = 0.020;

    /// <summary>推荐保证金占账户比例上限。</summary>
    public double PreferredMarginRateOfEquity { get; init; } = 0.400;

    /// <summary>极限保证金占账户比例上限。</summary>
    public double ExtremeMarginRateOfEquity { get; init; } = 0.500;

    /// <summary>推荐成本占止损风险比例上限。</summary>
    public double PreferredCostRatio { get; init; } = 0.200;

    /// <summary>极限成本占止损风险比例上限。</summary>
    public double ExtremeCostRatio { get; init; } = 0.300;

    /// <summary>止损 tick 数。</summary>
    public int StopTicks { get; init; } = 10;

    /// <summary>预估滑点 tick 数。</summary>
    public int SlippageTicks { get; init; } = 2;

    /// <summary>测算手数。当前阶段固定为 1 手。</summary>
    public int Lots { get; init; } = 1;
}
