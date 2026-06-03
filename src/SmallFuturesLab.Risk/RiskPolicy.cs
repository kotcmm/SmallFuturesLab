namespace SmallFuturesLab.Risk;

/// <summary>
/// 风险政策，定义账户可承受的风险阈值。
/// </summary>
public record RiskPolicy
{
    /// <summary>
    /// 推荐单笔风险比例（r_rec）。
    /// </summary>
    public double RecommendedRiskRate { get; init; }

    /// <summary>
    /// 常规单笔风险上限（r_norm）。
    /// </summary>
    public double NormalRiskRate { get; init; }

    /// <summary>
    /// 极限单笔风险上限（r_max）。
    /// </summary>
    public double ExtremeRiskRate { get; init; }

    /// <summary>
    /// 每日最大亏损比例（d_max）。
    /// </summary>
    public double DailyMaxLossRate { get; init; }

    /// <summary>
    /// 推荐保证金占用上限（m_pref）。
    /// </summary>
    public double PreferredMarginRate { get; init; }

    /// <summary>
    /// 极限保证金占用上限（m_max）。
    /// </summary>
    public double ExtremeMarginRate { get; init; }

    /// <summary>
    /// 推荐成本占比上限（c_pref），以止损风险为分母。
    /// </summary>
    public double PreferredCostRatio { get; init; }

    /// <summary>
    /// 极限成本占比上限（c_max），以止损风险为分母。
    /// </summary>
    public double ExtremeCostRatio { get; init; }

    /// <summary>
    /// 每日最大交易次数（N_max）。
    /// </summary>
    public int MaxTradesPerDay { get; init; }
}
