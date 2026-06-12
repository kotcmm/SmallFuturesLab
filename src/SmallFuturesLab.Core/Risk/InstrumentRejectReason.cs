namespace SmallFuturesLab.Core.Risk;

/// <summary>
/// 品种入选拒绝原因。
/// </summary>
public enum InstrumentRejectReason
{
    /// <summary>
    /// 未被拒绝。
    /// </summary>
    None = 0,

    /// <summary>
    /// 品种资料不具备最小可计算性。
    /// </summary>
    InvalidInstrument = 1,

    /// <summary>
    /// 当前不允许交易。
    /// </summary>
    TradingNotAllowed = 2,

    /// <summary>
    /// 成交量低于入选边界。
    /// </summary>
    VolumeTooLow = 3,

    /// <summary>
    /// 持仓量低于入选边界。
    /// </summary>
    OpenInterestTooLow = 4,

    /// <summary>
    /// 手续费高于入选边界。
    /// </summary>
    FeeTooHigh = 5,

    /// <summary>
    /// 最小跳动价值高于入选边界。
    /// </summary>
    TickValueTooLarge = 6,

    /// <summary>
    /// 最小交易风险颗粒度高于入选边界。
    /// </summary>
    MinimumTradeRiskTooLarge = 7
}
