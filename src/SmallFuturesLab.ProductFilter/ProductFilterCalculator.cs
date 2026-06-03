using SmallFuturesLab.Risk;

namespace SmallFuturesLab.ProductFilter;

/// <summary>
/// 品种筛选计算器，计算公式字段并复用交易许可逻辑。
/// </summary>
public class ProductFilterCalculator
{
    private readonly TradePermissionEvaluator _evaluator = new();

    private static readonly RiskPolicy DefaultPolicy = new()
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

    /// <summary>
    /// 计算单行数据。
    /// </summary>
    /// <param name="row">原始数据行。</param>
    /// <returns>计算结果。</returns>
    public ProductFilterCalculationResult Calculate(ProductFilterRow row)
    {
        // 公式计算
        var tickValue = row.TickSize * row.Multiplier;
        var marginPerLot = row.Price * row.Multiplier * row.MarginRate;
        var atrMoneyPerLot = row.TypicalAtr * row.Multiplier;
        var stopRiskMoney = row.StopDistance * row.Multiplier;
        var slippageMoney = row.SlippageTicks * row.TickSize * row.Multiplier;
        var costMoney = row.RoundTripFeePerLot + slippageMoney;
        var totalRiskMoney = stopRiskMoney + costMoney;
        var riskRate10k = totalRiskMoney / 10000.0;
        var riskRate20k = totalRiskMoney / 20000.0;
        var costRatio = stopRiskMoney > 0 ? costMoney / stopRiskMoney : double.PositiveInfinity;
        var marginRate10k = marginPerLot / 10000.0;
        var marginRate20k = marginPerLot / 20000.0;

        // 调用交易许可逻辑
        var result10k = EvaluateForEquity(row, 10000);
        var result20k = EvaluateForEquity(row, 20000);

        // 流动性降级
        result10k = ApplyLiquidityDowngrade(result10k, row);
        result20k = ApplyLiquidityDowngrade(result20k, row);

        // 生成原因
        var reasons = BuildReasons(row, result10k, result20k, totalRiskMoney, costRatio, marginPerLot);

        var calculatedRow = row with
        {
            TickValue = tickValue,
            MarginPerLot = marginPerLot,
            AtrMoneyPerLot = atrMoneyPerLot,
            StopRiskMoney = stopRiskMoney,
            SlippageMoney = slippageMoney,
            CostMoney = costMoney,
            TotalRiskMoney = totalRiskMoney,
            RiskRate10k = riskRate10k,
            RiskRate20k = riskRate20k,
            CostRatio = costRatio,
            MarginRate10k = marginRate10k,
            MarginRate20k = marginRate20k,
            Result10k = result10k,
            Result20k = result20k,
            Reasons = reasons,
        };

        return new ProductFilterCalculationResult(calculatedRow, result10k, result20k, reasons);
    }

    private ProductFilterResultStatus EvaluateForEquity(ProductFilterRow row, double equity)
    {
        var account = new AccountSnapshot
        {
            Equity = equity,
            AvailableCash = equity,
            DailyLossSoFar = 0,
            TradeCountToday = 0,
        };

        var instrument = new InstrumentSpec
        {
            Price = row.Price,
            Multiplier = row.Multiplier,
            TickSize = row.TickSize,
            MarginRatio = row.MarginRate,
            FeePerRoundTrip = row.RoundTripFeePerLot,
        };

        var trade = new TradeIdea
        {
            EntryPrice = row.Price,
            StopPrice = row.Price - row.StopDistance,
            Lots = 1,
            SlippageTicks = row.SlippageTicks,
            IsOvernight = false,
            IsAddPosition = false,
            HasStop = true,
        };

        var permission = _evaluator.Evaluate(account, instrument, trade, DefaultPolicy);

        return permission.Status switch
        {
            TradePermissionStatus.Allowed => ProductFilterResultStatus.Allowed,
            TradePermissionStatus.Caution => ProductFilterResultStatus.Caution,
            TradePermissionStatus.Rejected => ProductFilterResultStatus.Rejected,
            _ => ProductFilterResultStatus.Rejected,
        };
    }

    private static ProductFilterResultStatus ApplyLiquidityDowngrade(ProductFilterResultStatus status, ProductFilterRow row)
    {
        var hasLiquidityIssue =
            row.LiquidityLevel == LiquidityLevel.Poor || row.LiquidityLevel == LiquidityLevel.Unknown
            || row.BookContinuityLevel == BookContinuityLevel.Poor || row.BookContinuityLevel == BookContinuityLevel.Unknown
            || row.RolloverClarity == RolloverClarity.Poor || row.RolloverClarity == RolloverClarity.Unknown;

        if (hasLiquidityIssue && status == ProductFilterResultStatus.Allowed)
        {
            return ProductFilterResultStatus.Caution;
        }

        return status;
    }

    private static string BuildReasons(
        ProductFilterRow row,
        ProductFilterResultStatus result10k,
        ProductFilterResultStatus result20k,
        double totalRiskMoney,
        double costRatio,
        double marginPerLot)
    {
        var parts = new List<string>();

        // 风险占比判断
        var riskRate10k = totalRiskMoney / 10000.0;
        var riskRate20k = totalRiskMoney / 20000.0;

        if (riskRate10k <= 0.01)
            parts.Add("10k 账户单笔风险在常规上限内");
        else if (riskRate10k <= 0.02)
            parts.Add("10k 账户单笔风险处于谨慎区间");
        else
            parts.Add("10k 账户单笔风险超过极限上限");

        if (riskRate20k <= 0.01)
            parts.Add("20k 账户单笔风险在常规上限内");
        else if (riskRate20k <= 0.02)
            parts.Add("20k 账户单笔风险处于谨慎区间");
        else
            parts.Add("20k 账户单笔风险超过极限上限");

        // 成本占比判断
        if (costRatio <= 0.20)
            parts.Add("成本占比在优先范围");
        else if (costRatio <= 0.30)
            parts.Add("成本占比超过 0.2R，处于谨慎区间");
        else
            parts.Add("成本占比超过 0.3R，原则上排除");

        // 保证金占比判断
        var marginRate10k = marginPerLot / 10000.0;
        if (marginRate10k <= 0.40)
            parts.Add("10k 账户保证金占用在优先范围");
        else if (marginRate10k <= 0.50)
            parts.Add("10k 账户保证金占用处于谨慎区间");
        else
            parts.Add("10k 账户保证金占用超过极限上限");

        // 流动性判断
        var hasLiquidityIssue =
            row.LiquidityLevel == LiquidityLevel.Poor || row.LiquidityLevel == LiquidityLevel.Unknown
            || row.BookContinuityLevel == BookContinuityLevel.Poor || row.BookContinuityLevel == BookContinuityLevel.Unknown
            || row.RolloverClarity == RolloverClarity.Poor || row.RolloverClarity == RolloverClarity.Unknown;

        if (hasLiquidityIssue)
            parts.Add("流动性或盘口数据不足，不能进入优先候选");

        // 结论
        if (result10k == ProductFilterResultStatus.Allowed && result20k == ProductFilterResultStatus.Allowed)
            parts.Add("两个账户规模均允许进入后续周期研究");
        else if (result10k == ProductFilterResultStatus.Rejected && result20k != ProductFilterResultStatus.Rejected)
            parts.Add("10k 账户排除，20k 账户可继续观察");
        else if (result10k == ProductFilterResultStatus.Caution && result20k == ProductFilterResultStatus.Allowed)
            parts.Add("10k 账户谨慎观察，20k 账户允许进入后续研究");
        else if (result10k == ProductFilterResultStatus.Rejected && result20k == ProductFilterResultStatus.Rejected)
            parts.Add("当前账户规模排除");
        else
            parts.Add("综合结论为谨慎观察");

        return string.Join("；", parts);
    }
}
