namespace SmallFuturesLab.Core.Risk;

/// <summary>
/// 单笔交易风险验算器。
///
/// 职责：编排 TradeSetup 和 ContractRiskProfile 到 TradePlan 的验算流程。
/// 不负责行情判断，不负责账户状态持久化。
/// </summary>
public sealed class TradeRiskEvaluator
{
    private readonly AccountRiskLimits _limits;
    private readonly TradeRiskInputValidator _inputValidator;
    private readonly TradeRiskCalculator _calculator;
    private readonly TradeRiskResultValidator _resultValidator;
    private readonly TradePlanFactory _tradePlanFactory;

    /// <summary>
    /// 创建单笔交易风险验算器。
    /// </summary>
    /// <param name="limits">账户风险边界。</param>
    public TradeRiskEvaluator(AccountRiskLimits limits)
    {
        _limits = limits ?? throw new ArgumentNullException(nameof(limits));
        _inputValidator = new TradeRiskInputValidator();
        _calculator = new TradeRiskCalculator();
        _resultValidator = new TradeRiskResultValidator();
        _tradePlanFactory = new TradePlanFactory();
    }

    /// <summary>
    /// 根据交易结构、合约风险计算资料和当日风险状态生成交易计划。
    ///
    /// 计算顺序：
    /// 1. 验证 TradeSetup 和 ContractRiskProfile 是否具备最小可计算性；
    /// 2. 计算风险中间结果；
    /// 3. 验证风险中间结果是否满足账户边界和当日风险状态；
    /// 4. 返回 TradePlan。
    /// </summary>
    /// <param name="setup">行情结构阶段生成的交易设想。</param>
    /// <param name="contract">合约风险计算资料。</param>
    /// <param name="dailyRiskState">当日风险状态。</param>
    /// <returns>风险验算后的交易计划。</returns>
    public TradePlan Evaluate(TradeSetup setup, ContractRiskProfile contract, DailyRiskState dailyRiskState)
    {
        ArgumentNullException.ThrowIfNull(setup);
        ArgumentNullException.ThrowIfNull(contract);
        ArgumentNullException.ThrowIfNull(dailyRiskState);

        var inputRejectReason = _inputValidator.Validate(setup, contract);
        if (inputRejectReason != RiskRejectReason.None)
        {
            return _tradePlanFactory.Rejected(setup, _limits, dailyRiskState, inputRejectReason);
        }

        var calculation = _calculator.Calculate(_limits, setup, contract, dailyRiskState);

        var resultRejectReason = _resultValidator.Validate(_limits, dailyRiskState, calculation);

        if (resultRejectReason != RiskRejectReason.None)
        {
            return _tradePlanFactory.Rejected(setup, _limits, dailyRiskState, resultRejectReason, calculation);
        }

        return _tradePlanFactory.Accepted(setup, _limits, calculation);
    }
}
