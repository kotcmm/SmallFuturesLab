namespace SmallFuturesLab.Core.Risk;

/// <summary>
/// 单笔交易风险输入验证器。
///
/// 只验证 TradeSetup 和 ContractRiskProfile 是否具备后续风险计算的最小合法性，
/// 不计算风险结果，也不判断风险结果是否满足账户约束。
/// </summary>
internal sealed class TradeRiskInputValidator
{
    /// <summary>
    /// 验证交易结构输入和合约风险计算资料。
    /// </summary>
    /// <param name="setup">行情结构阶段生成的交易设想。</param>
    /// <param name="contract">合约风险计算资料。</param>
    /// <returns>拒绝原因；通过时返回 None。</returns>
    public RiskRejectReason Validate(
        TradeSetup setup,
        ContractRiskProfile contract)
    {
        var setupRejectReason = ValidateTradeSetup(setup);
        if (setupRejectReason != RiskRejectReason.None)
        {
            return setupRejectReason;
        }

        return ValidateContractRiskProfile(setup, contract);
    }

    /// <summary>
    /// 检查 TradeSetup 是否具备最小可计算性。
    /// </summary>
    private static RiskRejectReason ValidateTradeSetup(TradeSetup setup)
    {
        if (string.IsNullOrWhiteSpace(setup.Symbol))
        {
            return RiskRejectReason.InvalidTradeSetup;
        }

        if (setup.Direction is not TradeDirection.Long and not TradeDirection.Short)
        {
            return RiskRejectReason.InvalidTradeSetup;
        }

        if (setup.EntryPrice <= 0 || setup.StopPrice <= 0)
        {
            return RiskRejectReason.InvalidTradeSetup;
        }

        if (setup.EntryPrice == setup.StopPrice)
        {
            return RiskRejectReason.InvalidTradeSetup;
        }

        return RiskRejectReason.None;
    }

    /// <summary>
    /// 检查 ContractRiskProfile 是否具备最小可计算性。
    /// </summary>
    private static RiskRejectReason ValidateContractRiskProfile(
        TradeSetup setup,
        ContractRiskProfile contract)
    {
        if (string.IsNullOrWhiteSpace(contract.Symbol))
        {
            return RiskRejectReason.InvalidContractRiskProfile;
        }

        if (!string.Equals(setup.Symbol, contract.Symbol, StringComparison.Ordinal))
        {
            return RiskRejectReason.InvalidContractRiskProfile;
        }

        if (contract.Multiplier <= 0)
        {
            return RiskRejectReason.InvalidContractRiskProfile;
        }

        if (contract.EstimatedRoundTripCostPerLot < 0)
        {
            return RiskRejectReason.InvalidContractRiskProfile;
        }

        if (contract.OneLotMargin < 0)
        {
            return RiskRejectReason.InvalidContractRiskProfile;
        }

        return RiskRejectReason.None;
    }
}
