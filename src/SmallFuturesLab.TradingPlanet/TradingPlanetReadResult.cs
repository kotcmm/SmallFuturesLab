namespace SmallFuturesLab.TradingPlanet;

/// <summary>
/// 交易星球文件读取结果。
/// </summary>
public sealed record TradingPlanetReadResult
{
    /// <summary>成功读取的记录。</summary>
    public IReadOnlyList<TradingPlanetReadItem> Items { get; init; } = Array.Empty<TradingPlanetReadItem>();

    /// <summary>读取错误。</summary>
    public IReadOnlyList<TradingPlanetReadError> Errors { get; init; } = Array.Empty<TradingPlanetReadError>();

    /// <summary>是否没有错误。</summary>
    public bool IsSuccess => Errors.Count == 0;
}
