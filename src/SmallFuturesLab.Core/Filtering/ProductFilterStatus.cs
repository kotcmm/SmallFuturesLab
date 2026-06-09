namespace SmallFuturesLab.Core.Filtering;

/// <summary>
/// 品种过滤状态。
/// </summary>
public enum ProductFilterStatus
{
    /// <summary>允许进入后续周期或方法研究。</summary>
    Allowed,

    /// <summary>谨慎观察。</summary>
    Caution,

    /// <summary>当前账户规模下排除。</summary>
    Rejected,
}
