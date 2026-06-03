namespace SmallFuturesLab.ProductFilter;

/// <summary>
/// 品种筛选校验结果。
/// </summary>
public record ProductFilterValidationResult
{
    /// <summary>
    /// 是否通过校验。
    /// </summary>
    public bool IsValid { get; init; }

    /// <summary>
    /// 错误列表。
    /// </summary>
    public IReadOnlyList<ProductFilterValidationError> Errors { get; init; } = Array.Empty<ProductFilterValidationError>();
}
