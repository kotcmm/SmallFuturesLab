namespace SmallFuturesLab.ProductFilter;

/// <summary>
/// CSV 读取结果。
/// </summary>
public record CsvReadResult
{
    /// <summary>
    /// 是否成功。
    /// </summary>
    public bool IsSuccess { get; init; }

    /// <summary>
    /// 读取到的数据行。
    /// </summary>
    public IReadOnlyList<ProductFilterRow> Rows { get; init; } = Array.Empty<ProductFilterRow>();

    /// <summary>
    /// 错误列表。
    /// </summary>
    public IReadOnlyList<ProductFilterValidationError> Errors { get; init; } = Array.Empty<ProductFilterValidationError>();
}
