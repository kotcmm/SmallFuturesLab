namespace SmallFuturesLab.Risk;

/// <summary>
/// 交易许可评估结果。
/// </summary>
public record TradePermissionResult
{
    /// <summary>
    /// 许可状态：Allowed / Caution / Rejected。
    /// </summary>
    public TradePermissionStatus Status { get; init; }

    /// <summary>
    /// 核心风险指标。
    /// </summary>
    public RiskMetrics Metrics { get; init; } = new();

    /// <summary>
    /// 通过项列表，描述哪些检查已经通过。
    /// </summary>
    public IReadOnlyList<string> PassedItems { get; init; } = Array.Empty<string>();

    /// <summary>
    /// 警告项列表，描述哪些指标进入谨慎区间。
    /// </summary>
    public IReadOnlyList<string> WarningItems { get; init; } = Array.Empty<string>();

    /// <summary>
    /// 拒绝项列表，描述哪些指标触发了拒绝条件。
    /// </summary>
    public IReadOnlyList<string> RejectedItems { get; init; } = Array.Empty<string>();

    /// <summary>
    /// 结论描述。
    /// </summary>
    public string Conclusion { get; init; } = string.Empty;
}
