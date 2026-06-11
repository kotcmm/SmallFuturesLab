namespace SmallFuturesLab.Core.RiskConstraints;

public sealed class RiskConstraint
{
    private readonly RiskConstraintConfig config;

    public RiskConstraint(RiskConstraintConfig config)
    {
        this.config = config ?? throw new ArgumentNullException(nameof(config));
    }

    public TradePlan Evaluate(TradeSetup setup, DailyRiskState dailyRiskState)
    {
        ArgumentNullException.ThrowIfNull(setup);
        ArgumentNullException.ThrowIfNull(dailyRiskState);

        var invalidReason = ValidateTradeSetup(setup);
        if (invalidReason != RiskRejectReason.None)
        {
            return BuildInvalidTradePlan(setup, dailyRiskState, invalidReason);
        }

        var allowedLots = (int)Math.Floor(config.AccountR / setup.OneLotTradeR);

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

        if (plan.CostInR > config.PerTradeCostMaxR)
        {
            return plan with
            {
                Status = TradePlanStatus.Rejected,
                RejectReason = RiskRejectReason.CostTooHigh
            };
        }

        if (plan.MarginAfterOpen > config.MaxAllowedMargin)
        {
            return plan with
            {
                Status = TradePlanStatus.Rejected,
                RejectReason = RiskRejectReason.MarginUsageExceeded
            };
        }

        return plan;
    }

    private RiskRejectReason GetDailyRejectReason(DailyRiskState dailyRiskState)
    {
        if (dailyRiskState.RealizedLossToday >= config.DailyLossLimit)
        {
            return RiskRejectReason.DailyLossLimitReached;
        }

        if (dailyRiskState.RealizedPnlToday >= config.DailyProfitLockR)
        {
            return RiskRejectReason.DailyProfitLockReached;
        }

        if (dailyRiskState.DailyTradeCount >= config.MaxDailyTrades)
        {
            return RiskRejectReason.MaxDailyTradesReached;
        }

        if (dailyRiskState.ConsecutiveLosses >= config.MaxConsecutiveLosses)
        {
            return RiskRejectReason.ConsecutiveLossLimitReached;
        }

        return RiskRejectReason.None;
    }

    private TradePlan BuildTradePlan(
        TradeSetup setup,
        DailyRiskState dailyRiskState,
        TradePlanStatus status,
        RiskRejectReason rejectReason,
        int allowedLots)
    {
        var tradeR = setup.OneLotTradeR * allowedLots;
        var costInR = tradeR > 0
            ? setup.EstimatedRoundTripCostPerLot * allowedLots / tradeR
            : 0;

        var requiredRewardAmount = setup.OneLotTradeR * config.MinPlannedRewardR;
        var targetPriceDistance = requiredRewardAmount / setup.Multiplier;
        var targetPrice = setup.Direction == TradeDirection.Long
            ? setup.EntryPrice + targetPriceDistance
            : setup.EntryPrice - targetPriceDistance;

        var marginAfterOpen = dailyRiskState.CurrentMarginUsed + setup.OneLotMargin * allowedLots;

        return new TradePlan(
            Symbol: setup.Symbol,
            Status: status,
            RejectReason: rejectReason,
            Direction: setup.Direction,
            AccountR: config.AccountR,
            SetupPriceRisk: setup.SetupPriceRisk,
            OneLotPriceRisk: setup.OneLotPriceRisk,
            OneLotTradeR: setup.OneLotTradeR,
            AllowedLots: allowedLots,
            TradeR: tradeR,
            CostInR: costInR,
            RequiredRewardAmount: requiredRewardAmount,
            TargetPriceDistance: targetPriceDistance,
            TargetPrice: targetPrice,
            MaxAllowedMargin: config.MaxAllowedMargin,
            MarginAfterOpen: marginAfterOpen);
    }

    private TradePlan BuildInvalidTradePlan(
        TradeSetup setup,
        DailyRiskState dailyRiskState,
        RiskRejectReason rejectReason)
    {
        return new TradePlan(
            Symbol: setup.Symbol,
            Status: TradePlanStatus.Rejected,
            RejectReason: rejectReason,
            Direction: setup.Direction,
            AccountR: config.AccountR,
            SetupPriceRisk: setup.SetupPriceRisk,
            OneLotPriceRisk: 0,
            OneLotTradeR: 0,
            AllowedLots: 0,
            TradeR: 0,
            CostInR: 0,
            RequiredRewardAmount: 0,
            TargetPriceDistance: 0,
            TargetPrice: 0,
            MaxAllowedMargin: config.MaxAllowedMargin,
            MarginAfterOpen: dailyRiskState.CurrentMarginUsed);
    }

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
