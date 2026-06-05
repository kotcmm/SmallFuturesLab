namespace SmallFuturesLab.ProductData;

/// <summary>
/// 品种数据合并结果，包含成功合并的记录和合并错误。
/// </summary>
public record ProductDataMergeResult
{
    /// <summary>
    /// 成功合并的品种数据记录列表。
    /// </summary>
    public IReadOnlyList<ProductDataRecord> Records { get; init; } = Array.Empty<ProductDataRecord>();

    /// <summary>
    /// 合并错误列表。
    /// </summary>
    public IReadOnlyList<ProductDataMergeError> Errors { get; init; } = Array.Empty<ProductDataMergeError>();

    /// <summary>
    /// 是否全部合并成功，没有错误。
    /// </summary>
    public bool IsSuccess => Errors.Count == 0;
}
