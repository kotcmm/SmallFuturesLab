namespace SmallFuturesLab.Core.Selection;

/// <summary>
/// 已过滤品种查询接口。
/// </summary>
public interface IFilteredInstrumentQuery
{
    IReadOnlyList<InstrumentFilterProfile> GetAcceptedInstruments();

    IReadOnlyList<FilteredInstrument> GetAllResults();
}
