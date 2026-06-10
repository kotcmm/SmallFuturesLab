namespace SmallFuturesLab.Core;

/// <summary>
/// 小资金品种过滤配置。
/// 不是完整风控系统，只是当前品种过滤器使用的账户权益和阈值配置。
/// </summary>
public sealed class RiskConfig
{
    /// <summary>
    /// 账户权益。必须大于 0。
    /// </summary>
    public double AccountEquity { get; set; }

    /// <summary>
    /// 单笔总风险占账户权益比例进入谨慎区的阈值。默认 1%。
    /// </summary>
    public double CautionRiskRate { get; set; } = 0.01;

    /// <summary>
    /// 单笔总风险占账户权益比例直接排除的阈值。默认 2%。
    /// </summary>
    public double RejectRiskRate { get; set; } = 0.02;

    /// <summary>
    /// 保证金占账户权益比例进入谨慎区的阈值。默认 30%。
    /// </summary>
    public double CautionMarginRate { get; set; } = 0.30;

    /// <summary>
    /// 保证金占账户权益比例直接排除的阈值。默认 50%。
    /// </summary>
    public double RejectMarginRate { get; set; } = 0.50;

    /// <summary>
    /// 成本占止损风险比例进入谨慎区的阈值。默认 20%。
    /// CostRatio = (手续费 + 滑点成本) / 止损风险。
    /// </summary>
    public double CautionCostRatio { get; set; } = 0.20;

    /// <summary>
    /// 成本占止损风险比例直接排除的阈值。默认 30%。
    /// CostRatio = (手续费 + 滑点成本) / 止损风险。
    /// </summary>
    public double RejectCostRatio { get; set; } = 0.30;
}
