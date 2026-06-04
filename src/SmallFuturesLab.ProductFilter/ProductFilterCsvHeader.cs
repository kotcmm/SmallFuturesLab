namespace SmallFuturesLab.ProductFilter;

/// <summary>
/// 品种筛选 CSV 表头定义。
/// </summary>
public static class ProductFilterCsvHeader
{
    /// <summary>
    /// 标准表头字段列表，顺序必须与模板一致。
    /// </summary>
    public static readonly IReadOnlyList<string> ExpectedHeaders = new[]
    {
        "Exchange",
        "ProductName",
        "ProductCode",
        "ContractCode",
        "Price",
        "Multiplier",
        "TickSize",
        "TickValue",
        "MarginRate",
        "MarginPerLot",
        "RoundTripFeePerLot",
        "SlippageTicks",
        "TypicalAtr",
        "AtrMoneyPerLot",
        "StopDistance",
        "StopRiskMoney",
        "SlippageMoney",
        "CostMoney",
        "TotalRiskMoney",
        "AccountEquity",
        "RiskRate",
        "MarginRateOfEquity",
        "CostRatio",
        "LiquidityLevel",
        "BookContinuityLevel",
        "RolloverClarity",
        "Result",
        "Reasons",
        "DataDate",
        "DataSource",
    };

    /// <summary>
    /// 外部采集字段和初始假设字段，这些字段在 CSV 中必须非空。
    /// </summary>
    public static readonly IReadOnlySet<string> RequiredFields = new HashSet<string>(StringComparer.Ordinal)
    {
        "Exchange",
        "ProductName",
        "ProductCode",
        "ContractCode",
        "Price",
        "Multiplier",
        "TickSize",
        "MarginRate",
        "RoundTripFeePerLot",
        "SlippageTicks",
        "TypicalAtr",
        "StopDistance",
        "AccountEquity",
        "LiquidityLevel",
        "BookContinuityLevel",
        "RolloverClarity",
        "DataDate",
        "DataSource",
    };
}
