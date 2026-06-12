namespace SmallFuturesLab.Core.Selection;

/// <summary>
/// 品种过滤使用的统一内部对象。
/// </summary>
public sealed record InstrumentFilterProfile(
    string Symbol,
    double Multiplier,
    double PriceTick,
    double RoundTripFeePerLot,
    double LastPrice,
    long Volume,
    long OpenInterest,
    bool IsTradingAllowed)
{
    /// <summary>
    /// 最小价格跳动对应的一手盈亏金额。
    /// </summary>
    public double TickValue => PriceTick * Multiplier;
}
