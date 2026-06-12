using SmallFuturesLab.Core.Risk;

namespace SmallFuturesLab.Core.Selection;

/// <summary>
/// 品种过滤流程编排器。
///
/// 负责串联收集、转换、风险判断和结果保存。
/// </summary>
public sealed class InstrumentFilter
{
    private readonly IInstrumentCollector _collector;
    private readonly InstrumentFilterProfileMapper _mapper;
    private readonly InstrumentRiskEvaluator _riskEvaluator;
    private readonly FilteredInstrumentStore _store;

    public InstrumentFilter(
        IInstrumentCollector collector,
        InstrumentFilterProfileMapper mapper,
        InstrumentRiskEvaluator riskEvaluator,
        FilteredInstrumentStore store)
    {
        _collector = collector ?? throw new ArgumentNullException(nameof(collector));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _riskEvaluator = riskEvaluator ?? throw new ArgumentNullException(nameof(riskEvaluator));
        _store = store ?? throw new ArgumentNullException(nameof(store));
    }

    public void Run(AccountRiskLimits accountRiskLimits, InstrumentSelectionLimits selectionLimits)
    {
        ArgumentNullException.ThrowIfNull(accountRiskLimits);
        ArgumentNullException.ThrowIfNull(selectionLimits);

        var results = _collector
            .Collect()
            .Select(_mapper.Map)
            .Select(profile => new FilteredInstrument(
                Profile: profile,
                Decision: _riskEvaluator.Evaluate(profile, accountRiskLimits, selectionLimits)))
            .ToList();

        _store.ReplaceAll(results);
    }
}
