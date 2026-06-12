namespace SmallFuturesLab.Core.Selection;

/// <summary>
/// 收集器拿到的原始合约资料。
/// </summary>
public sealed record InstrumentRawInfo(
    string Symbol,
    double Multiplier,
    double PriceTick,
    double RoundTripFeePerLot,
    double LastPrice,
    long Volume,
    long OpenInterest,
    bool IsTradingAllowed);
