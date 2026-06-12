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

    /// <summary>
    /// 创建品种过滤流程编排器。
    /// </summary>
    /// <param name="collector">品种合约资料收集器。</param>
    /// <param name="mapper">品种过滤资料转换器。</param>
    /// <param name="riskEvaluator">品种风险验算器。</param>
    /// <param name="store">已过滤品种结果存储。</param>
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

    /// <summary>
    /// 执行一次品种过滤流程，并用本次结果替换旧结果。
    /// </summary>
    /// <param name="accountRiskLimits">账户风险边界。</param>
    /// <param name="selectionLimits">品种入选边界。</param>
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
