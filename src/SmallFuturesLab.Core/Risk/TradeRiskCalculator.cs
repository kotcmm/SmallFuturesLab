namespace SmallFuturesLab.Core.Risk;

/// <summary>
/// 单笔交易风险计算器。
///
/// 只负责根据输入计算风险中间结果，不判断这些结果是否满足账户约束。
/// </summary>
internal sealed class TradeRiskCalculator
{
    /// <summary>
    /// 计算单笔交易风险结果。
    /// </summary>
    /// <param name="limits">账户风险边界。</param>
    /// <param name="setup">行情结构阶段生成的交易设想。</param>
    /// <param name="contract">合约风险计算资料。</param>
    /// <param name="dailyRiskState">当日风险状态。</param>
    /// <returns>风险计算中间结果。</returns>
    public TradeRiskCalculation Calculate(
        AccountRiskLimits limits,
        TradeSetup setup,
        ContractRiskProfile contract,
        DailyRiskState dailyRiskState)
    {
        // SetupPriceRisk = |EntryPrice - StopPrice|。
        var setupPriceRisk = Math.Abs(setup.EntryPrice - setup.StopPrice);

        // OneLotPriceRisk = SetupPriceRisk × Multiplier。
        var oneLotPriceRisk = setupPriceRisk * contract.Multiplier;

        // OneLotTradeR = OneLotPriceRisk + EstimatedRoundTripCostPerLot。
        var oneLotTradeR = oneLotPriceRisk + contract.EstimatedRoundTripCostPerLot;

        // AllowedLots = floor(AccountR / OneLotTradeR)。
        var allowedLots = (int)Math.Floor(limits.AccountR / oneLotTradeR);

        // TradeR = OneLotTradeR × AllowedLots。
        var tradeR = oneLotTradeR * allowedLots;

        // CostInR = 本笔交易前预估总成本 / TradeR。
        var costInR = tradeR > 0
            ? contract.EstimatedRoundTripCostPerLot * allowedLots / tradeR
            : 0;

        // RequiredRewardAmount = OneLotTradeR × MinPlannedRewardR。
        var requiredRewardAmount = oneLotTradeR * limits.MinPlannedRewardR;

        // TargetPriceDistance = RequiredRewardAmount / Multiplier。
        var targetPriceDistance = requiredRewardAmount / contract.Multiplier;

        // 做多目标价在入场价上方；做空目标价在入场价下方。
        var targetPrice = setup.Direction == TradeDirection.Long
            ? setup.EntryPrice + targetPriceDistance
            : setup.EntryPrice - targetPriceDistance;

        // MarginAfterOpen = CurrentMarginUsed + OneLotMargin × AllowedLots。
        var marginAfterOpen = dailyRiskState.CurrentMarginUsed + contract.OneLotMargin * allowedLots;

        return new TradeRiskCalculation(
            setupPriceRisk: setupPriceRisk,
            oneLotPriceRisk: oneLotPriceRisk,
            oneLotTradeR: oneLotTradeR,
            allowedLots: allowedLots,
            tradeR: tradeR,
            costInR: costInR,
            requiredRewardAmount: requiredRewardAmount,
            targetPriceDistance: targetPriceDistance,
            targetPrice: targetPrice,
            marginAfterOpen: marginAfterOpen);
    }
}
