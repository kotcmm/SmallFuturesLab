namespace SmallFuturesLab.Risk;

/// <summary>
/// 账户快照，记录评估时刻的账户状态。
/// </summary>
public record AccountSnapshot
{
    /// <summary>
    /// 当前账户权益（E）。
    /// </summary>
    public double Equity { get; init; }

    /// <summary>
    /// 当前可用资金（A）。
    /// </summary>
    public double AvailableCash { get; init; }

    /// <summary>
    /// 今日已亏损金额（D）。
    /// </summary>
    public double DailyLossSoFar { get; init; }

    /// <summary>
    /// 今日已交易次数（N）。
    /// </summary>
    public int TradeCountToday { get; init; }
}
