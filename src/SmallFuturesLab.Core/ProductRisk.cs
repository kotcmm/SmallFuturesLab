namespace SmallFuturesLab.Core;

/// <summary>
/// 某个品种在指定账户配置和测算条件下的风险压力计算快照。
/// </summary>
public sealed record ProductRisk
{
    /// <summary>待过滤品种。</summary>
    public required Product Product { get; init; }

    /// <summary>账户风险配置。</summary>
    public required RiskConfig RiskConfig { get; init; }

    /// <summary>测算条件。</summary>
    public required FilterCondition Condition { get; init; }

    /// <summary>
    /// 保证金总金额。
    /// MarginMoney = Product.MarginPerLot * Condition.Lots。
    /// </summary>
    public double MarginMoney => Product.MarginPerLot * Condition.Lots;

    /// <summary>
    /// 保证金占账户权益比例。
    /// MarginRate = MarginMoney / RiskConfig.AccountEquity。
    /// </summary>
    public double MarginRate => MarginMoney / RiskConfig.AccountEquity;

    /// <summary>
    /// 止损风险金额。
    /// StopRiskMoney = Condition.StopTicks * Product.TickValue * Condition.Lots。
    /// </summary>
    public double StopRiskMoney => Condition.StopTicks * Product.TickValue * Condition.Lots;

    /// <summary>
    /// 滑点金额。
    /// SlippageMoney = Condition.SlippageTicks * Product.TickValue * Condition.Lots。
    /// </summary>
    public double SlippageMoney => Condition.SlippageTicks * Product.TickValue * Condition.Lots;

    /// <summary>
    /// 成本金额，包含手续费和滑点。
    /// CostMoney = Product.RoundTripFee * Condition.Lots + SlippageMoney。
    /// </summary>
    public double CostMoney => Product.RoundTripFee * Condition.Lots + SlippageMoney;

    /// <summary>
    /// 总风险金额，包含止损、手续费和滑点。
    /// TotalRiskMoney = StopRiskMoney + CostMoney。
    /// </summary>
    public double TotalRiskMoney => StopRiskMoney + CostMoney;

    /// <summary>
    /// 总风险占账户权益比例。
    /// RiskRate = TotalRiskMoney / RiskConfig.AccountEquity。
    /// </summary>
    public double RiskRate => TotalRiskMoney / RiskConfig.AccountEquity;

    /// <summary>
    /// 成本占止损风险比例。
    /// CostRatio = CostMoney / StopRiskMoney。
    /// 当 StopRiskMoney 为 0 时返回正无穷。
    /// </summary>
    public double CostRatio => StopRiskMoney > 0 ? CostMoney / StopRiskMoney : double.PositiveInfinity;

    /// <summary>
    /// 根据阈值判断的过滤状态。
    /// </summary>
    public ProductFilterStatus Status
    {
        get
        {
            if (RiskRate > RiskConfig.RejectRiskRate
                || MarginRate > RiskConfig.RejectMarginRate
                || CostRatio > RiskConfig.RejectCostRatio)
            {
                return ProductFilterStatus.Rejected;
            }

            if (RiskRate > RiskConfig.CautionRiskRate
                || MarginRate > RiskConfig.CautionMarginRate
                || CostRatio > RiskConfig.CautionCostRatio)
            {
                return ProductFilterStatus.Caution;
            }

            return ProductFilterStatus.Allowed;
        }
    }
}
