namespace SmallFuturesLab.Core.Selection;

/// <summary>
/// 已过滤品种查询接口。
/// </summary>
public interface IFilteredInstrumentQuery
{
    /// <summary>
    /// 查询已经通过过滤的候选品种。
    /// </summary>
    /// <returns>已经通过过滤的候选品种资料。</returns>
    IReadOnlyList<InstrumentFilterProfile> GetAcceptedInstruments();

    /// <summary>
    /// 查询本次过滤的完整结果。
    /// </summary>
    /// <returns>包含通过和拒绝结果的完整过滤结果。</returns>
    IReadOnlyList<FilteredInstrument> GetAllResults();
}
