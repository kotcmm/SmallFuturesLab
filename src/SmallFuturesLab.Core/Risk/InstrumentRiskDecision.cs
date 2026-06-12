namespace SmallFuturesLab.Core.Risk;

/// <summary>
/// 风险侧对单个品种是否适合入选的判断结果。
/// </summary>
public sealed record InstrumentRiskDecision(
    string Symbol,
    bool Accepted,
    InstrumentRejectReason RejectReason)
{
    public static InstrumentRiskDecision Accept(string symbol)
    {
        return new InstrumentRiskDecision(symbol, Accepted: true, InstrumentRejectReason.None);
    }

    public static InstrumentRiskDecision Reject(string symbol, InstrumentRejectReason rejectReason)
    {
        return new InstrumentRiskDecision(symbol, Accepted: false, rejectReason);
    }
}
