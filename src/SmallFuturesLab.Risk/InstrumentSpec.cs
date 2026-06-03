namespace SmallFuturesLab.Risk;

/// <summary>
/// 合约规格，描述交易品种的基础参数。
/// </summary>
public record InstrumentSpec
{
    /// <summary>
    /// 当前价格（P）。
    /// </summary>
    public double Price { get; init; }

    /// <summary>
    /// 合约乘数（M），每点对应多少元。
    /// </summary>
    public double Multiplier { get; init; }

    /// <summary>
    /// 最小变动价位（T），即 1 tick 的价格。
    /// </summary>
    public double TickSize { get; init; }

    /// <summary>
    /// 保证金比例（μ）。
    /// </summary>
    public double MarginRatio { get; init; }

    /// <summary>
    /// 单手开平总手续费估计（F），单位为元。
    /// </summary>
    public double FeePerRoundTrip { get; init; }
}
