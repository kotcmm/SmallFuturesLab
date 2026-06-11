namespace SmallFuturesLab.Core.Risk;

/// <summary>
/// 风险约束验算后的交易计划。
///
/// TradePlan 是风险约束阶段的完整输出：
/// 它既包含最终是否允许交易，也包含每一步公式推导出的中间值，便于回测、日志和人工复核。
/// </summary>
public sealed record TradePlan
{
    /// <summary>
    /// 创建风险约束验算后的交易计划。
    /// </summary>
    /// <param name="symbol">品种代码。</param>
    /// <param name="status">风险约束验算状态。</param>
    /// <param name="rejectReason">拒绝原因；通过时为 None。</param>
    /// <param name="direction">交易方向。</param>
    /// <param name="accountR">账户允许的单笔风险上限。</param>
    /// <param name="setupPriceRisk">入场价到止损价之间的价格距离。</param>
    /// <param name="oneLotPriceRisk">一手价格风险，不含成本。</param>
    /// <param name="oneLotTradeR">一手计划风险，包含交易前预估成本。</param>
    /// <param name="allowedLots">风险约束允许的手数。</param>
    /// <param name="tradeR">本笔交易实际计划风险。</param>
    /// <param name="costInR">本笔交易前预估成本占 TradeR 的比例。</param>
    /// <param name="requiredRewardAmount">为了满足 MinPlannedRewardR 所需的最低盈利金额。</param>
    /// <param name="targetPriceDistance">目标价距离入场价的价格距离。</param>
    /// <param name="targetPrice">由风险约束阶段推导出的目标价。</param>
    /// <param name="maxAllowedMargin">账户允许的最大保证金占用金额。</param>
    /// <param name="marginAfterOpen">如果执行本计划，新开仓后的保证金占用金额。</param>
    public TradePlan(
        string symbol,
        TradePlanStatus status,
        RiskRejectReason rejectReason,
        TradeDirection direction,
        double accountR,
        double setupPriceRisk,
        double oneLotPriceRisk,
        double oneLotTradeR,
        int allowedLots,
        double tradeR,
        double costInR,
        double requiredRewardAmount,
        double targetPriceDistance,
        double targetPrice,
        double maxAllowedMargin,
        double marginAfterOpen)
    {
        Symbol = symbol;
        Status = status;
        RejectReason = rejectReason;
        Direction = direction;
        AccountR = accountR;
        SetupPriceRisk = setupPriceRisk;
        OneLotPriceRisk = oneLotPriceRisk;
        OneLotTradeR = oneLotTradeR;
        AllowedLots = allowedLots;
        TradeR = tradeR;
        CostInR = costInR;
        RequiredRewardAmount = requiredRewardAmount;
        TargetPriceDistance = targetPriceDistance;
        TargetPrice = targetPrice;
        MaxAllowedMargin = maxAllowedMargin;
        MarginAfterOpen = marginAfterOpen;
    }

    /// <summary>
    /// 品种代码。
    /// </summary>
    public string Symbol { get; init; }

    /// <summary>
    /// 风险约束验算状态。
    /// </summary>
    public TradePlanStatus Status { get; init; }

    /// <summary>
    /// 拒绝原因；通过时为 None。
    /// </summary>
    public RiskRejectReason RejectReason { get; init; }

    /// <summary>
    /// 交易方向。
    /// </summary>
    public TradeDirection Direction { get; init; }

    /// <summary>
    /// 账户允许的单笔风险上限。
    /// </summary>
    public double AccountR { get; init; }

    /// <summary>
    /// 入场价到止损价之间的价格距离。
    /// </summary>
    public double SetupPriceRisk { get; init; }

    /// <summary>
    /// 一手价格风险，不含成本。
    /// </summary>
    public double OneLotPriceRisk { get; init; }

    /// <summary>
    /// 一手计划风险，包含交易前预估成本。
    /// </summary>
    public double OneLotTradeR { get; init; }

    /// <summary>
    /// 风险约束允许的手数。
    /// </summary>
    public int AllowedLots { get; init; }

    /// <summary>
    /// 本笔交易实际计划风险。
    /// </summary>
    public double TradeR { get; init; }

    /// <summary>
    /// 本笔交易前预估成本占 TradeR 的比例。
    /// </summary>
    public double CostInR { get; init; }

    /// <summary>
    /// 为了满足 MinPlannedRewardR 所需的最低盈利金额。
    /// </summary>
    public double RequiredRewardAmount { get; init; }

    /// <summary>
    /// 目标价距离入场价的价格距离。
    /// </summary>
    public double TargetPriceDistance { get; init; }

    /// <summary>
    /// 由风险约束阶段推导出的目标价。
    /// </summary>
    public double TargetPrice { get; init; }

    /// <summary>
    /// 账户允许的最大保证金占用金额。
    /// </summary>
    public double MaxAllowedMargin { get; init; }

    /// <summary>
    /// 如果执行本计划，新开仓后的保证金占用金额。
    /// </summary>
    public double MarginAfterOpen { get; init; }
}
