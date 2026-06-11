namespace SmallFuturesLab.Core.RiskConstraints;

/// <summary>
/// 账户风险边界。
///
/// 这是账户层的领域值对象，不是配置参数袋。
/// 构造成功代表这组风险边界已经通过最小合法性检查。
///
/// 它只保存账户层风险边界，不保存某一笔交易的行情结构。
/// </summary>
public sealed record AccountRiskLimits
{
    /// <summary>
    /// 创建一组合法的账户风险边界。
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
        AccountEquity = Ensure.Positive(
            accountEquity,
            "账户权益必须大于 0。");

        RiskPercentPerTrade = Ensure.Ratio(
            riskPercentPerTrade,
            "单笔风险比例必须在 (0, 1] 内。");

        MinPlannedRewardR = Ensure.Positive(
            minPlannedRewardR,
            "最低计划盈利倍数必须大于 0。");

        PerTradeCostMaxR = Ensure.NonNegative(
            perTradeCostMaxR,
            "单笔成本上限不能小于 0。");

        MaxMarginUsageRatio = Ensure.Ratio(
            maxMarginUsageRatio,
            "最大保证金占用比例必须在 (0, 1] 内。");

        DailyLossLimitMultiple = Ensure.Positive(
            dailyLossLimitMultiple,
            "每日亏损上限倍数必须大于 0。");

        DailyProfitLockMultiple = Ensure.Positive(
            dailyProfitLockMultiple,
            "每日盈利保护倍数必须大于 0。");

        MaxDailyTrades = Ensure.AtLeast(
            maxDailyTrades,
            minimum: 1,
            "每日最多交易次数必须至少为 1。");

        MaxConsecutiveLosses = Ensure.AtLeast(
            maxConsecutiveLosses,
            minimum: 1,
            "连续亏损暂停笔数必须至少为 1。");
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
