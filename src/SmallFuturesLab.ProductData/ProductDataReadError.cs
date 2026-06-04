namespace SmallFuturesLab.ProductData;

/// <summary>
/// 品种数据源读取错误，记录某行某字段的解析失败信息。
/// </summary>
public record ProductDataReadError
{
    /// <summary>
    /// 错误所在行号（从 1 开始计数）。
    /// </summary>
    public int RowNumber { get; init; }

    /// <summary>
    /// 发生错误的字段名。
    /// </summary>
    public string FieldName { get; init; } = string.Empty;

    /// <summary>
    /// 错误原因。
    /// </summary>
    public string Reason { get; init; } = string.Empty;
}
