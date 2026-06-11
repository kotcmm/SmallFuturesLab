namespace SmallFuturesLab.Core.RiskConstraints;

/// <summary>
/// 当日风险状态。
///
/// 这个对象描述“今天已经发生了什么”，用于判断是否还能继续新开仓。
/// 它不负责记录成交明细，只提供风险约束验算需要的聚合状态。
/// </summary>
public sealed record DailyRiskState
{
    /// <summary>
    /// 创建当日风险状态。
    /// </summary>
    /// <param name="realizedPnlToday">当日已实现盈亏；盈利为正，亏损为负。</param>
    /// <param name="dailyTradeCount">当日已经执行的交易次数。</param>
    /// <param name="consecutiveLosses">当前连续亏损次数。</param>
    /// <param name="currentMarginUsed">当前已经占用的保证金金额。</param>
    public DailyRiskState(
        double realizedPnlToday,
        int dailyTradeCount,
        int consecutiveLosses,
        double currentMarginUsed)
    {
        if (dailyTradeCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(dailyTradeCount), "当日交易次数不能小于 0。");
        }

        if (consecutiveLosses < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(consecutiveLosses), "连续亏损次数不能小于 0。");
        }

        if (currentMarginUsed < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(currentMarginUsed), "当前保证金占用不能小于 0。");
        }

        RealizedPnlToday = realizedPnlToday;
        DailyTradeCount = dailyTradeCount;
        ConsecutiveLosses = consecutiveLosses;
        CurrentMarginUsed = currentMarginUsed;
    }

    /// <summary>
    /// 当日已实现盈亏；盈利为正，亏损为负。
    /// </summary>
    public double RealizedPnlToday { get; }

    /// <summary>
    /// 当日已经执行的交易次数。
    /// </summary>
    public int DailyTradeCount { get; }

    /// <summary>
    /// 当前连续亏损次数。
    /// </summary>
    public int ConsecutiveLosses { get; }

    /// <summary>
    /// 当前已经占用的保证金金额。
    /// </summary>
    public double CurrentMarginUsed { get; }

    /// <summary>
    /// 当日已实现亏损金额。
    ///
    /// 例如 RealizedPnlToday = -500，则 RealizedLossToday = 500；
    /// 如果当天是盈利或持平，则亏损金额按 0 处理。
    /// </summary>
    public double RealizedLossToday => Math.Max(0, -RealizedPnlToday);
}
