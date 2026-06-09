using SmallFuturesLab.Core.Accounts;
using SmallFuturesLab.Core.Products;

namespace SmallFuturesLab.Core.Filtering;

/// <summary>
/// 单个品种在单个账户和测算场景下的过滤决定。
/// </summary>
public sealed record ProductFilterDecision
{
    /// <summary>品种信息。</summary>
    public ProductInfo Product { get; init; } = new();

    /// <summary>账户资料。</summary>
    public AccountProfile Account { get; init; } = new();

    /// <summary>测算场景。</summary>
    public FilterScenario Scenario { get; init; } = new();

    /// <summary>一手保证金金额。</summary>
    public double MarginPerLot { get; init; }

    /// <summary>保证金占账户权益比例。</summary>
    public double MarginRateOfEquity { get; init; }

    /// <summary>一跳金额。</summary>
    public double TickValue { get; init; }

    /// <summary>止损风险金额。</summary>
    public double StopRiskMoney { get; init; }

    /// <summary>滑点金额。</summary>
    public double SlippageMoney { get; init; }

    /// <summary>成本金额，包含手续费和滑点。</summary>
    public double CostMoney { get; init; }

    /// <summary>总风险金额，包含止损、手续费和滑点。</summary>
    public double TotalRiskMoney { get; init; }

    /// <summary>总风险占账户权益比例。</summary>
    public double RiskRate { get; init; }

    /// <summary>成本占止损风险比例。</summary>
    public double CostRatio { get; init; }

    /// <summary>过滤状态。</summary>
    public ProductFilterStatus Status { get; init; }

    /// <summary>原因列表。</summary>
    public IReadOnlyList<string> Reasons { get; init; } = Array.Empty<string>();
}
