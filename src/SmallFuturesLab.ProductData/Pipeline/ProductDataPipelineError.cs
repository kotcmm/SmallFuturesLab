namespace SmallFuturesLab.ProductData.Pipeline;

/// <summary>
/// 组合管线中的错误记录。
/// </summary>
public record ProductDataPipelineError
{
    /// <summary>
    /// 错误发生的阶段。推荐取值：Read、Merge、Expand、Export。
    /// </summary>
    public string Stage { get; init; } = string.Empty;

    /// <summary>
    /// 数据源输入名称。
    /// </summary>
    public string SourceName { get; init; } = string.Empty;

    /// <summary>
    /// 品种代码。
    /// </summary>
    public string ProductCode { get; init; } = string.Empty;

    /// <summary>
    /// 合约代码。
    /// </summary>
    public string ContractCode { get; init; } = string.Empty;

    /// <summary>
    /// 字段名或场景名。
    /// </summary>
    public string FieldName { get; init; } = string.Empty;

    /// <summary>
    /// 错误原因。
    /// </summary>
    public string Reason { get; init; } = string.Empty;
}
