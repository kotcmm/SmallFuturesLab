namespace SmallFuturesLab.ProductFilter;

/// <summary>
/// 品种筛选校验错误，描述某行某字段的具体问题。
/// </summary>
public record ProductFilterValidationError
{
    /// <summary>
    /// 行号（1-based）。
    /// </summary>
    public int RowNumber { get; init; }

    /// <summary>
    /// 字段名。
    /// </summary>
    public string FieldName { get; init; } = string.Empty;

    /// <summary>
    /// 错误原因。
    /// </summary>
    public string Reason { get; init; } = string.Empty;
}
