namespace SmallFuturesLab.Core;

/// <summary>
/// 品种过滤状态。
/// </summary>
public enum ProductFilterStatus
{
    /// <summary>
    /// 当前账户规模下，一手合约压力可接受，可以进入后续研究。
    /// </summary>
    Allowed,

    /// <summary>
    /// 当前账户规模下进入谨慎区，只能观察或进一步验证。
    /// </summary>
    Caution,

    /// <summary>
    /// 当前账户规模下压力过高，排除。
    /// </summary>
    Rejected,
}
