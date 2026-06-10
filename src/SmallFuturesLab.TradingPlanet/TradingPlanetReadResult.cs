using SmallFuturesLab.Core;

namespace SmallFuturesLab.TradingPlanet;

/// <summary>
/// 交易星球文件读取结果。
/// </summary>
public sealed record TradingPlanetReadResult
{
    /// <summary>成功读取的品种列表。</summary>
    public IReadOnlyList<ProductInfo> Products { get; init; } = Array.Empty<ProductInfo>();

    /// <summary>读取错误。</summary>
    public IReadOnlyList<TradingPlanetReadError> Errors { get; init; } = Array.Empty<TradingPlanetReadError>();

    /// <summary>是否没有错误。</summary>
    public bool IsSuccess => Errors.Count == 0;
}
