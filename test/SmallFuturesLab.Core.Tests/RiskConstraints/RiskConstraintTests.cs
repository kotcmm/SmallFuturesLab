using SmallFuturesLab.Core.RiskConstraints;

namespace SmallFuturesLab.Core.Tests.RiskConstraints;

public sealed class RiskConstraintTests
{
    [Fact]
    public void BuildsAcceptedLongTradePlanFromDocumentExample()
    {
        var constraint = new RiskConstraint(DefaultConfig());

        var plan = constraint.Evaluate(
            new TradeSetup(
                Symbol: "MA",
                Direction: TradeDirection.Long,
                EntryPrice: 3000,
                StopPrice: 2980,
                Multiplier: 10,
                EstimatedRoundTripCostPerLot: 20,
                OneLotMargin: 3000),
            new DailyRiskState(
                RealizedPnlToday: 260,
                DailyTradeCount: 1,
                ConsecutiveLosses: 0,
                CurrentMarginUsed: 0));

        Assert.Equal(TradePlanStatus.Accepted, plan.Status);
        Assert.Equal(RiskRejectReason.None, plan.RejectReason);
        Assert.Equal(250, plan.AccountR);
        Assert.Equal(20, plan.SetupPriceRisk);
        Assert.Equal(200, plan.OneLotPriceRisk);
        Assert.Equal(220, plan.OneLotTradeR);
        Assert.Equal(1, plan.AllowedLots);
        Assert.Equal(220, plan.TradeR);
        Assert.Equal(0.090909, plan.CostInR, 6);
        Assert.Equal(550, plan.RequiredRewardAmount);
        Assert.Equal(55, plan.TargetPriceDistance);
        Assert.Equal(3055, plan.TargetPrice);
        Assert.Equal(15000, plan.MaxAllowedMargin);
        Assert.Equal(3000, plan.MarginAfterOpen);
    }

    [Fact]
    public void BuildsShortTargetPriceBelowEntry()
    {
        var constraint = new RiskConstraint(DefaultConfig());

        var plan = constraint.Evaluate(
            new TradeSetup(
                Symbol: "MA",
                Direction: TradeDirection.Short,
                EntryPrice: 2980,
                StopPrice: 3000,
                Multiplier: 10,
                EstimatedRoundTripCostPerLot: 20,
                OneLotMargin: 3000),
            DefaultDailyState());

        Assert.Equal(TradePlanStatus.Accepted, plan.Status);
        Assert.Equal(2925, plan.TargetPrice);
    }

    [Fact]
    public void RejectsWhenDailyLossLimitReached()
    {
        var constraint = new RiskConstraint(DefaultConfig());

        var plan = constraint.Evaluate(
            DefaultTradeSetup(),
            new DailyRiskState(
                RealizedPnlToday: -500,
                DailyTradeCount: 0,
                ConsecutiveLosses: 0,
                CurrentMarginUsed: 0));

        Assert.Equal(TradePlanStatus.Rejected, plan.Status);
        Assert.Equal(RiskRejectReason.DailyLossLimitReached, plan.RejectReason);
    }

    [Fact]
    public void RejectsWhenDailyProfitLockReached()
    {
        var constraint = new RiskConstraint(DefaultConfig());

        var plan = constraint.Evaluate(
            DefaultTradeSetup(),
            new DailyRiskState(
                RealizedPnlToday: 500,
                DailyTradeCount: 0,
                ConsecutiveLosses: 0,
                CurrentMarginUsed: 0));

        Assert.Equal(TradePlanStatus.Rejected, plan.Status);
        Assert.Equal(RiskRejectReason.DailyProfitLockReached, plan.RejectReason);
    }

    [Fact]
    public void RejectsWhenMaxDailyTradesReached()
    {
        var constraint = new RiskConstraint(DefaultConfig());

        var plan = constraint.Evaluate(
            DefaultTradeSetup(),
            new DailyRiskState(
                RealizedPnlToday: 0,
                DailyTradeCount: 3,
                ConsecutiveLosses: 0,
                CurrentMarginUsed: 0));

        Assert.Equal(TradePlanStatus.Rejected, plan.Status);
        Assert.Equal(RiskRejectReason.MaxDailyTradesReached, plan.RejectReason);
    }

    [Fact]
    public void RejectsWhenConsecutiveLossLimitReached()
    {
        var constraint = new RiskConstraint(DefaultConfig());

        var plan = constraint.Evaluate(
            DefaultTradeSetup(),
            new DailyRiskState(
                RealizedPnlToday: 0,
                DailyTradeCount: 0,
                ConsecutiveLosses: 10,
                CurrentMarginUsed: 0));

        Assert.Equal(TradePlanStatus.Rejected, plan.Status);
        Assert.Equal(RiskRejectReason.ConsecutiveLossLimitReached, plan.RejectReason);
    }

    [Fact]
    public void RejectsWhenAccountRCanNotCoverOneLotTradeR()
    {
        var constraint = new RiskConstraint(DefaultConfig());

        var plan = constraint.Evaluate(
            new TradeSetup(
                Symbol: "MA",
                Direction: TradeDirection.Long,
                EntryPrice: 3000,
                StopPrice: 2900,
                Multiplier: 10,
                EstimatedRoundTripCostPerLot: 20,
                OneLotMargin: 3000),
            DefaultDailyState());

        Assert.Equal(TradePlanStatus.Rejected, plan.Status);
        Assert.Equal(RiskRejectReason.NotEnoughAccountR, plan.RejectReason);
        Assert.Equal(0, plan.AllowedLots);
    }

    [Fact]
    public void RejectsWhenCostInRExceedsLimit()
    {
        var constraint = new RiskConstraint(DefaultConfig());

        var plan = constraint.Evaluate(
            new TradeSetup(
                Symbol: "MA",
                Direction: TradeDirection.Long,
                EntryPrice: 100,
                StopPrice: 99,
                Multiplier: 10,
                EstimatedRoundTripCostPerLot: 10,
                OneLotMargin: 1000),
            DefaultDailyState());

        Assert.Equal(TradePlanStatus.Rejected, plan.Status);
        Assert.Equal(RiskRejectReason.CostTooHigh, plan.RejectReason);
        Assert.Equal(0.5, plan.CostInR, 6);
    }

    [Fact]
    public void RejectsWhenMarginAfterOpenExceedsLimit()
    {
        var constraint = new RiskConstraint(DefaultConfig());

        var plan = constraint.Evaluate(
            DefaultTradeSetup(),
            new DailyRiskState(
                RealizedPnlToday: 0,
                DailyTradeCount: 0,
                ConsecutiveLosses: 0,
                CurrentMarginUsed: 14000));

        Assert.Equal(TradePlanStatus.Rejected, plan.Status);
        Assert.Equal(RiskRejectReason.MarginUsageExceeded, plan.RejectReason);
        Assert.Equal(17000, plan.MarginAfterOpen);
    }

    [Fact]
    public void RejectsInvalidTradeSetupWithoutThrowing()
    {
        var constraint = new RiskConstraint(DefaultConfig());

        var plan = constraint.Evaluate(
            new TradeSetup(
                Symbol: "MA",
                Direction: TradeDirection.Long,
                EntryPrice: 3000,
                StopPrice: 2980,
                Multiplier: 0,
                EstimatedRoundTripCostPerLot: 20,
                OneLotMargin: 3000),
            DefaultDailyState());

        Assert.Equal(TradePlanStatus.Rejected, plan.Status);
        Assert.Equal(RiskRejectReason.InvalidTradeSetup, plan.RejectReason);
        Assert.Equal(0, plan.AllowedLots);
        Assert.Equal(0, plan.TargetPrice);
    }

    private static RiskConstraintConfig DefaultConfig()
    {
        return new RiskConstraintConfig(
            AccountEquity: 50000,
            RiskPercentPerTrade: 0.005,
            MinPlannedRewardR: 2.5,
            PerTradeCostMaxR: 0.20,
            MaxMarginUsageRatio: 0.30,
            DailyLossLimitMultiple: 2,
            DailyProfitLockMultiple: 2,
            MaxDailyTrades: 3,
            MaxConsecutiveLosses: 10);
    }

    private static DailyRiskState DefaultDailyState()
    {
        return new DailyRiskState(
            RealizedPnlToday: 260,
            DailyTradeCount: 1,
            ConsecutiveLosses: 0,
            CurrentMarginUsed: 0);
    }

    private static TradeSetup DefaultTradeSetup()
    {
        return new TradeSetup(
            Symbol: "MA",
            Direction: TradeDirection.Long,
            EntryPrice: 3000,
            StopPrice: 2980,
            Multiplier: 10,
            EstimatedRoundTripCostPerLot: 20,
            OneLotMargin: 3000);
    }
}
