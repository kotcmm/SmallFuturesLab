namespace SmallFuturesLab.Core.Selection;

/// <summary>
/// 品种入选边界。
///
/// 这些边界用于判断品种是否值得进入今日候选池，不等同于账户风险边界。
/// </summary>
public sealed record InstrumentSelectionLimits(
    long MinVolume,
    long MinOpenInterest,
    double MaxRoundTripFeePerLot,
    double MaxTickValue,
    double MaxMinimumTradeRiskAccountRRatio);
