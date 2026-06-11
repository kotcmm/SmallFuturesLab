namespace SmallFuturesLab.Core.RiskConstraints;

/// <summary>
/// 账户风险边界。
///
/// 这个对象只保存账户层的风险边界，不保存某一笔交易的行情结构。
/// 参数命名保持和 docs 中的业务术语一致，方便从文档公式直接映射到代码。
/// </summary>
public sealed record AccountRiskLimits
{
    /// <summary>
    /// 创建账户风险边界。
    /// </summary>
    /// <param name="accountEquity">账户权益。</param>
    /// <param name="riskPercentPerTrade">单笔风险比例，例如 0.005 表示 0.5%。</param>
    /// <param name="minPlannedRewardR">单笔最低计划盈利倍数，例如 2.5 表示至少计划 2.5R 的目标空间。</param>
    /// <param name="perTradeCostMaxR">单笔成本上限，单位是 R。</param>
    /// <param name="maxMarginUsageRatio">账户最大允许保证金占用比例。</param>
    /// <param name="dailyLossLimitMultiple">每日亏损上限相对 AccountR 的倍数。</param>
    /// <param name="dailyProfitLockMultiple">每日盈利保护线相对 AccountR 的倍数。</param>
    /// <param name="maxDailyTrades">每日最多新开仓次数。</param>
    /// <param name="maxConsecutiveLosses">达到多少次连续亏损后暂停新开仓。</param>
    public AccountRiskLimits(
        double accountEquity,
        double riskPercentPerTrade,
        double minPlannedRewardR,
        double perTradeCostMaxR,
        double maxMarginUsageRatio,
        double dailyLossLimitMultiple,
        double dailyProfitLockMultiple,
        int maxDailyTrades,
        int maxConsecutiveLosses)
    {
        if (accountEquity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(accountEquity), "账户权益必须大于 0。");
        }

        if (riskPercentPerTrade <= 0 || riskPercentPerTrade > 1)
        {
            throw new ArgumentOutOfRangeException(nameof(riskPercentPerTrade), "单笔风险比例必须在 (0, 1] 内。");
        }

        if (minPlannedRewardR <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(minPlannedRewardR), "最低计划盈利倍数必须大于 0。");
        }

        if (perTradeCostMaxR < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(perTradeCostMaxR), "单笔成本上限不能小于 0。");
        }

        if (maxMarginUsageRatio <= 0 || maxMarginUsageRatio > 1)
        {
            throw new ArgumentOutOfRangeException(nameof(maxMarginUsageRatio), "最大保证金占用比例必须在 (0, 1] 内。");
        }

        if (dailyLossLimitMultiple <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(dailyLossLimitMultiple), "每日亏损上限倍数必须大于 0。");
        }

        if (dailyProfitLockMultiple <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(dailyProfitLockMultiple), "每日盈利保护倍数必须大于 0。");
        }

        if (maxDailyTrades < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(maxDailyTrades), "每日最多交易次数必须至少为 1。");
        }

        if (maxConsecutiveLosses < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(maxConsecutiveLosses), "连续亏损暂停笔数必须至少为 1。");
        }

        AccountEquity = accountEquity;
        RiskPercentPerTrade = riskPercentPerTrade;
        MinPlannedRewardR = minPlannedRewardR;
        PerTradeCostMaxR = perTradeCostMaxR;
        MaxMarginUsageRatio = maxMarginUsageRatio;
        DailyLossLimitMultiple = dailyLossLimitMultiple;
        DailyProfitLockMultiple = dailyProfitLockMultiple;
        MaxDailyTrades = maxDailyTrades;
        MaxConsecutiveLosses = maxConsecutiveLosses;
    }

    /// <summary>
    /// 账户权益。
    /// </summary>
    public double AccountEquity { get; }

    /// <summary>
    /// 单笔风险比例。
    /// </summary>
    public double RiskPercentPerTrade { get; }

    /// <summary>
    /// 单笔最低计划盈利倍数。
    /// </summary>
    public double MinPlannedRewardR { get; }

    /// <summary>
    /// 单笔成本上限，单位是 R。
    /// </summary>
    public double PerTradeCostMaxR { get; }

    /// <summary>
    /// 账户最大允许保证金占用比例。
    /// </summary>
    public double MaxMarginUsageRatio { get; }

    /// <summary>
    /// 每日亏损上限相对 AccountR 的倍数。
    /// </summary>
    public double DailyLossLimitMultiple { get; }

    /// <summary>
    /// 每日盈利保护线相对 AccountR 的倍数。
    /// </summary>
    public double DailyProfitLockMultiple { get; }

    /// <summary>
    /// 每日最多新开仓次数。
    /// </summary>
    public int MaxDailyTrades { get; }

    /// <summary>
    /// 连续亏损暂停笔数。
    /// </summary>
    public int MaxConsecutiveLosses { get; }

    /// <summary>
    /// AccountR = AccountEquity × RiskPercentPerTrade。
    ///
    /// 含义：账户允许的单笔风险上限。
    /// </summary>
    public double AccountR => AccountEquity * RiskPercentPerTrade;

    /// <summary>
    /// DailyLossLimit = AccountR × DailyLossLimitMultiple。
    ///
    /// 含义：当天已实现亏损达到该金额后停止新开仓。
    /// </summary>
    public double DailyLossLimit => AccountR * DailyLossLimitMultiple;

    /// <summary>
    /// DailyProfitLockR = AccountR × DailyProfitLockMultiple。
    ///
    /// 含义：当天已实现盈利达到该金额后停止新开仓。
    /// </summary>
    public double DailyProfitLockR => AccountR * DailyProfitLockMultiple;

    /// <summary>
    /// MaxAllowedMargin = AccountEquity × MaxMarginUsageRatio。
    ///
    /// 含义：账户允许的最大保证金占用金额。
    /// </summary>
    public double MaxAllowedMargin => AccountEquity * MaxMarginUsageRatio;
}
