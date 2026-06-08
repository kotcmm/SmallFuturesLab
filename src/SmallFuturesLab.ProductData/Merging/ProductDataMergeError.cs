using SmallFuturesLab.ProductData.Models;
namespace SmallFuturesLab.ProductData.Merging;

/// <summary>
/// 品种数据合并错误，记录合并失败或冲突原因。
/// </summary>
public record ProductDataMergeError
{
    /// <summary>
    /// 品种代码。
    /// </summary>
    public string ProductCode { get; init; } = string.Empty;

    /// <summary>
    /// 合约代码。
    /// </summary>
    public string ContractCode { get; init; } = string.Empty;

    /// <summary>
    /// 冲突字段名称。
    /// </summary>
    public string FieldName { get; init; } = string.Empty;

    /// <summary>
    /// 失败原因。
    /// </summary>
    public string Reason { get; init; } = string.Empty;
}
