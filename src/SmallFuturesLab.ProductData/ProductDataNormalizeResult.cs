using SmallFuturesLab.ProductFilter;

namespace SmallFuturesLab.ProductData;

/// <summary>
/// 品种数据标准化结果。
/// </summary>
public record ProductDataNormalizeResult
{
    /// <summary>
    /// 是否成功。
    /// </summary>
    public bool IsSuccess { get; init; }

    /// <summary>
    /// 标准化后的品种筛选行数据。
    /// </summary>
    public ProductFilterRow Row { get; init; } = new();

    /// <summary>
    /// 错误信息。
    /// </summary>
    public string Error { get; init; } = string.Empty;
}
