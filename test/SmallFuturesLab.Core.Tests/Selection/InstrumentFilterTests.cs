using SmallFuturesLab.Core.Risk;
using SmallFuturesLab.Core.Selection;

namespace SmallFuturesLab.Core.Tests.Selection;

public sealed class InstrumentFilterTests
{
    [Fact]
    public void AcceptedInstrumentCanBeQueriedAfterFilterRun()
    {
        var store = Run(DefaultRawInfo());

        var accepted = store.GetAcceptedInstruments();

        Assert.Single(accepted);
        Assert.Equal("MA", accepted[0].Symbol);
    }

    [Fact]
    public void RejectedInstrumentDoesNotAppearInAcceptedQuery()
    {
        var store = Run(DefaultRawInfo(isTradingAllowed: false));

        Assert.Empty(store.GetAcceptedInstruments());
        Assert.Equal(InstrumentRejectReason.TradingNotAllowed, store.GetAllResults().Single().Decision.RejectReason);
    }

    [Fact]
    public void RejectsWhenVolumeTooLow()
    {
        var store = Run(DefaultRawInfo(volume: 99));

        Assert.Equal(InstrumentRejectReason.VolumeTooLow, store.GetAllResults().Single().Decision.RejectReason);
    }

    [Fact]
    public void RejectsWhenOpenInterestTooLow()
    {
        var store = Run(DefaultRawInfo(openInterest: 99));

        Assert.Equal(InstrumentRejectReason.OpenInterestTooLow, store.GetAllResults().Single().Decision.RejectReason);
    }

    [Fact]
    public void RejectsWhenFeeTooHigh()
    {
        var store = Run(DefaultRawInfo(roundTripFeePerLot: 51));

        Assert.Equal(InstrumentRejectReason.FeeTooHigh, store.GetAllResults().Single().Decision.RejectReason);
    }

    [Fact]
    public void RejectsWhenTickValueTooLarge()
    {
        var store = Run(DefaultRawInfo(priceTick: 11));

        Assert.Equal(InstrumentRejectReason.TickValueTooLarge, store.GetAllResults().Single().Decision.RejectReason);
    }

    [Fact]
    public void RejectsWhenMinimumTradeRiskTooLarge()
    {
        var store = Run(
            rawInfo: DefaultRawInfo(priceTick: 9, roundTripFeePerLot: 20),
            selectionLimits: DefaultSelectionLimits(maxTickValue: 500, maxMinimumTradeRiskAccountRRatio: 0.4));

        Assert.Equal(InstrumentRejectReason.MinimumTradeRiskTooLarge, store.GetAllResults().Single().Decision.RejectReason);
    }

    [Fact]
    public void ReplacesPreviousFilterResultsOnNextRun()
    {
        var store = new FilteredInstrumentStore();
        var filter = CreateFilter(store, DefaultRawInfo());
        filter.Run(DefaultAccountRiskLimits(), DefaultSelectionLimits());

        filter = CreateFilter(store, DefaultRawInfo(symbol: "RB"));
        filter.Run(DefaultAccountRiskLimits(), DefaultSelectionLimits());

        var accepted = store.GetAcceptedInstruments();
        Assert.Single(accepted);
        Assert.Equal("RB", accepted[0].Symbol);
    }

    private static FilteredInstrumentStore Run(
        InstrumentRawInfo rawInfo,
        InstrumentSelectionLimits? selectionLimits = null)
    {
        var store = new FilteredInstrumentStore();
        var filter = CreateFilter(store, rawInfo);

        filter.Run(
            accountRiskLimits: DefaultAccountRiskLimits(),
            selectionLimits: selectionLimits ?? DefaultSelectionLimits());

        return store;
    }

    private static InstrumentFilter CreateFilter(
        FilteredInstrumentStore store,
        params InstrumentRawInfo[] rawInfos)
    {
        return new InstrumentFilter(
            collector: new TestInstrumentCollector(rawInfos),
            mapper: new InstrumentFilterProfileMapper(),
            riskEvaluator: new InstrumentRiskEvaluator(),
            store: store);
    }

    private static AccountRiskLimits DefaultAccountRiskLimits()
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

    private static InstrumentSelectionLimits DefaultSelectionLimits(
        long minVolume = 100,
        long minOpenInterest = 100,
        double maxRoundTripFeePerLot = 50,
        double maxTickValue = 100,
        double maxMinimumTradeRiskAccountRRatio = 0.5)
    {
        return new InstrumentSelectionLimits(
            MinVolume: minVolume,
            MinOpenInterest: minOpenInterest,
            MaxRoundTripFeePerLot: maxRoundTripFeePerLot,
            MaxTickValue: maxTickValue,
            MaxMinimumTradeRiskAccountRRatio: maxMinimumTradeRiskAccountRRatio);
    }

    private static InstrumentRawInfo DefaultRawInfo(
        string symbol = "MA",
        double multiplier = 10,
        double priceTick = 1,
        double roundTripFeePerLot = 20,
        double lastPrice = 3000,
        long volume = 1000,
        long openInterest = 1000,
        bool isTradingAllowed = true)
    {
        return new InstrumentRawInfo(
            Symbol: symbol,
            Multiplier: multiplier,
            PriceTick: priceTick,
            RoundTripFeePerLot: roundTripFeePerLot,
            LastPrice: lastPrice,
            Volume: volume,
            OpenInterest: openInterest,
            IsTradingAllowed: isTradingAllowed);
    }

    private sealed class TestInstrumentCollector : IInstrumentCollector
    {
        private readonly IReadOnlyList<InstrumentRawInfo> _rawInfos;

        public TestInstrumentCollector(IReadOnlyList<InstrumentRawInfo> rawInfos)
        {
            _rawInfos = rawInfos;
        }

        public IReadOnlyList<InstrumentRawInfo> Collect()
        {
            return _rawInfos;
        }
    }
}
