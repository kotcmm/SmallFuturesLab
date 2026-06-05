using SmallFuturesLab.ProductFilter;

namespace SmallFuturesLab.ProductData;

/// <summary>
/// 品种数据记录，表示从某个数据源采集到的一条原始或半标准化品种数据。
/// </summary>
public record ProductDataRecord
{
    /// <summary>交易所。</summary>
    public string Exchange { get; init; } = string.Empty;

    /// <summary>品种名称。</summary>
    public string ProductName { get; init; } = string.Empty;

    /// <summary>品种代码。</summary>
    public string ProductCode { get; init; } = string.Empty;

    /// <summary>合约代码。</summary>
    public string ContractCode { get; init; } = string.Empty;

    /// <summary>典型价格。</summary>
    public double Price { get; init; }

    /// <summary>合约乘数。</summary>
    public double? Multiplier { get; init; }

    /// <summary>最小变动价位。</summary>
    public double? TickSize { get; init; }

    /// <summary>保证金比例。</summary>
    public double MarginRate { get; init; }

    /// <summary>一手保证金金额。</summary>
    public double MarginPerLot { get; init; }

    /// <summary>单手开平总手续费估计。</summary>
    public double RoundTripFeePerLot { get; init; }

    /// <summary>开仓手续费。</summary>
    public double OpenFeePerLot { get; init; }

    /// <summary>平昨手续费。</summary>
    public double CloseYesterdayFeePerLot { get; init; }

    /// <summary>平今手续费。</summary>
    public double CloseTodayFeePerLot { get; init; }

    /// <summary>成交量。</summary>
    public double Volume { get; init; }

    /// <summary>持仓量。</summary>
    public double OpenInterest { get; init; }

    /// <summary>典型 ATR。</summary>
    public double TypicalAtr { get; init; }

    /// <summary>流动性等级。</summary>
    public LiquidityLevel LiquidityLevel { get; init; } = LiquidityLevel.Unknown;

    /// <summary>盘口连续性等级。</summary>
    public BookContinuityLevel BookContinuityLevel { get; init; } = BookContinuityLevel.Unknown;

    /// <summary>主力合约换月清晰度。</summary>
    public RolloverClarity RolloverClarity { get; init; } = RolloverClarity.Unknown;

    /// <summary>是否主力合约。</summary>
    public bool IsMainContract { get; init; }

    /// <summary>数据日期。</summary>
    public string DataDate { get; init; } = string.Empty;

    /// <summary>数据来源描述。</summary>
    public string DataSource { get; init; } = string.Empty;

    /// <summary>数据来源类型。</summary>
    public ProductDataSourceType DataSourceType { get; init; }

    /// <summary>是否需要复核。第三方研究数据必须为 true。</summary>
    public bool NeedsReview { get; init; }
}
