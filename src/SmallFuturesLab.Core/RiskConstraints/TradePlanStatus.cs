namespace SmallFuturesLab.Core.RiskConstraints;

/// <summary>
/// 交易计划验算状态。
///
/// 这里只表达风险约束是否通过，不表达行情结构是否有效，也不表达实盘是否已经下单。
/// </summary>
public enum TradePlanStatus
{
    /// <summary>
    /// 风险约束通过，可以进入后续执行计划阶段。
    /// </summary>
    Accepted,

    /// <summary>
    /// 风险约束拒绝，不能新开仓。
    /// </summary>
    Rejected
}
