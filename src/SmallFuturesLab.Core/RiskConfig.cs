namespace SmallFuturesLab.Core;

/// <summary>
/// 账户风险配置。
/// 配置账户资金和风险阈值，用于品种过滤测算。
/// </summary>
public sealed record RiskConfig
{
    /// <summary>
    /// 账户权益。必须大于 0。
    /// </summary>
    public double AccountEquity { get; init; }

    /// <summary>
    /// 单笔风险进入谨慎区的比例。默认 1%。
    /// </summary>
    public double CautionRiskRate { get; init; } = 0.01;

    /// <summary>
    /// 单笔风险拒绝阈值。默认 2%。
    /// </summary>
    public double RejectRiskRate { get; init; } = 0.02;

    /// <summary>
    /// 保证金占账户权益进入谨慎区的比例。默认 30%。
    /// </summary>
    public double CautionMarginRate { get; init; } = 0.30;

    /// <summary>
    /// 保证金占账户权益拒绝阈值。默认 50%。
    /// </summary>
    public double RejectMarginRate { get; init; } = 0.50;

    /// <summary>
    /// 成本占止损风险进入谨慎区的比例。默认 20%。
    /// CostRatio = (手续费 + 滑点成本) / 止损风险。
    /// </summary>
    public double CautionCostRatio { get; init; } = 0.20;

    /// <summary>
    /// 成本占止损风险拒绝阈值。默认 30%。
    /// CostRatio = (手续费 + 滑点成本) / 止损风险。
    /// </summary>
    public double RejectCostRatio { get; init; } = 0.30;
}
