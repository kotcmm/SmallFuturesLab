namespace SmallFuturesLab.Core.Selection;

/// <summary>
/// 收集器拿到的原始合约资料。
/// </summary>
public sealed record InstrumentRawInfo
{
    /// <summary>
    /// 创建原始合约资料。
    /// </summary>
    /// <param name="Symbol">合约代码。</param>
    /// <param name="Multiplier">合约乘数。</param>
    /// <param name="PriceTick">最小价格变动单位。</param>
    /// <param name="RoundTripFeePerLot">一手往返手续费预估。</param>
    /// <param name="LastPrice">最新价格。</param>
    /// <param name="Volume">当前统计周期成交量。</param>
    /// <param name="OpenInterest">当前持仓量。</param>
    /// <param name="IsTradingAllowed">当前是否允许交易。</param>
    public InstrumentRawInfo(
        string Symbol,
        double Multiplier,
        double PriceTick,
        double RoundTripFeePerLot,
        double LastPrice,
        long Volume,
        long OpenInterest,
        bool IsTradingAllowed)
    {
        this.Symbol = Symbol;
        this.Multiplier = Multiplier;
        this.PriceTick = PriceTick;
        this.RoundTripFeePerLot = RoundTripFeePerLot;
        this.LastPrice = LastPrice;
        this.Volume = Volume;
        this.OpenInterest = OpenInterest;
        this.IsTradingAllowed = IsTradingAllowed;
    }

    /// <summary>
    /// 合约代码。
    /// </summary>
    public string Symbol { get; }

    /// <summary>
    /// 合约乘数。
    /// </summary>
    public double Multiplier { get; }

    /// <summary>
    /// 最小价格变动单位。
    /// </summary>
    public double PriceTick { get; }

    /// <summary>
    /// 一手往返手续费预估。
    /// </summary>
    public double RoundTripFeePerLot { get; }

    /// <summary>
    /// 最新价格。
    /// </summary>
    public double LastPrice { get; }

    /// <summary>
    /// 当前统计周期成交量。
    /// </summary>
    public long Volume { get; }

    /// <summary>
    /// 当前持仓量。
    /// </summary>
    public long OpenInterest { get; }

    /// <summary>
    /// 当前是否允许交易。
    /// </summary>
    public bool IsTradingAllowed { get; }
}
