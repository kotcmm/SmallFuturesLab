namespace SmallFuturesLab.Core.Accounts;

/// <summary>
/// 账户资料。
/// </summary>
public sealed record AccountProfile
{
    /// <summary>账户权益。</summary>
    public double Equity { get; init; }
}
