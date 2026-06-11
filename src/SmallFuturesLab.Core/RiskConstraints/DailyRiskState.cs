namespace SmallFuturesLab.Core.RiskConstraints;

public sealed record DailyRiskState
{
    public DailyRiskState(
        double RealizedPnlToday,
        int DailyTradeCount,
        int ConsecutiveLosses,
        double CurrentMarginUsed)
    {
        if (DailyTradeCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(DailyTradeCount), "当日交易次数不能小于 0。");
        }

        if (ConsecutiveLosses < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(ConsecutiveLosses), "连续亏损次数不能小于 0。");
        }

        if (CurrentMarginUsed < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(CurrentMarginUsed), "当前保证金占用不能小于 0。");
        }

        this.RealizedPnlToday = RealizedPnlToday;
        this.DailyTradeCount = DailyTradeCount;
        this.ConsecutiveLosses = ConsecutiveLosses;
        this.CurrentMarginUsed = CurrentMarginUsed;
    }

    public double RealizedPnlToday { get; }

    public int DailyTradeCount { get; }

    public int ConsecutiveLosses { get; }

    public double CurrentMarginUsed { get; }

    public double RealizedLossToday => Math.Max(0, -RealizedPnlToday);
}
