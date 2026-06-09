namespace SmallFuturesLab.Core.Models;

/// <summary>
/// 期货品种和合约的基础信息。
/// 这是品种过滤器的输入，不代表交易信号。
/// </summary>
public record ProductInfo
{
    /// <summary>交易所名称或简称。</summary>
    public string Exchange { get; init; } = string.Empty;

    /// <summary>品种名称。</summary>
    public string ProductName { get; init; } = string.Empty;

    /// <summary>品种代码，例如 MA、RB、CU。</summary>
    public string ProductCode { get; init; } = string.Empty;

    /// <summary>合约代码，例如 MA509、RB2510。</summary>
    public string ContractCode { get; init; } = string.Empty;

    /// <summary>当前或典型价格。</summary>
    public double Price { get; init; }

    /// <summary>合约乘数。交易星球表格不一定提供该字段。</summary>
    public double Multiplier { get; init; }

    /// <summary>最小变动价位。交易星球表格不一定提供该字段。</summary>
    public double TickSize { get; init; }

    /// <summary>一跳金额参考值。交易星球表格通常可提供“每跳毛利”。</summary>
    public double TickValue { get; init; }

    /// <summary>保证金比例。</summary>
    public double MarginRate { get; init; }

    /// <summary>一手保证金金额。如果数据源直接给出该字段，优先使用。</summary>
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

    /// <summary>是否主力合约。</summary>
    public bool IsMainContract { get; init; }

    /// <summary>数据日期。</summary>
    public string DataDate { get; init; } = string.Empty;

    /// <summary>数据来源。</summary>
    public string DataSource { get; init; } = string.Empty;

    /// <summary>备注。</summary>
    public string Remark { get; init; } = string.Empty;
}
