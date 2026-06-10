namespace SmallFuturesLab.Core;

/// <summary>
/// 表示一个用于过滤测算的期货合约参数。
/// 不包含数据来源信息，也不生成交易建议。
/// </summary>
public sealed record FuturesContract
{
    /// <summary>交易所名称或简称。</summary>
    public string Exchange { get; init; } = string.Empty;

    /// <summary>品种代码，例如 MA、RB、CU。</summary>
    public string ProductCode { get; init; } = string.Empty;

    /// <summary>合约代码，例如 MA2601、RB2510。</summary>
    public string ContractCode { get; init; } = string.Empty;

    /// <summary>品种名称，例如 甲醇。</summary>
    public string ProductName { get; init; } = string.Empty;

    /// <summary>当前或典型价格。必须大于 0。</summary>
    public double Price { get; init; }

    /// <summary>合约乘数。必须大于 0。</summary>
    public double Multiplier { get; init; }

    /// <summary>最小变动价位（tick 大小）。必须大于 0。</summary>
    public double TickSize { get; init; }

    /// <summary>保证金比例。必须大于 0。</summary>
    public double MarginRate { get; init; }

    /// <summary>单手开平总手续费。必须大于等于 0。</summary>
    public double RoundTripFee { get; init; }

    /// <summary>止损距离，单位为 tick。必须大于 0。</summary>
    public int StopTicks { get; init; }

    /// <summary>预估总滑点，单位为 tick。必须大于等于 0。</summary>
    public int SlippageTicks { get; init; }

    /// <summary>测算手数。默认 1 手。必须大于 0。</summary>
    public int Lots { get; init; } = 1;

    /// <summary>
    /// 一跳金额。
    /// TickValue = TickSize * Multiplier。
    /// </summary>
    public double TickValue => TickSize * Multiplier;

    /// <summary>
    /// 一手保证金金额。
    /// MarginPerLot = Price * Multiplier * MarginRate。
    /// </summary>
    public double MarginPerLot => Price * Multiplier * MarginRate;

    /// <summary>
    /// 保证金总金额。
    /// MarginMoney = MarginPerLot * Lots。
    /// </summary>
    public double MarginMoney => MarginPerLot * Lots;

    /// <summary>
    /// 止损风险金额。
    /// StopRiskMoney = StopTicks * TickValue * Lots。
    /// </summary>
    public double StopRiskMoney => StopTicks * TickValue * Lots;

    /// <summary>
    /// 滑点金额。
    /// SlippageMoney = SlippageTicks * TickValue * Lots。
    /// </summary>
    public double SlippageMoney => SlippageTicks * TickValue * Lots;

    /// <summary>
    /// 成本金额，包含手续费和滑点。
    /// CostMoney = RoundTripFee * Lots + SlippageMoney。
    /// </summary>
    public double CostMoney => RoundTripFee * Lots + SlippageMoney;

    /// <summary>
    /// 总风险金额，包含止损、手续费和滑点。
    /// TotalRiskMoney = StopRiskMoney + CostMoney。
    /// </summary>
    public double TotalRiskMoney => StopRiskMoney + CostMoney;

    /// <summary>
    /// 成本占止损风险比例。
    /// CostRatio = CostMoney / StopRiskMoney。
    /// 当 StopRiskMoney 为 0 时返回正无穷。
    /// </summary>
    public double CostRatio => StopRiskMoney > 0 ? CostMoney / StopRiskMoney : double.PositiveInfinity;
}
