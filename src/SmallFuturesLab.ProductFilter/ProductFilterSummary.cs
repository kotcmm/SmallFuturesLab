namespace SmallFuturesLab.ProductFilter;

/// <summary>
/// 按账户规模分组的品种筛选汇总统计。
/// </summary>
public record ProductFilterSummary
{
    /// <summary>
    /// 总记录数。
    /// </summary>
    public int TotalRecords { get; init; }

    /// <summary>
    /// 涉及品种数。
    /// </summary>
    public int UniqueProducts { get; init; }

    /// <summary>
    /// 按账户规模分组的统计。
    /// Key 为账户权益数值，Value 为该账户规模下的统计。
    /// </summary>
    public IReadOnlyDictionary<double, AccountEquitySummary> ByAccountEquity { get; init; } = new Dictionary<double, AccountEquitySummary>();

    /// <summary>
    /// 需要复核的数据。
    /// </summary>
    public IReadOnlyList<string> NeedsReview { get; init; } = Array.Empty<string>();

    /// <summary>
    /// 主要排除原因统计。
    /// </summary>
    public IReadOnlyDictionary<string, int> RejectionReasonStats { get; init; } = new Dictionary<string, int>();
}
