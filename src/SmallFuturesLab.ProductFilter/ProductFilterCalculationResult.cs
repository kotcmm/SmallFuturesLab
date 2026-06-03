namespace SmallFuturesLab.ProductFilter;

/// <summary>
/// 品种筛选单行计算结果，包含计算后的行数据以及两个账户规模的结论。
/// </summary>
public record ProductFilterCalculationResult
{
    /// <summary>
    /// 计算后的品种筛选行数据。
    /// </summary>
    public ProductFilterRow Row { get; init; } = new();

    /// <summary>
    /// 10,000 元账户结论。
    /// </summary>
    public ProductFilterResultStatus Result10k { get; init; }

    /// <summary>
    /// 20,000 元账户结论。
    /// </summary>
    public ProductFilterResultStatus Result20k { get; init; }

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
    public ProductFilterCalculationResult(ProductFilterRow row, ProductFilterResultStatus result10k, ProductFilterResultStatus result20k, string reasons)
    {
        Row = row;
        Result10k = result10k;
        Result20k = result20k;
        Reasons = reasons;
    }
}
