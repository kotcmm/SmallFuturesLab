using SmallFuturesLab.Core.Selection;

namespace SmallFuturesLab.Core.Risk;

/// <summary>
/// 品种风险验算器。
///
/// 只判断品种是否适合进入候选池，不判断具体入场、止损、目标价和手数。
/// </summary>
public sealed class InstrumentRiskEvaluator
{
    /// <summary>
    /// 判断单个品种是否适合进入候选池。
    /// </summary>
    /// <param name="instrument">品种过滤使用的统一内部对象。</param>
    /// <param name="accountRiskLimits">账户风险边界。</param>
    /// <param name="selectionLimits">品种入选边界。</param>
    /// <returns>品种是否适合入选的判断结果。</returns>
    public InstrumentRiskDecision Evaluate(
        InstrumentFilterProfile instrument,
        AccountRiskLimits accountRiskLimits,
        InstrumentSelectionLimits selectionLimits)
    {
        ArgumentNullException.ThrowIfNull(instrument);
        ArgumentNullException.ThrowIfNull(accountRiskLimits);
        ArgumentNullException.ThrowIfNull(selectionLimits);

        if (IsInvalid(instrument))
        {
            return InstrumentRiskDecision.Reject(instrument.Symbol, InstrumentRejectReason.InvalidInstrument);
        }

        if (!instrument.IsTradingAllowed)
        {
            return InstrumentRiskDecision.Reject(instrument.Symbol, InstrumentRejectReason.TradingNotAllowed);
        }

        if (instrument.Volume < selectionLimits.MinVolume)
        {
            return InstrumentRiskDecision.Reject(instrument.Symbol, InstrumentRejectReason.VolumeTooLow);
        }

        if (instrument.OpenInterest < selectionLimits.MinOpenInterest)
        {
            return InstrumentRiskDecision.Reject(instrument.Symbol, InstrumentRejectReason.OpenInterestTooLow);
        }

        if (instrument.RoundTripFeePerLot > selectionLimits.MaxRoundTripFeePerLot)
        {
            return InstrumentRiskDecision.Reject(instrument.Symbol, InstrumentRejectReason.FeeTooHigh);
        }

        if (instrument.TickValue > selectionLimits.MaxTickValue)
        {
            return InstrumentRiskDecision.Reject(instrument.Symbol, InstrumentRejectReason.TickValueTooLarge);
        }

        if (CalculateMinimumTradeRisk(instrument) > accountRiskLimits.AccountR * selectionLimits.MaxMinimumTradeRiskAccountRRatio)
        {
            return InstrumentRiskDecision.Reject(instrument.Symbol, InstrumentRejectReason.MinimumTradeRiskTooLarge);
        }

        return InstrumentRiskDecision.Accept(instrument.Symbol);
    }

    private static bool IsInvalid(InstrumentFilterProfile instrument)
    {
        return string.IsNullOrWhiteSpace(instrument.Symbol)
            || instrument.Multiplier <= 0
            || instrument.PriceTick <= 0
            || instrument.LastPrice <= 0
            || instrument.RoundTripFeePerLot < 0
            || instrument.Volume < 0
            || instrument.OpenInterest < 0;
    }

    private static double CalculateMinimumTradeRisk(InstrumentFilterProfile instrument)
    {
        return instrument.TickValue + instrument.RoundTripFeePerLot;
    }
}
