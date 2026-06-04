namespace SmallFuturesLab.ProductFilter;

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
