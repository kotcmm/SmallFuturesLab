namespace SmallFuturesLab.ProductData;

/// <summary>
/// 品种数据源读取结果，包含成功读取的记录和解析错误列表。
/// </summary>
public record ProductDataReadResult
{
    /// <summary>
    /// 是否成功（没有解析错误）。
    /// </summary>
    public bool IsSuccess => Errors.Count == 0;

    /// <summary>
    /// 成功解析的品种数据记录。
    /// 存在错误的行不会加入此列表。
    /// </summary>
    public IReadOnlyList<ProductDataRecord> Records { get; init; } = Array.Empty<ProductDataRecord>();

    /// <summary>
    /// 解析错误列表。
    /// </summary>
    public IReadOnlyList<ProductDataReadError> Errors { get; init; } = Array.Empty<ProductDataReadError>();
}
