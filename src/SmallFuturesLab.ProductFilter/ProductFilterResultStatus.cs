namespace SmallFuturesLab.ProductFilter;

/// <summary>
/// 品种筛选结果状态。
/// </summary>
public enum ProductFilterResultStatus
{
    /// <summary>
    /// 允许进入后续周期研究。
    /// </summary>
    Allowed,

    /// <summary>
    /// 谨慎，只允许继续观察或模拟测算。
    /// </summary>
    Caution,

    /// <summary>
    /// 排除，当前账户规模不研究。
    /// </summary>
    Rejected,
}
