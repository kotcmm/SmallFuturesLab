namespace SmallFuturesLab.Core.RiskConstraints;

public sealed record TradePlan(
    string Symbol,
    TradePlanStatus Status,
    RiskRejectReason RejectReason,
    TradeDirection Direction,
    double AccountR,
    double SetupPriceRisk,
    double OneLotPriceRisk,
    double OneLotTradeR,
    int AllowedLots,
    double TradeR,
    double CostInR,
    double RequiredRewardAmount,
    double TargetPriceDistance,
    double TargetPrice,
    double MaxAllowedMargin,
    double MarginAfterOpen);
