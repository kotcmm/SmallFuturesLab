namespace SmallFuturesLab.Core.Risk;

/// <summary>
/// 交易结构输入。
///
/// TradeSetup 来自行情结构模块。
/// 它只描述“如果要做这笔交易，在哪里进场，在哪里证明错了”。
///
/// TradeSetup 不是交易计划。
/// 它不包含手数、目标价、TradeR、保证金占用，也不决定是否允许交易。
/// </summary>
public sealed record TradeSetup
{
    /// <summary>
    /// 创建交易结构输入。
    /// </summary>
    /// <param name="symbol">品种代码。</param>
    /// <param name="direction">交易方向。</param>
    /// <param name="entryPrice">计划入场价。</param>
    /// <param name="stopPrice">计划止损价。</param>
    public TradeSetup(
        string symbol,
        TradeDirection direction,
        double entryPrice,
        double stopPrice)
    {
        Symbol = symbol;
        Direction = direction;
        EntryPrice = entryPrice;
        StopPrice = stopPrice;
    }

    /// <summary>
    /// 品种代码。
    /// </summary>
    public string Symbol { get; init; }

    /// <summary>
    /// 交易方向。
    /// </summary>
    public TradeDirection Direction { get; init; }

    /// <summary>
    /// 计划入场价。
    /// </summary>
    public double EntryPrice { get; init; }

    /// <summary>
    /// 计划止损价。
    /// </summary>
    public double StopPrice { get; init; }
}
