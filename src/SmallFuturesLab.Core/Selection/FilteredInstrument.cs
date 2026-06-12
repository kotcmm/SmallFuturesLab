using SmallFuturesLab.Core.Risk;

namespace SmallFuturesLab.Core.Selection;

/// <summary>
/// 单个品种的过滤结果。
/// </summary>
public sealed record FilteredInstrument
{
    /// <summary>
    /// 创建单个品种的过滤结果。
    /// </summary>
    /// <param name="Profile">品种过滤使用的统一内部对象。</param>
    /// <param name="Decision">风险侧对该品种是否适合入选的判断结果。</param>
    public FilteredInstrument(InstrumentFilterProfile Profile, InstrumentRiskDecision Decision)
    {
        this.Profile = Profile;
        this.Decision = Decision;
    }

    /// <summary>
    /// 品种过滤使用的统一内部对象。
    /// </summary>
    public InstrumentFilterProfile Profile { get; }

    /// <summary>
    /// 风险侧对该品种是否适合入选的判断结果。
    /// </summary>
    public InstrumentRiskDecision Decision { get; }
}
