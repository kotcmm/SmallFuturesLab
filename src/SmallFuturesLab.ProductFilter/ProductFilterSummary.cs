namespace SmallFuturesLab.ProductFilter;

/// <summary>
/// 品种筛选汇总统计。
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
    /// 10,000 元账户 Allowed 数量。
    /// </summary>
    public int AllowedCount10k { get; init; }

    /// <summary>
    /// 10,000 元账户 Caution 数量。
    /// </summary>
    public int CautionCount10k { get; init; }

    /// <summary>
    /// 10,000 元账户 Rejected 数量。
    /// </summary>
    public int RejectedCount10k { get; init; }

    /// <summary>
    /// 20,000 元账户 Allowed 数量。
    /// </summary>
    public int AllowedCount20k { get; init; }

    /// <summary>
    /// 20,000 元账户 Caution 数量。
    /// </summary>
    public int CautionCount20k { get; init; }

    /// <summary>
    /// 20,000 元账户 Rejected 数量。
    /// </summary>
    public int RejectedCount20k { get; init; }

    /// <summary>
    /// 10,000 元账户候选列表（Allowed）。
    /// </summary>
    public IReadOnlyList<string> Candidates10k { get; init; } = Array.Empty<string>();

    /// <summary>
    /// 20,000 元账户候选列表（Allowed）。
    /// </summary>
    public IReadOnlyList<string> Candidates20k { get; init; } = Array.Empty<string>();

    /// <summary>
    /// 谨慎观察列表（Caution）。
    /// </summary>
    public IReadOnlyList<string> CautionList { get; init; } = Array.Empty<string>();

    /// <summary>
    /// 排除列表（Rejected）。
    /// </summary>
    public IReadOnlyList<string> ExcludedList { get; init; } = Array.Empty<string>();

    /// <summary>
    /// 需要复核的数据。
    /// </summary>
    public IReadOnlyList<string> NeedsReview { get; init; } = Array.Empty<string>();

    /// <summary>
    /// 主要排除原因统计。
    /// </summary>
    public IReadOnlyDictionary<string, int> RejectionReasonStats { get; init; } = new Dictionary<string, int>();
}
