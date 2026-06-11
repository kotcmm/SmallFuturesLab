namespace SmallFuturesLab.Core.RiskConstraints;

/// <summary>
/// 交易结构输入。
///
/// TradeSetup 来自行情结构模块。它只描述“如果要做这笔交易，在哪里进场，在哪里证明错了”。
/// 目标价、手数、TradeR 和是否允许交易，都由风险约束模块推导。
/// </summary>
/// <param name="Symbol">品种代码。</param>
/// <param name="Direction">交易方向。</param>
/// <param name="EntryPrice">计划入场价。</param>
/// <param name="StopPrice">计划止损价。</param>
/// <param name="Multiplier">合约乘数。</param>
/// <param name="EstimatedRoundTripCostPerLot">预估单手开平总成本，包含手续费、滑点、价差等。</param>
/// <param name="OneLotMargin">单手保证金。</param>
public sealed record TradeSetup(
    string Symbol,
    TradeDirection Direction,
    double EntryPrice,
    double StopPrice,
    double Multiplier,
    double EstimatedRoundTripCostPerLot,
    double OneLotMargin)
{
    /// <summary>
    /// SetupPriceRisk = |EntryPrice - StopPrice|。
    ///
    /// 含义：入场价到止损价之间的价格距离。
    /// </summary>
    public double SetupPriceRisk => Math.Abs(EntryPrice - StopPrice);

    /// <summary>
    /// OneLotPriceRisk = SetupPriceRisk × Multiplier。
    ///
    /// 含义：一手合约从入场价亏到止损价时，对应的价格风险金额，不含成本。
    /// </summary>
    public double OneLotPriceRisk => SetupPriceRisk * Multiplier;

    /// <summary>
    /// OneLotTradeR = OneLotPriceRisk + EstimatedRoundTripCostPerLot。
    ///
    /// 含义：一手计划风险，包含价格风险和预估交易成本。
    /// </summary>
    public double OneLotTradeR => OneLotPriceRisk + EstimatedRoundTripCostPerLot;
}
