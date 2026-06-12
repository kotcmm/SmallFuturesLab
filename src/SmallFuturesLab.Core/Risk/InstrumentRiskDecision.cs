namespace SmallFuturesLab.Core.Risk;

/// <summary>
/// 风险侧对单个品种是否适合入选的判断结果。
/// </summary>
public sealed record InstrumentRiskDecision
{
    /// <summary>
    /// 创建品种风险判断结果。
    /// </summary>
    /// <param name="symbol">合约代码。</param>
    /// <param name="accepted">是否通过品种风险验算。</param>
    /// <param name="rejectReason">未通过品种风险验算时的拒绝原因。</param>
    public InstrumentRiskDecision(string symbol, bool accepted, InstrumentRejectReason rejectReason)
    {
        Symbol = symbol;
        Accepted = accepted;
        RejectReason = rejectReason;
    }

    /// <summary>
    /// 合约代码。
    /// </summary>
    public string Symbol { get; }

    /// <summary>
    /// 是否通过品种风险验算。
    /// </summary>
    public bool Accepted { get; }

    /// <summary>
    /// 未通过品种风险验算时的拒绝原因。
    /// </summary>
    public InstrumentRejectReason RejectReason { get; }

    /// <summary>
    /// 创建通过品种风险验算的判断结果。
    /// </summary>
    /// <param name="symbol">合约代码。</param>
    /// <returns>通过品种风险验算的判断结果。</returns>
    public static InstrumentRiskDecision Accept(string symbol)
    {
        return new InstrumentRiskDecision(symbol, accepted: true, InstrumentRejectReason.None);
    }

    /// <summary>
    /// 创建未通过品种风险验算的判断结果。
    /// </summary>
    /// <param name="symbol">合约代码。</param>
    /// <param name="rejectReason">拒绝原因。</param>
    /// <returns>未通过品种风险验算的判断结果。</returns>
    public static InstrumentRiskDecision Reject(string symbol, InstrumentRejectReason rejectReason)
    {
        return new InstrumentRiskDecision(symbol, accepted: false, rejectReason);
    }
}
