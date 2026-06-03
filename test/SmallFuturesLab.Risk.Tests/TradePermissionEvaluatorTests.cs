namespace SmallFuturesLab.Risk.Tests;

public class TradePermissionEvaluatorTests
{
    private static RiskPolicy DefaultPolicy() => new()
    {
        RecommendedRiskRate = 0.005,
        NormalRiskRate = 0.010,
        ExtremeRiskRate = 0.020,
        DailyMaxLossRate = 0.020,
        PreferredMarginRate = 0.40,
        ExtremeMarginRate = 0.50,
        PreferredCostRatio = 0.20,
        ExtremeCostRatio = 0.30,
        MaxTradesPerDay = 3,
    };

    private static AccountSnapshot DefaultAccount() => new()
    {
        Equity = 20000,
        AvailableCash = 18000,
        DailyLossSoFar = 0,
        TradeCountToday = 0,
    };

    private static InstrumentSpec DefaultInstrument() => new()
    {
        Price = 2500,
        Multiplier = 10,
        TickSize = 1,
        MarginRatio = 0.10,
        FeePerRoundTrip = 6,
    };

    private static TradeIdea DefaultTradeIdea() => new()
    {
        EntryPrice = 2500,
        StopPrice = 2488,
        Lots = 1,
        SlippageTicks = 2,
        IsOvernight = false,
        IsAddPosition = false,
        HasStop = true,
    };

    /// <summary>
    /// 文档中的完整示例：成本占比 21.67% 超过推荐上限 0.2R，应输出 Caution。
    /// </summary>
    [Fact]
    public void Evaluate_DocumentExample_ReturnsCaution()
    {
        var evaluator = new TradePermissionEvaluator();
        var result = evaluator.Evaluate(DefaultAccount(), DefaultInstrument(), DefaultTradeIdea(), DefaultPolicy());

        Assert.Equal(TradePermissionStatus.Caution, result.Status);
    }

    /// <summary>
    /// 单笔风险超过极限上限 2% 时输出 Rejected。
    /// </summary>
    [Fact]
    public void Evaluate_RiskExceedsExtremeLimit_ReturnsRejected()
    {
        var evaluator = new TradePermissionEvaluator();
        var trade = DefaultTradeIdea() with { StopPrice = 2460 };
        var result = evaluator.Evaluate(DefaultAccount(), DefaultInstrument(), trade, DefaultPolicy());

        Assert.Equal(TradePermissionStatus.Rejected, result.Status);
        Assert.Contains(result.RejectedItems, r => r.Contains("单笔风险"));
    }

    /// <summary>
    /// 成本占比超过极限上限 0.3R 时输出 Rejected。
    /// </summary>
    [Fact]
    public void Evaluate_CostRatioExceedsExtremeLimit_ReturnsRejected()
    {
        var evaluator = new TradePermissionEvaluator();
        var instrument = DefaultInstrument() with { FeePerRoundTrip = 50 };
        var result = evaluator.Evaluate(DefaultAccount(), instrument, DefaultTradeIdea(), DefaultPolicy());

        Assert.Equal(TradePermissionStatus.Rejected, result.Status);
        Assert.Contains(result.RejectedItems, r => r.Contains("成本占比"));
    }

    /// <summary>
    /// 保证金占用超过极限上限 50% 时输出 Rejected。
    /// </summary>
    [Fact]
    public void Evaluate_MarginExceedsExtremeLimit_ReturnsRejected()
    {
        var evaluator = new TradePermissionEvaluator();
        var instrument = DefaultInstrument() with { MarginRatio = 0.55 };
        var result = evaluator.Evaluate(DefaultAccount(), instrument, DefaultTradeIdea(), DefaultPolicy());

        Assert.Equal(TradePermissionStatus.Rejected, result.Status);
        Assert.Contains(result.RejectedItems, r => r.Contains("保证金"));
    }

    /// <summary>
    /// 本笔亏损后超过每日亏损上限时输出 Rejected。
    /// </summary>
    [Fact]
    public void Evaluate_ProjectedDailyLossExceedsLimit_ReturnsRejected()
    {
        var evaluator = new TradePermissionEvaluator();
        var account = DefaultAccount() with { DailyLossSoFar = 300 };
        var result = evaluator.Evaluate(account, DefaultInstrument(), DefaultTradeIdea(), DefaultPolicy());

        Assert.Equal(TradePermissionStatus.Rejected, result.Status);
        Assert.Contains(result.RejectedItems, r => r.Contains("每日亏损上限"));
    }

    /// <summary>
    /// 今日交易次数达到上限时输出 Rejected。
    /// </summary>
    [Fact]
    public void Evaluate_TradeCountReachedMax_ReturnsRejected()
    {
        var evaluator = new TradePermissionEvaluator();
        var account = DefaultAccount() with { TradeCountToday = 3 };
        var result = evaluator.Evaluate(account, DefaultInstrument(), DefaultTradeIdea(), DefaultPolicy());

        Assert.Equal(TradePermissionStatus.Rejected, result.Status);
        Assert.Contains(result.RejectedItems, r => r.Contains("交易次数"));
    }

    /// <summary>
    /// 手数不是 1 手时输出 Rejected。
    /// </summary>
    [Fact]
    public void Evaluate_LotsNotEqualToOne_ReturnsRejected()
    {
        var evaluator = new TradePermissionEvaluator();
        var trade = DefaultTradeIdea() with { Lots = 2 };
        var result = evaluator.Evaluate(DefaultAccount(), DefaultInstrument(), trade, DefaultPolicy());

        Assert.Equal(TradePermissionStatus.Rejected, result.Status);
        Assert.Contains(result.RejectedItems, r => r.Contains("手数"));
    }

    /// <summary>
    /// 隔夜交易时输出 Rejected。
    /// </summary>
    [Fact]
    public void Evaluate_OvernightTrade_ReturnsRejected()
    {
        var evaluator = new TradePermissionEvaluator();
        var trade = DefaultTradeIdea() with { IsOvernight = true };
        var result = evaluator.Evaluate(DefaultAccount(), DefaultInstrument(), trade, DefaultPolicy());

        Assert.Equal(TradePermissionStatus.Rejected, result.Status);
        Assert.Contains(result.RejectedItems, r => r.Contains("隔夜"));
    }

    /// <summary>
    /// 加仓交易时输出 Rejected。
    /// </summary>
    [Fact]
    public void Evaluate_AddPositionTrade_ReturnsRejected()
    {
        var evaluator = new TradePermissionEvaluator();
        var trade = DefaultTradeIdea() with { IsAddPosition = true };
        var result = evaluator.Evaluate(DefaultAccount(), DefaultInstrument(), trade, DefaultPolicy());

        Assert.Equal(TradePermissionStatus.Rejected, result.Status);
        Assert.Contains(result.RejectedItems, r => r.Contains("加仓"));
    }

    /// <summary>
    /// 所有条件优秀时输出 Allowed。
    /// </summary>
    [Fact]
    public void Evaluate_AllConditionsExcellent_ReturnsAllowed()
    {
        var evaluator = new TradePermissionEvaluator();
        var trade = DefaultTradeIdea() with { EntryPrice = 2500, StopPrice = 2487 };
        var result = evaluator.Evaluate(DefaultAccount(), DefaultInstrument(), trade, DefaultPolicy());

        Assert.Equal(TradePermissionStatus.Allowed, result.Status);
    }

    /// <summary>
    /// 单笔风险超过常规上限 1% 但未超过极限上限 2% 时输出 Caution。
    /// </summary>
    [Fact]
    public void Evaluate_RiskExceedsNormalButNotExtreme_ReturnsCaution()
    {
        var evaluator = new TradePermissionEvaluator();
        var trade = DefaultTradeIdea() with { EntryPrice = 2500, StopPrice = 2480 };
        var result = evaluator.Evaluate(DefaultAccount(), DefaultInstrument(), trade, DefaultPolicy());

        Assert.Equal(TradePermissionStatus.Caution, result.Status);
    }

    /// <summary>
    /// 保证金占用超过推荐上限 40% 但未超过极限上限 50% 时输出 Caution。
    /// </summary>
    [Fact]
    public void Evaluate_MarginExceedsPreferredButNotExtreme_ReturnsCaution()
    {
        var evaluator = new TradePermissionEvaluator();
        var instrument = DefaultInstrument() with { MarginRatio = 0.35 };
        var result = evaluator.Evaluate(DefaultAccount(), instrument, DefaultTradeIdea(), DefaultPolicy());

        Assert.Equal(TradePermissionStatus.Caution, result.Status);
        Assert.Contains(result.WarningItems, w => w.Contains("保证金"));
    }

    /// <summary>
    /// 成本占比超过推荐上限 0.2R 但未超过极限上限 0.3R 时输出 Caution。
    /// </summary>
    [Fact]
    public void Evaluate_CostRatioExceedsPreferredButNotExtreme_ReturnsCaution()
    {
        var evaluator = new TradePermissionEvaluator();
        var instrument = DefaultInstrument() with { FeePerRoundTrip = 15 };
        var result = evaluator.Evaluate(DefaultAccount(), instrument, DefaultTradeIdea(), DefaultPolicy());

        Assert.Equal(TradePermissionStatus.Caution, result.Status);
        Assert.Contains(result.WarningItems, w => w.Contains("成本占比"));
        Assert.True(result.Metrics.CostRatio > 0.20, "CostRatio 应大于 0.20");
        Assert.True(result.Metrics.CostRatio <= 0.30, "CostRatio 应小于等于 0.30");
    }

    /// <summary>
    /// 多个拒绝原因同时触发时，结果中必须同时返回多个原因。
    /// </summary>
    [Fact]
    public void Evaluate_MultipleRejections_ReturnsAllReasons()
    {
        var evaluator = new TradePermissionEvaluator();
        var trade = DefaultTradeIdea() with
        {
            Lots = 2,
            IsOvernight = true,
            IsAddPosition = true,
        };
        var result = evaluator.Evaluate(DefaultAccount(), DefaultInstrument(), trade, DefaultPolicy());

        Assert.Equal(TradePermissionStatus.Rejected, result.Status);
        Assert.Contains(result.RejectedItems, r => r.Contains("手数"));
        Assert.Contains(result.RejectedItems, r => r.Contains("隔夜"));
        Assert.Contains(result.RejectedItems, r => r.Contains("加仓"));
    }

    // ========== 无效输入测试 ==========

    /// <summary>
    /// 账户权益小于等于 0 时输出 Rejected。
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1000)]
    public void Evaluate_EquityNotPositive_ReturnsRejected(double equity)
    {
        var evaluator = new TradePermissionEvaluator();
        var account = DefaultAccount() with { Equity = equity };
        var result = evaluator.Evaluate(account, DefaultInstrument(), DefaultTradeIdea(), DefaultPolicy());

        Assert.Equal(TradePermissionStatus.Rejected, result.Status);
        Assert.Contains(result.RejectedItems, r => r.Contains("权益"));
    }

    /// <summary>
    /// 最小变动价位小于等于 0 时输出 Rejected。
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Evaluate_TickSizeNotPositive_ReturnsRejected(double tickSize)
    {
        var evaluator = new TradePermissionEvaluator();
        var instrument = DefaultInstrument() with { TickSize = tickSize };
        var result = evaluator.Evaluate(DefaultAccount(), instrument, DefaultTradeIdea(), DefaultPolicy());

        Assert.Equal(TradePermissionStatus.Rejected, result.Status);
        Assert.Contains(result.RejectedItems, r => r.Contains("最小变动价位"));
    }

    /// <summary>
    /// 合约乘数小于等于 0 时输出 Rejected。
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void Evaluate_MultiplierNotPositive_ReturnsRejected(double multiplier)
    {
        var evaluator = new TradePermissionEvaluator();
        var instrument = DefaultInstrument() with { Multiplier = multiplier };
        var result = evaluator.Evaluate(DefaultAccount(), instrument, DefaultTradeIdea(), DefaultPolicy());

        Assert.Equal(TradePermissionStatus.Rejected, result.Status);
        Assert.Contains(result.RejectedItems, r => r.Contains("合约乘数"));
    }

    /// <summary>
    /// 手数小于等于 0 时输出 Rejected。
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Evaluate_LotsNotPositive_ReturnsRejected(int lots)
    {
        var evaluator = new TradePermissionEvaluator();
        var trade = DefaultTradeIdea() with { Lots = lots };
        var result = evaluator.Evaluate(DefaultAccount(), DefaultInstrument(), trade, DefaultPolicy());

        Assert.Equal(TradePermissionStatus.Rejected, result.Status);
        Assert.Contains(result.RejectedItems, r => r.Contains("手数"));
    }

    /// <summary>
    /// 滑点跳数小于 0 时输出 Rejected。
    /// </summary>
    [Theory]
    [InlineData(-1)]
    [InlineData(-5)]
    public void Evaluate_SlippageTicksNegative_ReturnsRejected(int slippageTicks)
    {
        var evaluator = new TradePermissionEvaluator();
        var trade = DefaultTradeIdea() with { SlippageTicks = slippageTicks };
        var result = evaluator.Evaluate(DefaultAccount(), DefaultInstrument(), trade, DefaultPolicy());

        Assert.Equal(TradePermissionStatus.Rejected, result.Status);
        Assert.Contains(result.RejectedItems, r => r.Contains("滑点"));
    }

    /// <summary>
    /// 单手开平总手续费小于 0 时输出 Rejected。
    /// </summary>
    [Theory]
    [InlineData(-1)]
    [InlineData(-10)]
    public void Evaluate_FeePerRoundTripNegative_ReturnsRejected(double fee)
    {
        var evaluator = new TradePermissionEvaluator();
        var instrument = DefaultInstrument() with { FeePerRoundTrip = fee };
        var result = evaluator.Evaluate(DefaultAccount(), instrument, DefaultTradeIdea(), DefaultPolicy());

        Assert.Equal(TradePermissionStatus.Rejected, result.Status);
        Assert.Contains(result.RejectedItems, r => r.Contains("手续费"));
    }

    /// <summary>
    /// 入场价与止损价相同时输出 Rejected。
    /// </summary>
    [Fact]
    public void Evaluate_EntryPriceEqualsStopPrice_ReturnsRejected()
    {
        var evaluator = new TradePermissionEvaluator();
        var trade = DefaultTradeIdea() with { EntryPrice = 2500, StopPrice = 2500 };
        var result = evaluator.Evaluate(DefaultAccount(), DefaultInstrument(), trade, DefaultPolicy());

        Assert.Equal(TradePermissionStatus.Rejected, result.Status);
        Assert.Contains(result.RejectedItems, r => r.Contains("止损"));
    }

    /// <summary>
    /// 无明确止损时输出 Rejected。
    /// </summary>
    [Fact]
    public void Evaluate_HasStopIsFalse_ReturnsRejected()
    {
        var evaluator = new TradePermissionEvaluator();
        var trade = DefaultTradeIdea() with { HasStop = false };
        var result = evaluator.Evaluate(DefaultAccount(), DefaultInstrument(), trade, DefaultPolicy());

        Assert.Equal(TradePermissionStatus.Rejected, result.Status);
        Assert.Contains(result.RejectedItems, r => r.Contains("止损"));
    }

    // ========== 连续亏损压力测试 ==========

    /// <summary>
    /// 连续亏损压力全部满足通过标准时，PassedItems 应包含通过描述，WarningItems 不应包含接近上限描述。
    /// </summary>
    [Fact]
    public void Evaluate_ConsecutiveLossPressureAllPass_ContainsPassItemOnly()
    {
        var evaluator = new TradePermissionEvaluator();
        var result = evaluator.Evaluate(DefaultAccount(), DefaultInstrument(), DefaultTradeIdea(), DefaultPolicy());

        Assert.Contains(result.PassedItems, p => p.Contains("连续亏损压力测试通过"));
        Assert.DoesNotContain(result.WarningItems, w => w.Contains("连续亏损压力接近上限"));
    }

    /// <summary>
    /// 连续亏损压力超过通过标准但未达严重失败时，输出 Caution，WarningItems 应包含接近上限描述，PassedItems 不应包含通过描述。
    /// </summary>
    [Fact]
    public void Evaluate_ConsecutiveLossPressureNearLimit_ReturnsCautionWithoutPassItem()
    {
        var evaluator = new TradePermissionEvaluator();
        // StopPrice = 2480 时 TotalRiskMoney = 226，LossAfter5Rate = 5.65% > 5%
        var trade = DefaultTradeIdea() with { EntryPrice = 2500, StopPrice = 2480 };
        var result = evaluator.Evaluate(DefaultAccount(), DefaultInstrument(), trade, DefaultPolicy());

        Assert.Equal(TradePermissionStatus.Caution, result.Status);
        Assert.Contains(result.WarningItems, w => w.Contains("连续亏损压力接近上限"));
        Assert.DoesNotContain(result.PassedItems, p => p.Contains("连续亏损压力测试通过"));
    }

    /// <summary>
    /// 连续亏损压力严重失败时，RejectedItems 应包含严重失败描述。
    /// </summary>
    [Fact]
    public void Evaluate_ConsecutiveLossPressureExtremeFail_ContainsExtremeFailReason()
    {
        var evaluator = new TradePermissionEvaluator();
        var trade = DefaultTradeIdea() with { EntryPrice = 2500, StopPrice = 2460 };
        var result = evaluator.Evaluate(DefaultAccount(), DefaultInstrument(), trade, DefaultPolicy());

        Assert.Equal(TradePermissionStatus.Rejected, result.Status);
        Assert.Contains(result.RejectedItems, r => r.Contains("连续亏损压力测试严重失败"));
    }
}
