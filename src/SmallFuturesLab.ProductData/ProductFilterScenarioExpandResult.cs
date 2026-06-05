using SmallFuturesLab.ProductFilter;

namespace SmallFuturesLab.ProductData;

/// <summary>
/// 品种测算场景展开结果，包含成功展开的行和失败的错误。
/// </summary>
public record ProductFilterScenarioExpandResult
{
    /// <summary>
    /// 成功展开的品种筛选行列表。
    /// </summary>
    public IReadOnlyList<ProductFilterRow> Rows { get; init; } = Array.Empty<ProductFilterRow>();

    /// <summary>
    /// 展开失败的错误列表。
    /// </summary>
    public IReadOnlyList<ProductFilterScenarioExpandError> Errors { get; init; } = Array.Empty<ProductFilterScenarioExpandError>();
}
