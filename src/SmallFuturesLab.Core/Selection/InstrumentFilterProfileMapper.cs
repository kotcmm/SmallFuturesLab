namespace SmallFuturesLab.Core.Selection;

/// <summary>
/// 把外部收集到的合约资料转换成品种过滤使用的统一对象。
/// </summary>
public sealed class InstrumentFilterProfileMapper
{
    public InstrumentFilterProfile Map(InstrumentRawInfo raw)
    {
        ArgumentNullException.ThrowIfNull(raw);

        return new InstrumentFilterProfile(
            Symbol: raw.Symbol,
            Multiplier: raw.Multiplier,
            PriceTick: raw.PriceTick,
            RoundTripFeePerLot: raw.RoundTripFeePerLot,
            LastPrice: raw.LastPrice,
            Volume: raw.Volume,
            OpenInterest: raw.OpenInterest,
            IsTradingAllowed: raw.IsTradingAllowed);
    }
}
