namespace SmallFuturesLab.Core.Selection;

/// <summary>
/// 品种合约资料收集器。
///
/// 只负责从外部来源收集合约资料，不做过滤、不做风险判断。
/// </summary>
public interface IInstrumentCollector
{
    /// <summary>
    /// 收集合约原始资料。
    /// </summary>
    /// <returns>合约原始资料列表。</returns>
    IReadOnlyList<InstrumentRawInfo> Collect();
}
