namespace SmallFuturesLab.Core.RiskConstraints;

public enum RiskRejectReason
{
    None,
    InvalidTradeSetup,
    DailyLossLimitReached,
    DailyProfitLockReached,
    MaxDailyTradesReached,
    ConsecutiveLossLimitReached,
    NotEnoughAccountR,
    CostTooHigh,
    MarginUsageExceeded
}
