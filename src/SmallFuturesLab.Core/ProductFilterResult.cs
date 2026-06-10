namespace SmallFuturesLab.Core;

/// <summary>
/// 单个品种的过滤输出结果。
/// </summary>
public sealed record ProductFilterResult
{
    /// <summary>品种代码。</summary>
    public string Code { get; init; } = string.Empty;

    /// <summary>合约代码。</summary>
    public string Contract { get; init; } = string.Empty;

    /// <summary>总风险占账户权益比例。</summary>
    public double RiskRate { get; init; }

    /// <summary>保证金占账户权益比例。</summary>
    public double MarginRate { get; init; }

    /// <summary>成本占止损风险比例。</summary>
    public double CostRatio { get; init; }

    /// <summary>总风险金额。</summary>
    public double TotalRiskMoney { get; init; }

    /// <summary>保证金总金额。</summary>
    public double MarginMoney { get; init; }

    /// <summary>过滤状态。</summary>
    public ProductFilterStatus Status { get; init; }

    /// <summary>状态原因列表。</summary>
    public IReadOnlyList<string> Reasons { get; init; } = Array.Empty<string>();
}
