namespace SmallFuturesLab.Core.Risk;

/// <summary>
/// 单笔交易风险计算结果。
///
/// 这个对象只保存风险模块从 TradeSetup、ContractRiskProfile、AccountRiskLimits 和 DailyRiskState
/// 推导出来的中间结果，避免在方法之间传递过长的参数列表。
/// </summary>
internal sealed record TradeRiskCalculation
{
    /// <summary>
    /// 创建单笔交易风险计算结果。
    /// </summary>
    public TradeRiskCalculation(
        double setupPriceRisk,
        double oneLotPriceRisk,
        double oneLotTradeR,
        int allowedLots,
        double tradeR,
        double costInR,
        double requiredRewardAmount,
        double targetPriceDistance,
        double targetPrice,
        double marginAfterOpen)
    {
        SetupPriceRisk = setupPriceRisk;
        OneLotPriceRisk = oneLotPriceRisk;
        OneLotTradeR = oneLotTradeR;
        AllowedLots = allowedLots;
        TradeR = tradeR;
        CostInR = costInR;
        RequiredRewardAmount = requiredRewardAmount;
        TargetPriceDistance = targetPriceDistance;
        TargetPrice = targetPrice;
        MarginAfterOpen = marginAfterOpen;
    }

    /// <summary>
    /// 入场价到止损价之间的价格距离。
    /// </summary>
    public double SetupPriceRisk { get; }

    /// <summary>
    /// 一手价格风险，不含成本。
    /// </summary>
    public double OneLotPriceRisk { get; }

    /// <summary>
    /// 一手计划风险，包含交易前预估成本。
    /// </summary>
    public double OneLotTradeR { get; }

    /// <summary>
    /// 风险约束允许的手数。
    /// </summary>
    public int AllowedLots { get; }

    /// <summary>
    /// 本笔交易实际计划风险。
    /// </summary>
    public double TradeR { get; }

    /// <summary>
    /// 本笔交易前预估成本占 TradeR 的比例。
    /// </summary>
    public double CostInR { get; }

    /// <summary>
    /// 为了满足 MinPlannedRewardR 所需的最低盈利金额。
    /// </summary>
    public double RequiredRewardAmount { get; }

    /// <summary>
    /// 目标价距离入场价的价格距离。
    /// </summary>
    public double TargetPriceDistance { get; }

    /// <summary>
    /// 由风险约束阶段推导出的目标价。
    /// </summary>
    public double TargetPrice { get; }

    /// <summary>
    /// 如果执行本计划，新开仓后的保证金占用金额。
    /// </summary>
    public double MarginAfterOpen { get; }
}
