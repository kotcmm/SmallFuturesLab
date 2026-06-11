namespace SmallFuturesLab.Core.Risk;

/// <summary>
/// 风险约束拒绝原因。
///
/// 拒绝原因用于把“为什么不能交易”明确返回给调用方，避免只返回 true/false。
/// </summary>
public enum RiskRejectReason
{
    /// <summary>
    /// 未被拒绝。
    /// </summary>
    None,

    /// <summary>
    /// TradeSetup 本身无效，例如价格无效、入场价等于止损价、合约乘数无效。
    /// </summary>
    InvalidTradeSetup,

    /// <summary>
    /// 当日已实现亏损达到每日亏损上限。
    /// </summary>
    DailyLossLimitReached,

    /// <summary>
    /// 当日已实现盈利达到每日盈利保护线。
    /// </summary>
    DailyProfitLockReached,

    /// <summary>
    /// 当日交易次数达到上限。
    /// </summary>
    MaxDailyTradesReached,

    /// <summary>
    /// 连续亏损次数达到暂停新开仓条件。
    /// </summary>
    ConsecutiveLossLimitReached,

    /// <summary>
    /// 账户单笔风险上限 AccountR 不足以覆盖一手计划风险。
    /// </summary>
    NotEnoughAccountR,

    /// <summary>
    /// 单笔成本占 TradeR 的比例超过 PerTradeCostMaxR。
    /// </summary>
    CostTooHigh,

    /// <summary>
    /// 新开仓后保证金占用超过 MaxMarginUsageRatio 对应的上限。
    /// </summary>
    MarginUsageExceeded
}
