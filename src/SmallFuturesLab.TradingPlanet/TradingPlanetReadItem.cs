using SmallFuturesLab.Core.Products;

namespace SmallFuturesLab.TradingPlanet;

/// <summary>
/// 交易星球读取出的单条记录。
/// 来源备注和复核状态只保留在读取层，不进入 Core.ProductInfo。
/// </summary>
public sealed record TradingPlanetReadItem
{
    /// <summary>转换后的核心品种信息。</summary>
    public ProductInfo Product { get; init; } = new();

    /// <summary>原始备注。</summary>
    public string RawRemark { get; init; } = string.Empty;

    /// <summary>是否需要复核。交易星球属于第三方研究来源，默认需要复核。</summary>
    public bool NeedsReview { get; init; } = true;
}
