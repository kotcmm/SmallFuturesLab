namespace SmallFuturesLab.ProductData.Scenarios;

/// <summary>
/// 品种测算场景展开错误，记录单个场景展开失败的原因。
/// </summary>
public record ProductFilterScenarioExpandError
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
    /// 场景名称。
    /// </summary>
    public string ScenarioName { get; init; } = string.Empty;

    /// <summary>
    /// 失败原因。
    /// </summary>
    public string Reason { get; init; } = string.Empty;
}
