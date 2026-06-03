namespace SmallFuturesLab.ProductFilter;

/// <summary>
/// 盘口连续性等级。
/// </summary>
public enum BookContinuityLevel
{
    /// <summary>
    /// 良好。
    /// </summary>
    Good,

    /// <summary>
    /// 一般。
    /// </summary>
    Medium,

    /// <summary>
    /// 较差。
    /// </summary>
    Poor,

    /// <summary>
    /// 暂无数据。
    /// </summary>
    Unknown,
}
