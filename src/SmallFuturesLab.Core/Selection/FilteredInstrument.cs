using SmallFuturesLab.Core.Risk;

namespace SmallFuturesLab.Core.Selection;

/// <summary>
/// 单个品种的过滤结果。
/// </summary>
public sealed record FilteredInstrument(
    InstrumentFilterProfile Profile,
    InstrumentRiskDecision Decision);
