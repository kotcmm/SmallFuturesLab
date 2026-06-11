namespace SmallFuturesLab.Core.Risk;

/// <summary>
/// 交易方向。
///
/// 风险模块不判断方向是否有行情优势，只根据方向决定目标价是在入场价上方还是下方。
/// </summary>
public enum TradeDirection
{
    /// <summary>
    /// 做多：目标价 = 入场价 + 目标价格距离。
    /// </summary>
    Long = 1,

    /// <summary>
    /// 做空：目标价 = 入场价 - 目标价格距离。
    /// </summary>
    Short = -1
}
