namespace SmallFuturesLab.ProductFilter;

/// <summary>
/// 品种筛选单行计算结果，包含计算后的行数据以及当前账户规模的结论。
/// </summary>
public record ProductFilterCalculationResult
{
    /// <summary>
    /// 计算后的品种筛选行数据。
    /// </summary>
    public ProductFilterRow Row { get; init; } = new();

    /// <summary>
    /// 当前账户规模下的结论。
    /// </summary>
    public ProductFilterResultStatus Result { get; init; }

    /// <summary>
    /// 结论原因。
    /// </summary>
    public string Reasons { get; init; } = string.Empty;

    /// <summary>
    /// 构造函数。
    /// </summary>
    public ProductFilterCalculationResult() { }

    /// <summary>
    /// 构造函数。
    /// </summary>
    public ProductFilterCalculationResult(ProductFilterRow row, ProductFilterResultStatus result, string reasons)
    {
        Row = row;
        Result = result;
        Reasons = reasons;
    }
}
