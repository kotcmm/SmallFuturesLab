namespace SmallFuturesLab.Risk;

/// <summary>
/// 交易设想，用于风险测算的候选交易方案。
/// 不是策略信号，仅用于评估该交易是否被账户允许。
/// </summary>
public record TradeIdea
{
    /// <summary>
    /// 假设入场价（Pe）。
    /// </summary>
    public double EntryPrice { get; init; }

    /// <summary>
    /// 假设止损价（Ps）。
    /// </summary>
    public double StopPrice { get; init; }

    /// <summary>
    /// 手数（L）。当前阶段固定为 1 手。
    /// </summary>
    public int Lots { get; init; }

    /// <summary>
    /// 预估总滑点跳数（S）。
    /// </summary>
    public int SlippageTicks { get; init; }

    /// <summary>
    /// 是否隔夜持仓。
    /// </summary>
    public bool IsOvernight { get; init; }

    /// <summary>
    /// 是否加仓。
    /// </summary>
    public bool IsAddPosition { get; init; }

    /// <summary>
    /// 是否有明确止损。
    /// </summary>
    public bool HasStop { get; init; }
}
