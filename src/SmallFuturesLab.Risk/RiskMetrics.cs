namespace SmallFuturesLab.Risk;

/// <summary>
/// 风险指标，记录交易许可评估过程中计算的核心中间值。
/// </summary>
public record RiskMetrics
{
    /// <summary>
    /// 一跳金额，TickValue = T × M。
    /// </summary>
    public double TickValue { get; init; }

    /// <summary>
    /// 一手名义金额，NotionalPerLot = P × M。
    /// </summary>
    public double NotionalPerLot { get; init; }

    /// <summary>
    /// 一手保证金，MarginPerLot = P × M × μ。
    /// </summary>
    public double MarginPerLot { get; init; }

    /// <summary>
    /// 止损距离跳数，StopTicks = ceil(|Pe - Ps| / T)。
    /// </summary>
    public int StopTicks { get; init; }

    /// <summary>
    /// 标准化后的止损距离，AdjustedStopDistance = StopTicks × T。
    /// </summary>
    public double AdjustedStopDistance { get; init; }

    /// <summary>
    /// 一手止损风险金额，StopRiskPerLot = AdjustedStopDistance × M。
    /// </summary>
    public double StopRiskPerLot { get; init; }

    /// <summary>
    /// 总止损风险金额，StopRisk = StopRiskPerLot × L。
    /// </summary>
    public double StopRisk { get; init; }

    /// <summary>
    /// 滑点金额，SlippageMoney = S × T × M × L。
    /// </summary>
    public double SlippageMoney { get; init; }

    /// <summary>
    /// 总成本金额，CostMoney = F × L + SlippageMoney。
    /// </summary>
    public double CostMoney { get; init; }

    /// <summary>
    /// 实际 1R，TotalRiskMoney = StopRisk + CostMoney。
    /// </summary>
    public double TotalRiskMoney { get; init; }

    /// <summary>
    /// 单笔风险占比，RiskRate = TotalRiskMoney / E。
    /// </summary>
    public double RiskRate { get; init; }

    /// <summary>
    /// 成本占比，CostRatio = CostMoney / StopRisk。
    /// </summary>
    public double CostRatio { get; init; }

    /// <summary>
    /// 保证金占用金额，MarginMoney = MarginPerLot × L。
    /// </summary>
    public double MarginMoney { get; init; }

    /// <summary>
    /// 保证金占用占比，MarginRateOfEquity = MarginMoney / E。
    /// </summary>
    public double MarginRateOfEquity { get; init; }

    /// <summary>
    /// 连续 5 次亏损金额。
    /// </summary>
    public double LossAfter5 { get; init; }

    /// <summary>
    /// 连续 8 次亏损金额。
    /// </summary>
    public double LossAfter8 { get; init; }

    /// <summary>
    /// 连续 10 次亏损金额。
    /// </summary>
    public double LossAfter10 { get; init; }

    /// <summary>
    /// 连续 5 次亏损占账户比例。
    /// </summary>
    public double LossAfter5Rate { get; init; }

    /// <summary>
    /// 连续 8 次亏损占账户比例。
    /// </summary>
    public double LossAfter8Rate { get; init; }

    /// <summary>
    /// 连续 10 次亏损占账户比例。
    /// </summary>
    public double LossAfter10Rate { get; init; }

    /// <summary>
    /// 本笔亏损后今日累计亏损，ProjectedDailyLoss = D + TotalRiskMoney。
    /// </summary>
    public double ProjectedDailyLoss { get; init; }

    /// <summary>
    /// 每日亏损上限金额，DailyLossLimitCash = E × d_max。
    /// </summary>
    public double DailyLossLimitCash { get; init; }

    /// <summary>
    /// 推荐单笔风险金额。
    /// </summary>
    public double RecommendedRiskCash { get; init; }

    /// <summary>
    /// 常规单笔风险上限金额。
    /// </summary>
    public double NormalRiskCash { get; init; }

    /// <summary>
    /// 极限单笔风险上限金额。
    /// </summary>
    public double ExtremeRiskCash { get; init; }
}
