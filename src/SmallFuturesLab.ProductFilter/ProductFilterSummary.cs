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

/// <summary>
/// 单个账户规模下的统计。
/// </summary>
public record AccountEquitySummary
{
    /// <summary>
    /// Allowed 数量。
    /// </summary>
    public int AllowedCount { get; init; }

    /// <summary>
    /// Caution 数量。
    /// </summary>
    public int CautionCount { get; init; }

    /// <summary>
    /// Rejected 数量。
    /// </summary>
    public int RejectedCount { get; init; }

    /// <summary>
    /// 进入后续周期研究列表（Allowed）。
    /// </summary>
    public IReadOnlyList<string> Candidates { get; init; } = Array.Empty<string>();

    /// <summary>
    /// 谨慎观察列表（Caution）。
    /// </summary>
    public IReadOnlyList<string> CautionList { get; init; } = Array.Empty<string>();

    /// <summary>
    /// 当前账户规模排除列表（Rejected）。
    /// </summary>
    public IReadOnlyList<string> ExcludedList { get; init; } = Array.Empty<string>();
}
