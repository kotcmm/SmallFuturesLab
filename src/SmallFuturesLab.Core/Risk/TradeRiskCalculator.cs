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
        var setupPriceRisk = CalculateSetupPriceRisk(setup);
        var oneLotPriceRisk = CalculateOneLotPriceRisk(setupPriceRisk, contract);
        var oneLotTradeR = CalculateOneLotTradeR(oneLotPriceRisk, contract);
        var allowedLots = CalculateAllowedLots(limits, oneLotTradeR);
        var tradeR = CalculateTradeR(oneLotTradeR, allowedLots);
        var costInR = CalculateCostInR(contract, allowedLots, tradeR);
        var requiredRewardAmount = CalculateRequiredRewardAmount(limits, oneLotTradeR);
        var targetPriceDistance = CalculateTargetPriceDistance(requiredRewardAmount, contract);
        var targetPrice = CalculateTargetPrice(setup, targetPriceDistance);
        var marginAfterOpen = CalculateMarginAfterOpen(contract, dailyRiskState, allowedLots);

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

    private static double CalculateSetupPriceRisk(TradeSetup setup)
    {
        return Math.Abs(setup.EntryPrice - setup.StopPrice);
    }

    private static double CalculateOneLotPriceRisk(
        double setupPriceRisk,
        ContractRiskProfile contract)
    {
        return setupPriceRisk * contract.Multiplier;
    }

    private static double CalculateOneLotTradeR(
        double oneLotPriceRisk,
        ContractRiskProfile contract)
    {
        return oneLotPriceRisk + contract.EstimatedRoundTripCostPerLot;
    }

    private static int CalculateAllowedLots(
        AccountRiskLimits limits,
        double oneLotTradeR)
    {
        return (int)Math.Floor(limits.AccountR / oneLotTradeR);
    }

    private static double CalculateTradeR(
        double oneLotTradeR,
        int allowedLots)
    {
        return oneLotTradeR * allowedLots;
    }

    private static double CalculateCostInR(
        ContractRiskProfile contract,
        int allowedLots,
        double tradeR)
    {
        if (tradeR <= 0)
        {
            return 0;
        }

        return contract.EstimatedRoundTripCostPerLot * allowedLots / tradeR;
    }

    private static double CalculateRequiredRewardAmount(
        AccountRiskLimits limits,
        double oneLotTradeR)
    {
        return oneLotTradeR * limits.MinPlannedRewardR;
    }

    private static double CalculateTargetPriceDistance(
        double requiredRewardAmount,
        ContractRiskProfile contract)
    {
        return requiredRewardAmount / contract.Multiplier;
    }

    private static double CalculateTargetPrice(
        TradeSetup setup,
        double targetPriceDistance)
    {
        return setup.Direction == TradeDirection.Long
            ? setup.EntryPrice + targetPriceDistance
            : setup.EntryPrice - targetPriceDistance;
    }

    private static double CalculateMarginAfterOpen(
        ContractRiskProfile contract,
        DailyRiskState dailyRiskState,
        int allowedLots)
    {
        return dailyRiskState.CurrentMarginUsed + contract.OneLotMargin * allowedLots;
    }
}
