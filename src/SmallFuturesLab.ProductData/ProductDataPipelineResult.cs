using SmallFuturesLab.ProductFilter;

namespace SmallFuturesLab.ProductData;

/// <summary>
/// 组合管线输出结果。
/// </summary>
public record ProductDataPipelineResult
{
    /// <summary>
    /// 合并后的完整品种数据记录。
    /// </summary>
    public IReadOnlyList<ProductDataRecord> MergedRecords { get; init; } = Array.Empty<ProductDataRecord>();

    /// <summary>
    /// 展开后的未计算品种筛选行。
    /// </summary>
    public IReadOnlyList<ProductFilterRow> Rows { get; init; } = Array.Empty<ProductFilterRow>();

    /// <summary>
    /// 管线错误列表。
    /// </summary>
    public IReadOnlyList<ProductDataPipelineError> Errors { get; init; } = Array.Empty<ProductDataPipelineError>();

    /// <summary>
    /// 是否成功（没有错误）。
    /// </summary>
    public bool IsSuccess => Errors.Count == 0;
}
