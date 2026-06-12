namespace SmallFuturesLab.Core.Selection;

/// <summary>
/// 把外部收集到的合约资料转换成品种过滤使用的统一对象。
/// </summary>
public sealed class InstrumentFilterProfileMapper
{
    /// <summary>
    /// 转换单个合约原始资料。
    /// </summary>
    /// <param name="raw">收集器返回的原始合约资料。</param>
    /// <returns>品种过滤使用的统一内部对象。</returns>
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
