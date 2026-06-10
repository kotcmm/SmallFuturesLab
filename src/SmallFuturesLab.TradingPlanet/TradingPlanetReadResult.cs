using SmallFuturesLab.Core;

namespace SmallFuturesLab.TradingPlanet;

/// <summary>
/// 交易星球文件读取结果。
/// </summary>
public sealed record TradingPlanetReadResult
{
    /// <summary>成功读取的合约列表。</summary>
    public IReadOnlyList<FuturesContract> Contracts { get; init; } = Array.Empty<FuturesContract>();

    /// <summary>读取错误。</summary>
    public IReadOnlyList<TradingPlanetReadError> Errors { get; init; } = Array.Empty<TradingPlanetReadError>();

    /// <summary>是否没有错误。</summary>
    public bool IsSuccess => Errors.Count == 0;
}
