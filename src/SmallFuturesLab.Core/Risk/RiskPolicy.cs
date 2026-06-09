namespace SmallFuturesLab.Core.Risk;

/// <summary>
/// 小资金品种过滤使用的风控阈值。
/// </summary>
public sealed record RiskPolicy
{
    /// <summary>单笔风险进入谨慎区的比例。</summary>
    public double CautionRiskRate { get; init; } = 0.01;

    /// <summary>单笔风险拒绝阈值。</summary>
    public double MaxRiskRate { get; init; } = 0.02;

    /// <summary>保证金占账户比例进入谨慎区的阈值。</summary>
    public double CautionMarginRateOfEquity { get; init; } = 0.30;

    /// <summary>保证金占账户比例拒绝阈值。</summary>
    public double MaxMarginRateOfEquity { get; init; } = 0.50;

    /// <summary>
    /// 成本占止损风险进入谨慎区的比例。
    /// CostRatio = (手续费 + 滑点成本) / 止损风险。
    /// </summary>
    public double CautionCostRatio { get; init; } = 0.20;

    /// <summary>
    /// 成本占止损风险的拒绝阈值。
    /// CostRatio = (手续费 + 滑点成本) / 止损风险。
    /// </summary>
    public double MaxCostRatio { get; init; } = 0.30;

    /// <summary>默认风控阈值。</summary>
    public static RiskPolicy Default { get; } = new();
}
