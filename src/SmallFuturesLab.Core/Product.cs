namespace SmallFuturesLab.Core;

/// <summary>
/// 表示一个待过滤的品种 / 合约信息。
/// 不包含数据来源信息，不生成交易建议。
/// </summary>
public sealed class Product
{
    /// <summary>交易所名称或简称。</summary>
    public string Exchange { get; set; } = string.Empty;

    /// <summary>品种代码，例如 MA、RB、CU。</summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>合约代码，例如 MA2601、RB2510。</summary>
    public string Contract { get; set; } = string.Empty;

    /// <summary>品种名称，例如 甲醇。</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>当前或典型价格。必须大于 0。</summary>
    public double Price { get; set; }

    /// <summary>合约乘数。必须大于 0。</summary>
    public double Multiplier { get; set; }

    /// <summary>最小变动价位（tick 大小）。必须大于 0。</summary>
    public double TickSize { get; set; }

    /// <summary>保证金比例。必须大于 0。</summary>
    public double MarginRate { get; set; }

    /// <summary>单手开平总手续费。必须大于等于 0。</summary>
    public double RoundTripFee { get; set; }

    /// <summary>
    /// 一跳金额。
    /// TickValue = TickSize * Multiplier。
    /// </summary>
    public double TickValue => TickSize * Multiplier;

    /// <summary>
    /// 一手保证金金额。
    /// MarginPerLot = Price * Multiplier * MarginRate。
    /// </summary>
    public double MarginPerLot => Price * Multiplier * MarginRate;
}
