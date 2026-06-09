namespace SmallFuturesLab.Core.Products;

/// <summary>
/// 用于小资金品种过滤的期货合约信息。
/// </summary>
public sealed record ProductInfo
{
    /// <summary>品种和合约身份。</summary>
    public ProductIdentity Identity { get; init; } = new();

    /// <summary>一手合约经济参数。</summary>
    public PerLotEconomics Economics { get; init; } = new();
}
