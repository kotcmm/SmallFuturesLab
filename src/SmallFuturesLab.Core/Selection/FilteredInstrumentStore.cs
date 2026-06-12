namespace SmallFuturesLab.Core.Selection;

/// <summary>
/// 已过滤品种结果存储。
/// </summary>
public sealed class FilteredInstrumentStore : IFilteredInstrumentQuery
{
    private readonly List<FilteredInstrument> _items = [];

    public void ReplaceAll(IEnumerable<FilteredInstrument> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        _items.Clear();
        _items.AddRange(items);
    }

    public IReadOnlyList<InstrumentFilterProfile> GetAcceptedInstruments()
    {
        return _items
            .Where(x => x.Decision.Accepted)
            .Select(x => x.Profile)
            .ToList();
    }

    public IReadOnlyList<FilteredInstrument> GetAllResults()
    {
        return _items.ToList();
    }
}
