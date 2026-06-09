namespace SmallFuturesLab.Core.Products;

/// <summary>
/// 一手合约的经济参数。
/// 这里保存的是过滤器直接需要的每手经济值，不关心原始数据从哪里来。
/// </summary>
public sealed record PerLotEconomics
{
    /// <summary>当前或典型价格。</summary>
    public double Price { get; init; }

    /// <summary>一手保证金金额。</summary>
    public double MarginPerLot { get; init; }

    /// <summary>一跳金额。</summary>
    public double TickValue { get; init; }

    /// <summary>单手开平总手续费估计。</summary>
    public double RoundTripFeePerLot { get; init; }
}
