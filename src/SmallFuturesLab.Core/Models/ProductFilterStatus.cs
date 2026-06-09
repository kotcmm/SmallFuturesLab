namespace SmallFuturesLab.Core.Models;

/// <summary>
/// 品种过滤结果状态。
/// </summary>
public enum ProductFilterStatus
{
    /// <summary>可以进入后续研究。</summary>
    Allowed,

    /// <summary>谨慎观察，不是优先候选。</summary>
    Caution,

    /// <summary>当前账户规模下排除。</summary>
    Rejected,
}
