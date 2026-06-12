namespace SmallFuturesLab.Core.Risk;

/// <summary>
/// 单笔交易风险结果验证器。
///
/// 只负责判断计算结果是否满足账户风险边界和当日风险状态，
/// 不负责输入合法性检查，也不负责构造 TradePlan。
/// </summary>
internal sealed class TradeRiskResultValidator
{
    /// <summary>
    /// 验证风险计算结果是否允许形成可执行交易计划。
    /// </summary>
    /// <param name="limits">账户风险边界。</param>
    /// <param name="dailyRiskState">当日风险状态。</param>
    /// <param name="calculation">风险计算中间结果。</param>
    /// <returns>拒绝原因；通过时返回 None。</returns>
    public RiskRejectReason Validate(
        AccountRiskLimits limits,
        DailyRiskState dailyRiskState,
        TradeRiskCalculation calculation)
    {
        var dailyRejectReason = ValidateDailyRisk(limits, dailyRiskState);
        if (dailyRejectReason != RiskRejectReason.None)
        {
            return dailyRejectReason;
        }

        if (calculation.AllowedLots < 1)
        {
            return RiskRejectReason.NotEnoughAccountR;
        }

        if (calculation.CostInR > limits.PerTradeCostMaxR)
        {
            return RiskRejectReason.CostTooHigh;
        }

        if (calculation.MarginAfterOpen > limits.MaxAllowedMargin)
        {
            return RiskRejectReason.MarginUsageExceeded;
        }

        return RiskRejectReason.None;
    }

    /// <summary>
    /// 检查当天是否触发停止继续评估新计划的条件。
    /// </summary>
    private static RiskRejectReason ValidateDailyRisk(
        AccountRiskLimits limits,
        DailyRiskState dailyRiskState)
    {
        if (dailyRiskState.RealizedLossToday >= limits.DailyLossLimit)
        {
            return RiskRejectReason.DailyLossLimitReached;
        }

        if (dailyRiskState.RealizedPnlToday >= limits.DailyProfitLockR)
        {
            return RiskRejectReason.DailyProfitLockReached;
        }

        if (dailyRiskState.DailyTradeCount >= limits.MaxDailyTrades)
        {
            return RiskRejectReason.MaxDailyTradesReached;
        }

        if (dailyRiskState.ConsecutiveLosses >= limits.MaxConsecutiveLosses)
        {
            return RiskRejectReason.ConsecutiveLossLimitReached;
        }

        return RiskRejectReason.None;
    }
}
