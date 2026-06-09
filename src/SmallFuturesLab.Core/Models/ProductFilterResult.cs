namespace SmallFuturesLab.Core.Models;

/// <summary>
/// 品种过滤结果。
/// </summary>
public record ProductFilterResult
{
    /// <summary>交易所名称或简称。</summary>
    public string Exchange { get; init; } = string.Empty;

    /// <summary>品种名称。</summary>
    public string ProductName { get; init; } = string.Empty;

    /// <summary>品种代码。</summary>
    public string ProductCode { get; init; } = string.Empty;

    /// <summary>合约代码。</summary>
    public string ContractCode { get; init; } = string.Empty;

    /// <summary>账户权益。</summary>
    public double AccountEquity { get; init; }

    /// <summary>一跳金额。</summary>
    public double TickValue { get; init; }

    /// <summary>一手保证金。</summary>
    public double MarginPerLot { get; init; }

    /// <summary>保证金占账户比例。</summary>
    public double MarginRateOfEquity { get; init; }

    /// <summary>止损风险金额。</summary>
    public double StopRiskMoney { get; init; }

    /// <summary>滑点金额。</summary>
    public double SlippageMoney { get; init; }

    /// <summary>手续费和滑点成本。</summary>
    public double CostMoney { get; init; }

    /// <summary>包含成本后的单笔总风险金额。</summary>
    public double TotalRiskMoney { get; init; }

    /// <summary>单笔总风险占账户比例。</summary>
    public double RiskRate { get; init; }

    /// <summary>成本占止损风险比例。</summary>
    public double CostRatio { get; init; }

    /// <summary>过滤结果。</summary>
    public ProductFilterStatus Status { get; init; }

    /// <summary>结果原因。</summary>
    public string Reasons { get; init; } = string.Empty;
}
