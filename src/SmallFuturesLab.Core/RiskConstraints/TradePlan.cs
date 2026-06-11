namespace SmallFuturesLab.Core.RiskConstraints;

/// <summary>
/// 风险约束验算后的交易计划。
///
/// TradePlan 是风险约束阶段的完整输出：
/// 它既包含最终是否允许交易，也包含每一步公式推导出的中间值，便于回测、日志和人工复核。
/// </summary>
/// <param name="Symbol">品种代码。</param>
/// <param name="Status">风险约束验算状态。</param>
/// <param name="RejectReason">拒绝原因；通过时为 None。</param>
/// <param name="Direction">交易方向。</param>
/// <param name="AccountR">账户允许的单笔风险上限。</param>
/// <param name="SetupPriceRisk">入场价到止损价之间的价格距离。</param>
/// <param name="OneLotPriceRisk">一手价格风险，不含成本。</param>
/// <param name="OneLotTradeR">一手计划风险，包含预估交易成本。</param>
/// <param name="AllowedLots">风险约束允许的手数。</param>
/// <param name="TradeR">本笔交易实际计划风险。</param>
/// <param name="CostInR">本笔交易成本占 TradeR 的比例。</param>
/// <param name="RequiredRewardAmount">为了满足 MinPlannedRewardR 所需的最低盈利金额。</param>
/// <param name="TargetPriceDistance">目标价距离入场价的价格距离。</param>
/// <param name="TargetPrice">由风险约束阶段推导出的目标价。</param>
/// <param name="MaxAllowedMargin">账户允许的最大保证金占用金额。</param>
/// <param name="MarginAfterOpen">如果执行本计划，新开仓后的保证金占用金额。</param>
public sealed record TradePlan(
    string Symbol,
    TradePlanStatus Status,
    RiskRejectReason RejectReason,
    TradeDirection Direction,
    double AccountR,
    double SetupPriceRisk,
    double OneLotPriceRisk,
    double OneLotTradeR,
    int AllowedLots,
    double TradeR,
    double CostInR,
    double RequiredRewardAmount,
    double TargetPriceDistance,
    double TargetPrice,
    double MaxAllowedMargin,
    double MarginAfterOpen);
