using SmallFuturesLab.Core.Risk;

namespace SmallFuturesLab.Core.Tests.Risk;

public sealed class RiskConstraintTests
{
    [Fact]
    public void BuildsAcceptedLongTradePlanFromDocumentExample()
    {
        var constraint = new RiskConstraint(DefaultLimits());

        var plan = constraint.Evaluate(
            new TradeSetup(
                symbol: "MA",
                direction: TradeDirection.Long,
                entryPrice: 3000,
                stopPrice: 2980,
                multiplier: 10,
                estimatedRoundTripCostPerLot: 20,
                oneLotMargin: 3000),
            new DailyRiskState(
                realizedPnlToday: 260,
                dailyTradeCount: 1,
                consecutiveLosses: 0,
                currentMarginUsed: 0));

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
        var constraint = new RiskConstraint(DefaultLimits());

        var plan = constraint.Evaluate(
            new TradeSetup(
                symbol: "MA",
                direction: TradeDirection.Short,
                entryPrice: 2980,
                stopPrice: 3000,
                multiplier: 10,
                estimatedRoundTripCostPerLot: 20,
                oneLotMargin: 3000),
            DefaultDailyState());

        Assert.Equal(TradePlanStatus.Accepted, plan.Status);
        Assert.Equal(2925, plan.TargetPrice);
    }

    [Fact]
    public void RejectsWhenDailyLossLimitReached()
    {
        var constraint = new RiskConstraint(DefaultLimits());

        var plan = constraint.Evaluate(
            DefaultTradeSetup(),
            new DailyRiskState(
                realizedPnlToday: -500,
                dailyTradeCount: 0,
                consecutiveLosses: 0,
                currentMarginUsed: 0));

        Assert.Equal(TradePlanStatus.Rejected, plan.Status);
        Assert.Equal(RiskRejectReason.DailyLossLimitReached, plan.RejectReason);
    }

    [Fact]
    public void RejectsWhenDailyProfitLockReached()
    {
        var constraint = new RiskConstraint(DefaultLimits());

        var plan = constraint.Evaluate(
            DefaultTradeSetup(),
            new DailyRiskState(
                realizedPnlToday: 500,
                dailyTradeCount: 0,
                consecutiveLosses: 0,
                currentMarginUsed: 0));

        Assert.Equal(TradePlanStatus.Rejected, plan.Status);
        Assert.Equal(RiskRejectReason.DailyProfitLockReached, plan.RejectReason);
    }

    [Fact]
    public void RejectsWhenMaxDailyTradesReached()
    {
        var constraint = new RiskConstraint(DefaultLimits());

        var plan = constraint.Evaluate(
            DefaultTradeSetup(),
            new DailyRiskState(
                realizedPnlToday: 0,
                dailyTradeCount: 3,
                consecutiveLosses: 0,
                currentMarginUsed: 0));

        Assert.Equal(TradePlanStatus.Rejected, plan.Status);
        Assert.Equal(RiskRejectReason.MaxDailyTradesReached, plan.RejectReason);
    }

    [Fact]
    public void RejectsWhenConsecutiveLossLimitReached()
    {
        var constraint = new RiskConstraint(DefaultLimits());

        var plan = constraint.Evaluate(
            DefaultTradeSetup(),
            new DailyRiskState(
                realizedPnlToday: 0,
                dailyTradeCount: 0,
                consecutiveLosses: 10,
                currentMarginUsed: 0));

        Assert.Equal(TradePlanStatus.Rejected, plan.Status);
        Assert.Equal(RiskRejectReason.ConsecutiveLossLimitReached, plan.RejectReason);
    }

    [Fact]
    public void RejectsWhenAccountRCanNotCoverOneLotTradeR()
    {
        var constraint = new RiskConstraint(DefaultLimits());

        var plan = constraint.Evaluate(
            new TradeSetup(
                symbol: "MA",
                direction: TradeDirection.Long,
                entryPrice: 3000,
                stopPrice: 2900,
                multiplier: 10,
                estimatedRoundTripCostPerLot: 20,
                oneLotMargin: 3000),
            DefaultDailyState());

        Assert.Equal(TradePlanStatus.Rejected, plan.Status);
        Assert.Equal(RiskRejectReason.NotEnoughAccountR, plan.RejectReason);
        Assert.Equal(0, plan.AllowedLots);
    }

    [Fact]
    public void RejectsWhenCostInRExceedsLimit()
    {
        var constraint = new RiskConstraint(DefaultLimits());

        var plan = constraint.Evaluate(
            new TradeSetup(
                symbol: "MA",
                direction: TradeDirection.Long,
                entryPrice: 100,
                stopPrice: 99,
                multiplier: 10,
                estimatedRoundTripCostPerLot: 10,
                oneLotMargin: 1000),
            DefaultDailyState());

        Assert.Equal(TradePlanStatus.Rejected, plan.Status);
        Assert.Equal(RiskRejectReason.CostTooHigh, plan.RejectReason);
        Assert.Equal(0.5, plan.CostInR, 6);
    }

    [Fact]
    public void RejectsWhenMarginAfterOpenExceedsLimit()
    {
        var constraint = new RiskConstraint(DefaultLimits());

        var plan = constraint.Evaluate(
            DefaultTradeSetup(),
            new DailyRiskState(
                realizedPnlToday: 0,
                dailyTradeCount: 0,
                consecutiveLosses: 0,
                currentMarginUsed: 14000));

        Assert.Equal(TradePlanStatus.Rejected, plan.Status);
        Assert.Equal(RiskRejectReason.MarginUsageExceeded, plan.RejectReason);
        Assert.Equal(17000, plan.MarginAfterOpen);
    }

    [Fact]
    public void RejectsInvalidTradeSetupWithoutThrowing()
    {
        var constraint = new RiskConstraint(DefaultLimits());

        var plan = constraint.Evaluate(
            new TradeSetup(
                symbol: "MA",
                direction: TradeDirection.Long,
                entryPrice: 3000,
                stopPrice: 2980,
                multiplier: 0,
                estimatedRoundTripCostPerLot: 20,
                oneLotMargin: 3000),
            DefaultDailyState());

        Assert.Equal(TradePlanStatus.Rejected, plan.Status);
        Assert.Equal(RiskRejectReason.InvalidTradeSetup, plan.RejectReason);
        Assert.Equal(0, plan.AllowedLots);
        Assert.Equal(0, plan.TargetPrice);
    }

    private static AccountRiskLimits DefaultLimits()
    {
        return new AccountRiskLimits(
            accountEquity: 50000,
            riskPercentPerTrade: 0.005,
            minPlannedRewardR: 2.5,
            perTradeCostMaxR: 0.20,
            maxMarginUsageRatio: 0.30,
            dailyLossLimitMultiple: 2,
            dailyProfitLockMultiple: 2,
            maxDailyTrades: 3,
            maxConsecutiveLosses: 10);
    }

    private static DailyRiskState DefaultDailyState()
    {
        return new DailyRiskState(
            realizedPnlToday: 260,
            dailyTradeCount: 1,
            consecutiveLosses: 0,
            currentMarginUsed: 0);
    }

    private static TradeSetup DefaultTradeSetup()
    {
        return new TradeSetup(
            symbol: "MA",
            direction: TradeDirection.Long,
            entryPrice: 3000,
            stopPrice: 2980,
            multiplier: 10,
            estimatedRoundTripCostPerLot: 20,
            oneLotMargin: 3000);
    }
}
