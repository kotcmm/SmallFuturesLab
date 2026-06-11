namespace SmallFuturesLab.Core.RiskConstraints;

/// <summary>
/// 风险约束验算器。
///
/// 职责：把 TradeSetup 转换成 TradePlan。
/// 不负责行情判断，不负责账户状态持久化，只做纯计算。
/// </summary>
public sealed class RiskConstraint
{
    private readonly AccountRiskLimits _limits;

    /// <summary>
    /// 创建风险约束验算器。
    /// </summary>
    /// <param name="limits">账户风险边界。</param>
    public RiskConstraint(AccountRiskLimits limits)
    {
        _limits = limits ?? throw new ArgumentNullException(nameof(limits));
    }

    /// <summary>
    /// 根据交易结构和当日风险状态生成交易计划。
    ///
    /// 计算顺序：
    /// 1. 检查 TradeSetup 是否有效；
    /// 2. 计算 AllowedLots；
    /// 3. 检查每日节奏约束；
    /// 4. 检查成本约束；
    /// 5. 检查保证金约束。
    /// </summary>
    /// <param name="setup">行情结构阶段生成的交易设想。</param>
    /// <param name="dailyRiskState">当日风险状态。</param>
    /// <returns>风险约束验算后的交易计划。</returns>
    public TradePlan Evaluate(TradeSetup setup, DailyRiskState dailyRiskState)
    {
        ArgumentNullException.ThrowIfNull(setup);
        ArgumentNullException.ThrowIfNull(dailyRiskState);

        // 输入无效时，后续价格、手数、成本推导都没有业务意义。
        var invalidReason = ValidateTradeSetup(setup);
        if (invalidReason != RiskRejectReason.None)
        {
            return BuildInvalidTradePlan(setup, dailyRiskState, invalidReason);
        }

        // AllowedLots = floor(AccountR / OneLotTradeR)。
        var allowedLots = (int)Math.Floor(_limits.AccountR / setup.OneLotTradeR);

        // 每日节奏约束优先检查，因为它决定当天是否还能继续评估新计划。
        var rejectReason = GetDailyRejectReason(dailyRiskState);
        if (rejectReason == RiskRejectReason.None && allowedLots < 1)
        {
            rejectReason = RiskRejectReason.NotEnoughAccountR;
        }

        var plan = BuildTradePlan(
            setup,
            dailyRiskState,
            rejectReason == RiskRejectReason.None ? TradePlanStatus.Accepted : TradePlanStatus.Rejected,
            rejectReason,
            allowedLots);

        if (plan.Status == TradePlanStatus.Rejected)
        {
            return plan;
        }

        // c = 本笔交易总成本 / TradeR。
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
        DailyRiskState dailyRiskState,
        TradePlanStatus status,
        RiskRejectReason rejectReason,
        int allowedLots)
    {
        // TradeR = OneLotTradeR × AllowedLots。
        var tradeR = setup.OneLotTradeR * allowedLots;

        // CostInR = 本笔交易总成本 / TradeR。
        var costInR = tradeR > 0
            ? setup.EstimatedRoundTripCostPerLot * allowedLots / tradeR
            : 0;

        // RequiredRewardAmount = OneLotTradeR × MinPlannedRewardR。
        var requiredRewardAmount = setup.OneLotTradeR * _limits.MinPlannedRewardR;

        // TargetPriceDistance = RequiredRewardAmount / Multiplier。
        var targetPriceDistance = requiredRewardAmount / setup.Multiplier;

        // 做多目标价在入场价上方；做空目标价在入场价下方。
        var targetPrice = setup.Direction == TradeDirection.Long
            ? setup.EntryPrice + targetPriceDistance
            : setup.EntryPrice - targetPriceDistance;

        var marginAfterOpen = dailyRiskState.CurrentMarginUsed + setup.OneLotMargin * allowedLots;

        return new TradePlan(
            symbol: setup.Symbol,
            status: status,
            rejectReason: rejectReason,
            direction: setup.Direction,
            accountR: _limits.AccountR,
            setupPriceRisk: setup.SetupPriceRisk,
            oneLotPriceRisk: setup.OneLotPriceRisk,
            oneLotTradeR: setup.OneLotTradeR,
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
            setupPriceRisk: setup.SetupPriceRisk,
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

        if (setup.Multiplier <= 0)
        {
            return RiskRejectReason.InvalidTradeSetup;
        }

        if (setup.EstimatedRoundTripCostPerLot < 0)
        {
            return RiskRejectReason.InvalidTradeSetup;
        }

        if (setup.OneLotMargin < 0)
        {
            return RiskRejectReason.InvalidTradeSetup;
        }

        return RiskRejectReason.None;
    }
}
