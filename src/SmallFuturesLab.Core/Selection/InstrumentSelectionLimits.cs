namespace SmallFuturesLab.Core.Selection;

/// <summary>
/// 品种入选边界。
///
/// 这些边界用于判断品种是否值得进入今日候选池，不等同于账户风险边界。
/// </summary>
public sealed record InstrumentSelectionLimits
{
    /// <summary>
    /// 创建品种入选边界。
    /// </summary>
    /// <param name="MinVolume">允许入选的最低成交量。</param>
    /// <param name="MinOpenInterest">允许入选的最低持仓量。</param>
    /// <param name="MaxRoundTripFeePerLot">允许入选的一手最高往返手续费预估。</param>
    /// <param name="MaxTickValue">允许入选的一手最大最小跳动价值。</param>
    /// <param name="MaxMinimumTradeRiskAccountRRatio">允许入选的最小交易风险颗粒度相对 AccountR 的最大比例。</param>
    public InstrumentSelectionLimits(
        long MinVolume,
        long MinOpenInterest,
        double MaxRoundTripFeePerLot,
        double MaxTickValue,
        double MaxMinimumTradeRiskAccountRRatio)
    {
        this.MinVolume = MinVolume;
        this.MinOpenInterest = MinOpenInterest;
        this.MaxRoundTripFeePerLot = MaxRoundTripFeePerLot;
        this.MaxTickValue = MaxTickValue;
        this.MaxMinimumTradeRiskAccountRRatio = MaxMinimumTradeRiskAccountRRatio;
    }

    /// <summary>
    /// 允许入选的最低成交量。
    /// </summary>
    public long MinVolume { get; }

    /// <summary>
    /// 允许入选的最低持仓量。
    /// </summary>
    public long MinOpenInterest { get; }

    /// <summary>
    /// 允许入选的一手最高往返手续费预估。
    /// </summary>
    public double MaxRoundTripFeePerLot { get; }

    /// <summary>
    /// 允许入选的一手最大最小跳动价值。
    /// </summary>
    public double MaxTickValue { get; }

    /// <summary>
    /// 允许入选的最小交易风险颗粒度相对 AccountR 的最大比例。
    /// </summary>
    public double MaxMinimumTradeRiskAccountRRatio { get; }
}
