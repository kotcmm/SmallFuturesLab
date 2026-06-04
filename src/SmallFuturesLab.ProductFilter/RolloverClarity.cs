namespace SmallFuturesLab.ProductFilter;

/// <summary>
/// 主力合约换月清晰度。
/// </summary>
public enum RolloverClarity
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
