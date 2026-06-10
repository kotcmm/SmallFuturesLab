namespace SmallFuturesLab.TradingPlanet;

/// <summary>
/// 品种规格查找表。
/// 用于根据品种代码补齐合约乘数和最小变动价位。
/// 不连接网络，不查询真实交易所。
/// </summary>
public sealed class ProductSpecLookup
{
    private readonly Dictionary<string, ProductSpec> _specs;

    /// <summary>
    /// 使用默认品种规格创建查找表。
    /// </summary>
    public ProductSpecLookup()
    {
        _specs = new Dictionary<string, ProductSpec>(StringComparer.OrdinalIgnoreCase)
        {
            { "MA", new ProductSpec(10, 1) },
        };
    }

    /// <summary>
    /// 使用自定义品种规格创建查找表。
    /// </summary>
    /// <param name="specs">品种代码到规格的映射。</param>
    public ProductSpecLookup(IReadOnlyDictionary<string, ProductSpec> specs)
    {
        _specs = new Dictionary<string, ProductSpec>(specs, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// 查找品种规格。
    /// </summary>
    /// <param name="productCode">品种代码。</param>
    /// <returns>查找结果，找不到时返回 null。</returns>
    public ProductSpec? Find(string productCode)
    {
        if (string.IsNullOrWhiteSpace(productCode))
        {
            return null;
        }

        return _specs.TryGetValue(productCode, out var spec) ? spec : null;
    }
}

/// <summary>
/// 品种规格。
/// </summary>
public sealed record ProductSpec
{
    /// <summary>合约乘数。</summary>
    public double Multiplier { get; init; }

    /// <summary>最小变动价位。</summary>
    public double TickSize { get; init; }

    /// <summary>
    /// 创建品种规格。
    /// </summary>
    /// <param name="multiplier">合约乘数。</param>
    /// <param name="tickSize">最小变动价位。</param>
    public ProductSpec(double multiplier, double tickSize)
    {
        Multiplier = multiplier;
        TickSize = tickSize;
    }
}
