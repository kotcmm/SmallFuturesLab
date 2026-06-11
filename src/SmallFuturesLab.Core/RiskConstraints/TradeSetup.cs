namespace SmallFuturesLab.Core.RiskConstraints;

public sealed record TradeSetup(
    string Symbol,
    TradeDirection Direction,
    double EntryPrice,
    double StopPrice,
    double Multiplier,
    double EstimatedRoundTripCostPerLot,
    double OneLotMargin)
{
    public double SetupPriceRisk => Math.Abs(EntryPrice - StopPrice);

    public double OneLotPriceRisk => SetupPriceRisk * Multiplier;

    public double OneLotTradeR => OneLotPriceRisk + EstimatedRoundTripCostPerLot;
}
