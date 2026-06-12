namespace SmallFuturesLab.Core.Risk;

/// <summary>
/// 交易计划工厂。
///
/// 只负责把风险计算结果和拒绝原因组装成 TradePlan，
/// 避免 TradeRiskEvaluator 直接持有过长的 TradePlan 构造参数列表。
/// </summary>
internal sealed class TradePlanFactory
{
    /// <summary>
    /// 创建通过风险验算的交易计划。
    /// </summary>
    public TradePlan Accepted(
        TradeSetup setup,
        AccountRiskLimits limits,
        TradeRiskCalculation calculation)
    {
        return Build(
            setup,
            limits,
            status: TradePlanStatus.Accepted,
            rejectReason: RiskRejectReason.None,
            calculation: calculation,
            marginAfterOpenWhenMissing: 0);
    }

    /// <summary>
    /// 创建未通过风险验算的交易计划。
    /// </summary>
    public TradePlan Rejected(
        TradeSetup setup,
        AccountRiskLimits limits,
        DailyRiskState dailyRiskState,
        RiskRejectReason rejectReason,
        TradeRiskCalculation? calculation = null)
    {
        return Build(
            setup,
            limits,
            status: TradePlanStatus.Rejected,
            rejectReason: rejectReason,
            calculation: calculation,
            marginAfterOpenWhenMissing: dailyRiskState.CurrentMarginUsed);
    }

    /// <summary>
    /// 构造交易计划。
    /// </summary>
    private static TradePlan Build(
        TradeSetup setup,
        AccountRiskLimits limits,
        TradePlanStatus status,
        RiskRejectReason rejectReason,
        TradeRiskCalculation? calculation,
        double marginAfterOpenWhenMissing)
    {
        return new TradePlan(
            symbol: setup.Symbol,
            status: status,
            rejectReason: rejectReason,
            direction: setup.Direction,
            accountR: limits.AccountR,
            setupPriceRisk: calculation?.SetupPriceRisk ?? 0,
            oneLotPriceRisk: calculation?.OneLotPriceRisk ?? 0,
            oneLotTradeR: calculation?.OneLotTradeR ?? 0,
            allowedLots: calculation?.AllowedLots ?? 0,
            tradeR: calculation?.TradeR ?? 0,
            costInR: calculation?.CostInR ?? 0,
            requiredRewardAmount: calculation?.RequiredRewardAmount ?? 0,
            targetPriceDistance: calculation?.TargetPriceDistance ?? 0,
            targetPrice: calculation?.TargetPrice ?? 0,
            maxAllowedMargin: limits.MaxAllowedMargin,
            marginAfterOpen: calculation?.MarginAfterOpen ?? marginAfterOpenWhenMissing);
    }
}
