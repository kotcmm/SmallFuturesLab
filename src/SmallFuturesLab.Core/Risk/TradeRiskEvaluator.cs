namespace SmallFuturesLab.Core.Risk;

/// <summary>
/// 单笔交易风险验算器。
///
/// 职责：把 TradeSetup 和 ContractRiskProfile 转换成 TradePlan。
/// 不负责行情判断，不负责账户状态持久化，只做纯计算。
/// </summary>
public sealed class TradeRiskEvaluator
{
    private readonly AccountRiskLimits _limits;

    /// <summary>
    /// 创建单笔交易风险验算器。
    /// </summary>
    /// <param name="limits">账户风险边界。</param>
    public TradeRiskEvaluator(AccountRiskLimits limits)
    {
        _limits = limits ?? throw new ArgumentNullException(nameof(limits));
    }

    /// <summary>
    /// 根据交易结构、合约风险计算资料和当日风险状态生成交易计划。
    ///
    /// 计算顺序：
    /// 1. 检查 TradeSetup 是否有效；
    /// 2. 检查 ContractRiskProfile 是否有效；
    /// 3. 在风险模块内部计算结构价格风险、一手价格风险和一手计划风险；
    /// 4. 计算 AllowedLots；
    /// 5. 检查每日节奏约束；
    /// 6. 检查成本约束；
    /// 7. 检查保证金约束。
    /// </summary>
    /// <param name="setup">行情结构阶段生成的交易设想。</param>
    /// <param name="contract">合约风险计算资料。</param>
    /// <param name="dailyRiskState">当日风险状态。</param>
    /// <returns>风险验算后的交易计划。</returns>
    public TradePlan Evaluate(
        TradeSetup setup,
        ContractRiskProfile contract,
        DailyRiskState dailyRiskState)
    {
        ArgumentNullException.ThrowIfNull(setup);
        ArgumentNullException.ThrowIfNull(contract);
        ArgumentNullException.ThrowIfNull(dailyRiskState);

        // 输入无效时，后续价格、手数、成本推导都没有业务意义。
        var invalidSetupReason = ValidateTradeSetup(setup);
        if (invalidSetupReason != RiskRejectReason.None)
        {
            return BuildInvalidTradePlan(setup, dailyRiskState, invalidSetupReason);
        }

        var invalidContractReason = ValidateContractRiskProfile(setup, contract);
        if (invalidContractReason != RiskRejectReason.None)
        {
            return BuildInvalidTradePlan(setup, dailyRiskState, invalidContractReason);
        }

        // SetupPriceRisk = |EntryPrice - StopPrice|。
        var setupPriceRisk = Math.Abs(setup.EntryPrice - setup.StopPrice);

        // OneLotPriceRisk = SetupPriceRisk × Multiplier。
        var oneLotPriceRisk = setupPriceRisk * contract.Multiplier;

        // OneLotTradeR = OneLotPriceRisk + EstimatedRoundTripCostPerLot。
        var oneLotTradeR = oneLotPriceRisk + contract.EstimatedRoundTripCostPerLot;

        // AllowedLots = floor(AccountR / OneLotTradeR)。
        var allowedLots = (int)Math.Floor(_limits.AccountR / oneLotTradeR);

        // 每日节奏约束优先检查，因为它决定当天是否还能继续评估新计划。
        var rejectReason = GetDailyRejectReason(dailyRiskState);
        if (rejectReason == RiskRejectReason.None && allowedLots < 1)
        {
            rejectReason = RiskRejectReason.NotEnoughAccountR;
        }

        var plan = BuildTradePlan(
            setup,
            contract,
            dailyRiskState,
            rejectReason == RiskRejectReason.None ? TradePlanStatus.Accepted : TradePlanStatus.Rejected,
            rejectReason,
            setupPriceRisk,
            oneLotPriceRisk,
            oneLotTradeR,
            allowedLots);

        if (plan.Status == TradePlanStatus.Rejected)
        {
            return plan;
        }

        // c = 本笔交易前预估总成本 / TradeR。
        if (plan.CostInR > _limits.PerTradeCostMaxR)
        {
            return plan with
            {
                Status = TradePlanStatus.Rejected,
                RejectReason = RiskRejectReason.CostTooHigh
            };
        }

        // MarginAfterOpen = CurrentMarginUsed + OneLotMargin × AllowedLots。
        if (plan.MarginAfterOpen > _limits.MaxAllowedMargin)
        {
            return plan with
            {
                Status = TradePlanStatus.Rejected,
                RejectReason = RiskRejectReason.MarginUsageExceeded
            };
        }

        return plan;
    }

    /// <summary>
    /// 检查当天是否触发停止继续评估新计划的条件。
    /// </summary>
    private RiskRejectReason GetDailyRejectReason(DailyRiskState dailyRiskState)
    {
        if (dailyRiskState.RealizedLossToday >= _limits.DailyLossLimit)
        {
            return RiskRejectReason.DailyLossLimitReached;
        }

        if (dailyRiskState.RealizedPnlToday >= _limits.DailyProfitLockR)
        {
            return RiskRejectReason.DailyProfitLockReached;
        }

        if (dailyRiskState.DailyTradeCount >= _limits.MaxDailyTrades)
        {
            return RiskRejectReason.MaxDailyTradesReached;
        }

        if (dailyRiskState.ConsecutiveLosses >= _limits.MaxConsecutiveLosses)
        {
            return RiskRejectReason.ConsecutiveLossLimitReached;
        }

        return RiskRejectReason.None;
    }

    /// <summary>
    /// 构造正常可计算的交易计划。
    /// </summary>
    private TradePlan BuildTradePlan(
        TradeSetup setup,
        ContractRiskProfile contract,
        DailyRiskState dailyRiskState,
        TradePlanStatus status,
        RiskRejectReason rejectReason,
        double setupPriceRisk,
        double oneLotPriceRisk,
        double oneLotTradeR,
        int allowedLots)
    {
        // TradeR = OneLotTradeR × AllowedLots。
        var tradeR = oneLotTradeR * allowedLots;

        // CostInR = 本笔交易前预估总成本 / TradeR。
        var costInR = tradeR > 0
            ? contract.EstimatedRoundTripCostPerLot * allowedLots / tradeR
            : 0;

        // RequiredRewardAmount = OneLotTradeR × MinPlannedRewardR。
        var requiredRewardAmount = oneLotTradeR * _limits.MinPlannedRewardR;

        // TargetPriceDistance = RequiredRewardAmount / Multiplier。
        var targetPriceDistance = requiredRewardAmount / contract.Multiplier;

        // 做多目标价在入场价上方；做空目标价在入场价下方。
        var targetPrice = setup.Direction == TradeDirection.Long
            ? setup.EntryPrice + targetPriceDistance
            : setup.EntryPrice - targetPriceDistance;

        var marginAfterOpen = dailyRiskState.CurrentMarginUsed + contract.OneLotMargin * allowedLots;

        return new TradePlan(
            symbol: setup.Symbol,
            status: status,
            rejectReason: rejectReason,
            direction: setup.Direction,
            accountR: _limits.AccountR,
            setupPriceRisk: setupPriceRisk,
            oneLotPriceRisk: oneLotPriceRisk,
            oneLotTradeR: oneLotTradeR,
            allowedLots: allowedLots,
            tradeR: tradeR,
            costInR: costInR,
            requiredRewardAmount: requiredRewardAmount,
            targetPriceDistance: targetPriceDistance,
            targetPrice: targetPrice,
            maxAllowedMargin: _limits.MaxAllowedMargin,
            marginAfterOpen: marginAfterOpen);
    }

    /// <summary>
    /// 构造无效输入对应的拒绝计划。
    /// </summary>
    private TradePlan BuildInvalidTradePlan(
        TradeSetup setup,
        DailyRiskState dailyRiskState,
        RiskRejectReason rejectReason)
    {
        return new TradePlan(
            symbol: setup.Symbol,
            status: TradePlanStatus.Rejected,
            rejectReason: rejectReason,
            direction: setup.Direction,
            accountR: _limits.AccountR,
            setupPriceRisk: 0,
            oneLotPriceRisk: 0,
            oneLotTradeR: 0,
            allowedLots: 0,
            tradeR: 0,
            costInR: 0,
            requiredRewardAmount: 0,
            targetPriceDistance: 0,
            targetPrice: 0,
            maxAllowedMargin: _limits.MaxAllowedMargin,
            marginAfterOpen: dailyRiskState.CurrentMarginUsed);
    }

    /// <summary>
    /// 检查 TradeSetup 是否具备最小可计算性。
    /// </summary>
    private static RiskRejectReason ValidateTradeSetup(TradeSetup setup)
    {
        if (string.IsNullOrWhiteSpace(setup.Symbol))
        {
            return RiskRejectReason.InvalidTradeSetup;
        }

        if (setup.Direction is not TradeDirection.Long and not TradeDirection.Short)
        {
            return RiskRejectReason.InvalidTradeSetup;
        }

        if (setup.EntryPrice <= 0 || setup.StopPrice <= 0)
        {
            return RiskRejectReason.InvalidTradeSetup;
        }

        if (setup.EntryPrice == setup.StopPrice)
        {
            return RiskRejectReason.InvalidTradeSetup;
        }

        return RiskRejectReason.None;
    }

    /// <summary>
    /// 检查 ContractRiskProfile 是否具备最小可计算性。
    /// </summary>
    private static RiskRejectReason ValidateContractRiskProfile(
        TradeSetup setup,
        ContractRiskProfile contract)
    {
        if (string.IsNullOrWhiteSpace(contract.Symbol))
        {
            return RiskRejectReason.InvalidContractRiskProfile;
        }

        if (!string.Equals(setup.Symbol, contract.Symbol, StringComparison.Ordinal))
        {
            return RiskRejectReason.InvalidContractRiskProfile;
        }

        if (contract.Multiplier <= 0)
        {
            return RiskRejectReason.InvalidContractRiskProfile;
        }

        if (contract.EstimatedRoundTripCostPerLot < 0)
        {
            return RiskRejectReason.InvalidContractRiskProfile;
        }

        if (contract.OneLotMargin < 0)
        {
            return RiskRejectReason.InvalidContractRiskProfile;
        }

        return RiskRejectReason.None;
    }
}
