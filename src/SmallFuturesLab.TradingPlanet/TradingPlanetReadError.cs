namespace SmallFuturesLab.TradingPlanet;

/// <summary>
/// 交易星球文件读取错误。
/// </summary>
public sealed record TradingPlanetReadError
{
    /// <summary>行号。</summary>
    public int RowNumber { get; init; }

    /// <summary>字段名。</summary>
    public string FieldName { get; init; } = string.Empty;

    /// <summary>错误原因。</summary>
    public string Reason { get; init; } = string.Empty;
}
