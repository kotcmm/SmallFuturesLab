namespace SmallFuturesLab.Risk;

/// <summary>
/// 交易许可状态。
/// </summary>
public enum TradePermissionStatus
{
    /// <summary>
    /// 允许继续研究或模拟。
    /// </summary>
    Allowed,

    /// <summary>
    /// 谨慎，只允许继续观察或模拟。
    /// </summary>
    Caution,

    /// <summary>
    /// 拒绝，行情再好也不做。
    /// </summary>
    Rejected,
}
