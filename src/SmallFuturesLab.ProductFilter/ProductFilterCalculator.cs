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
        var riskRate = totalRiskMoney / row.AccountEquity;
        var costRatio = stopRiskMoney > 0 ? costMoney / stopRiskMoney : double.PositiveInfinity;
        var marginRateOfEquity = marginPerLot / row.AccountEquity;

        // 调用交易许可逻辑，使用 AccountEquity 作为账户权益
        var permission = Evaluate(row);

        // 将 Risk 模块结果映射为品种筛选结果
        var result = permission.Status switch
        {
            TradePermissionStatus.Allowed => ProductFilterResultStatus.Allowed,
            TradePermissionStatus.Caution => ProductFilterResultStatus.Caution,
            TradePermissionStatus.Rejected => ProductFilterResultStatus.Rejected,
            _ => ProductFilterResultStatus.Rejected,
        };

        // 流动性降级
        result = ApplyLiquidityDowngrade(result, row);

        // 生成原因，优先复用 Risk 模块的 WarningItems / RejectedItems / Conclusion
        var reasons = BuildReasons(row, permission, result);

        var calculatedRow = row with
        {
            TickValue = tickValue,
            MarginPerLot = marginPerLot,
            AtrMoneyPerLot = atrMoneyPerLot,
            StopRiskMoney = stopRiskMoney,
            SlippageMoney = slippageMoney,
            CostMoney = costMoney,
            TotalRiskMoney = totalRiskMoney,
            RiskRate = riskRate,
            MarginRateOfEquity = marginRateOfEquity,
            CostRatio = costRatio,
            Result = result,
            Reasons = reasons,
        };

        return new ProductFilterCalculationResult(calculatedRow, result, reasons);
    }

    private TradePermissionResult Evaluate(ProductFilterRow row)
    {
        var account = new AccountSnapshot
        {
            Equity = row.AccountEquity,
            AvailableCash = row.AccountEquity,
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

        return _evaluator.Evaluate(account, instrument, trade, DefaultPolicy);
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

    private static string BuildReasons(ProductFilterRow row, TradePermissionResult permission, ProductFilterResultStatus finalResult)
    {
        var parts = new List<string>();

        // 优先复用 Risk 模块的 WarningItems / RejectedItems / Conclusion
        foreach (var rejectedItem in permission.RejectedItems)
        {
            parts.Add(rejectedItem);
        }

        foreach (var warningItem in permission.WarningItems)
        {
            parts.Add(warningItem);
        }

        if (!string.IsNullOrWhiteSpace(permission.Conclusion))
        {
            parts.Add(permission.Conclusion);
        }

        // 流动性判断
        var hasLiquidityIssue =
            row.LiquidityLevel == LiquidityLevel.Poor || row.LiquidityLevel == LiquidityLevel.Unknown
            || row.BookContinuityLevel == BookContinuityLevel.Poor || row.BookContinuityLevel == BookContinuityLevel.Unknown
            || row.RolloverClarity == RolloverClarity.Poor || row.RolloverClarity == RolloverClarity.Unknown;

        if (hasLiquidityIssue)
        {
            parts.Add("流动性或盘口数据不足，不能进入优先候选");
        }

        // 最终结论
        parts.Add(finalResult switch
        {
            ProductFilterResultStatus.Allowed => "进入后续周期研究",
            ProductFilterResultStatus.Caution => "谨慎观察",
            ProductFilterResultStatus.Rejected => "当前账户规模排除",
            _ => "当前账户规模排除",
        });

        return string.Join("；", parts);
    }
}
