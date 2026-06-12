namespace SmallFuturesLab.Core.Selection;

/// <summary>
/// 已过滤品种结果存储。
/// </summary>
public sealed class FilteredInstrumentStore : IFilteredInstrumentQuery
{
    private readonly List<FilteredInstrument> _items = [];

    /// <summary>
    /// 用新的过滤结果替换当前存储的全部结果。
    /// </summary>
    /// <param name="items">新的过滤结果。</param>
    public void ReplaceAll(IEnumerable<FilteredInstrument> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        _items.Clear();
        _items.AddRange(items);
    }

    /// <summary>
    /// 查询已经通过过滤的候选品种。
    /// </summary>
    /// <returns>已经通过过滤的候选品种资料。</returns>
    public IReadOnlyList<InstrumentFilterProfile> GetAcceptedInstruments()
    {
        return _items
            .Where(x => x.Decision.Accepted)
            .Select(x => x.Profile)
            .ToList();
    }

    /// <summary>
    /// 查询本次过滤的完整结果。
    /// </summary>
    /// <returns>包含通过和拒绝结果的完整过滤结果。</returns>
    public IReadOnlyList<FilteredInstrument> GetAllResults()
    {
        return _items.ToList();
    }
}
