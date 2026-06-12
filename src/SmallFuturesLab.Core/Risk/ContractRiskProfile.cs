namespace SmallFuturesLab.Core.Risk;

/// <summary>
/// 合约风险计算资料。
///
/// ContractRiskProfile 不是行情结构，也不是交易计划。
/// 它只提供风险模块计算单手风险、交易前预估成本和保证金占用所需的合约资料。
/// </summary>
public sealed record ContractRiskProfile
{
    /// <summary>
    /// 创建合约风险计算资料。
    /// </summary>
    /// <param name="symbol">品种代码。</param>
    /// <param name="multiplier">合约乘数。</param>
    /// <param name="estimatedRoundTripCostPerLot">交易前预估单手开平总成本。</param>
    /// <param name="oneLotMargin">单手保证金。</param>
    public ContractRiskProfile(
        string symbol,
        double multiplier,
        double estimatedRoundTripCostPerLot,
        double oneLotMargin)
    {
        Symbol = symbol;
        Multiplier = multiplier;
        EstimatedRoundTripCostPerLot = estimatedRoundTripCostPerLot;
        OneLotMargin = oneLotMargin;
    }

    /// <summary>
    /// 品种代码。
    /// </summary>
    public string Symbol { get; init; }

    /// <summary>
    /// 合约乘数。
    /// </summary>
    public double Multiplier { get; init; }

    /// <summary>
    /// 交易前预估单手开平总成本。
    /// </summary>
    public double EstimatedRoundTripCostPerLot { get; init; }

    /// <summary>
    /// 单手保证金。
    /// </summary>
    public double OneLotMargin { get; init; }
}
