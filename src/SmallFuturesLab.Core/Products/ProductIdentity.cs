namespace SmallFuturesLab.Core.Products;

/// <summary>
/// 期货品种和合约身份。
/// </summary>
public sealed record ProductIdentity
{
    /// <summary>交易所。</summary>
    public string Exchange { get; init; } = string.Empty;

    /// <summary>品种代码。</summary>
    public string ProductCode { get; init; } = string.Empty;

    /// <summary>合约代码。</summary>
    public string ContractCode { get; init; } = string.Empty;

    /// <summary>品种名称。</summary>
    public string ProductName { get; init; } = string.Empty;
}
