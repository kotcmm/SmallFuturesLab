namespace SmallFuturesLab.Core.RiskConstraints;

public sealed record RiskConstraintConfig
{
    public RiskConstraintConfig(
        double AccountEquity,
        double RiskPercentPerTrade,
        double MinPlannedRewardR,
        double PerTradeCostMaxR,
        double MaxMarginUsageRatio,
        double DailyLossLimitMultiple,
        double DailyProfitLockMultiple,
        int MaxDailyTrades,
        int MaxConsecutiveLosses)
    {
        if (AccountEquity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(AccountEquity), "账户权益必须大于 0。");
        }

        if (RiskPercentPerTrade <= 0 || RiskPercentPerTrade > 1)
        {
            throw new ArgumentOutOfRangeException(nameof(RiskPercentPerTrade), "单笔风险比例必须在 (0, 1] 内。");
        }

        if (MinPlannedRewardR <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(MinPlannedRewardR), "最低计划盈利倍数必须大于 0。");
        }

        if (PerTradeCostMaxR < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(PerTradeCostMaxR), "单笔成本上限不能小于 0。");
        }

        if (MaxMarginUsageRatio <= 0 || MaxMarginUsageRatio > 1)
        {
            throw new ArgumentOutOfRangeException(nameof(MaxMarginUsageRatio), "最大保证金占用比例必须在 (0, 1] 内。");
        }

        if (DailyLossLimitMultiple <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(DailyLossLimitMultiple), "每日亏损上限倍数必须大于 0。");
        }

        if (DailyProfitLockMultiple <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(DailyProfitLockMultiple), "每日盈利保护倍数必须大于 0。");
        }

        if (MaxDailyTrades < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(MaxDailyTrades), "每日最多交易次数必须至少为 1。");
        }

        if (MaxConsecutiveLosses < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(MaxConsecutiveLosses), "连续亏损暂停笔数必须至少为 1。");
        }

        this.AccountEquity = AccountEquity;
        this.RiskPercentPerTrade = RiskPercentPerTrade;
        this.MinPlannedRewardR = MinPlannedRewardR;
        this.PerTradeCostMaxR = PerTradeCostMaxR;
        this.MaxMarginUsageRatio = MaxMarginUsageRatio;
        this.DailyLossLimitMultiple = DailyLossLimitMultiple;
        this.DailyProfitLockMultiple = DailyProfitLockMultiple;
        this.MaxDailyTrades = MaxDailyTrades;
        this.MaxConsecutiveLosses = MaxConsecutiveLosses;
    }

    public double AccountEquity { get; }

    public double RiskPercentPerTrade { get; }

    public double MinPlannedRewardR { get; }

    public double PerTradeCostMaxR { get; }

    public double MaxMarginUsageRatio { get; }

    public double DailyLossLimitMultiple { get; }

    public double DailyProfitLockMultiple { get; }

    public int MaxDailyTrades { get; }

    public int MaxConsecutiveLosses { get; }

    public double AccountR => AccountEquity * RiskPercentPerTrade;

    public double DailyLossLimit => AccountR * DailyLossLimitMultiple;

    public double DailyProfitLockR => AccountR * DailyProfitLockMultiple;

    public double MaxAllowedMargin => AccountEquity * MaxMarginUsageRatio;
}
