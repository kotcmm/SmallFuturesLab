namespace SmallFuturesLab.Core.Risk;

/// <summary>
/// 品种入选拒绝原因。
/// </summary>
public enum InstrumentRejectReason
{
    None = 0,
    InvalidInstrument = 1,
    TradingNotAllowed = 2,
    VolumeTooLow = 3,
    OpenInterestTooLow = 4,
    FeeTooHigh = 5,
    TickValueTooLarge = 6,
    MinimumTradeRiskTooLarge = 7
}
