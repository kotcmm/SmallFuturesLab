namespace SmallFuturesLab.ProductFilter;

/// <summary>
/// 品种筛选 CSV 中的一行数据，包含原始采集字段和公式计算字段。
/// 账户规模（AccountEquity）是测算维度，不是固定字段名，支持任意账户规模。
/// </summary>
public record ProductFilterRow
{
    /// <summary>交易所。</summary>
    public string Exchange { get; init; } = string.Empty;

    /// <summary>品种名称。</summary>
    public string ProductName { get; init; } = string.Empty;

    /// <summary>品种代码。</summary>
    public string ProductCode { get; init; } = string.Empty;

    /// <summary>合约代码。</summary>
    public string ContractCode { get; init; } = string.Empty;

    /// <summary>典型价格（P）。</summary>
    public double Price { get; init; }

    /// <summary>合约乘数（M）。</summary>
    public double Multiplier { get; init; }

    /// <summary>最小变动价位（T）。</summary>
    public double TickSize { get; init; }

    /// <summary>一跳金额，TickValue = T × M。</summary>
    public double TickValue { get; init; }

    /// <summary>保证金比例（μ）。</summary>
    public double MarginRate { get; init; }

    /// <summary>一手保证金，MarginPerLot = P × M × μ。</summary>
    public double MarginPerLot { get; init; }

    /// <summary>单手开平总手续费估计（F）。</summary>
    public double RoundTripFeePerLot { get; init; }

    /// <summary>预估总滑点跳数（S）。</summary>
    public int SlippageTicks { get; init; }

    /// <summary>典型 ATR。</summary>
    public double TypicalAtr { get; init; }

    /// <summary>1 手 1 ATR 金额，AtrMoneyPerLot = ATR × M。</summary>
    public double AtrMoneyPerLot { get; init; }

    /// <summary>测算止损距离。</summary>
    public double StopDistance { get; init; }

    /// <summary>止损金额，StopRiskMoney = StopDistance × M。</summary>
    public double StopRiskMoney { get; init; }

    /// <summary>滑点金额，SlippageMoney = S × T × M。</summary>
    public double SlippageMoney { get; init; }

    /// <summary>成本金额，CostMoney = F + SlippageMoney。</summary>
    public double CostMoney { get; init; }

    /// <summary>含成本 1R，TotalRiskMoney = StopRiskMoney + CostMoney。</summary>
    public double TotalRiskMoney { get; init; }

    /// <summary>当前记录使用的账户权益（E）。</summary>
    public double AccountEquity { get; init; }

    /// <summary>当前账户规模下的风险占比，RiskRate = TotalRiskMoney / AccountEquity。</summary>
    public double RiskRate { get; init; }

    /// <summary>当前账户规模下的保证金占比，MarginRateOfEquity = MarginPerLot / AccountEquity。</summary>
    public double MarginRateOfEquity { get; init; }

    /// <summary>成本占止损风险比例，CostRatio = CostMoney / StopRiskMoney。</summary>
    public double CostRatio { get; init; }

    /// <summary>流动性等级。</summary>
    public LiquidityLevel LiquidityLevel { get; init; }

    /// <summary>盘口连续性等级。</summary>
    public BookContinuityLevel BookContinuityLevel { get; init; }

    /// <summary>换月清晰度。</summary>
    public RolloverClarity RolloverClarity { get; init; }

    /// <summary>当前账户规模下的结论。</summary>
    public ProductFilterResultStatus Result { get; init; }

    /// <summary>结论原因。</summary>
    public string Reasons { get; init; } = string.Empty;

    /// <summary>数据日期。</summary>
    public string DataDate { get; init; } = string.Empty;

    /// <summary>数据来源。</summary>
    public string DataSource { get; init; } = string.Empty;
}
