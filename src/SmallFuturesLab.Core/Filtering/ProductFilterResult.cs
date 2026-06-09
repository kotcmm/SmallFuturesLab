namespace SmallFuturesLab.Core.Filtering;

/// <summary>
/// 一批品种过滤后的整体结果。
/// </summary>
public sealed record ProductFilterResult
{
    /// <summary>每个品种和测算场景对应的过滤决定。</summary>
    public IReadOnlyList<ProductFilterDecision> Decisions { get; init; } = Array.Empty<ProductFilterDecision>();
}
