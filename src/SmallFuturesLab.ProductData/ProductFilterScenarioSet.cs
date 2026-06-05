namespace SmallFuturesLab.ProductData;

/// <summary>
/// 一组测算场景，用于把单条品种数据展开成多条测算记录。
/// </summary>
public record ProductFilterScenarioSet
{
    /// <summary>
    /// 测算场景列表。
    /// </summary>
    public IReadOnlyList<ProductFilterScenario> Scenarios { get; init; } = Array.Empty<ProductFilterScenario>();

    /// <summary>
    /// 创建默认测算场景集。
    /// 默认生成 5 个止损场景 × 2 个账户规模 = 10 个场景。
    /// </summary>
    /// <param name="tickSize">最小变动价位，必须是有限数字且大于 0。</param>
    /// <param name="typicalAtr">典型 ATR，必须是有限数字且大于 0。</param>
    /// <param name="slippageTicks">预估滑点 tick 数，不能为负数。</param>
    /// <returns>默认测算场景集。</returns>
    public static ProductFilterScenarioSet CreateDefault(double tickSize, double typicalAtr, int slippageTicks)
    {
        if (double.IsNaN(tickSize) || double.IsInfinity(tickSize) || tickSize <= 0)
        {
            throw new ArgumentException("tickSize 必须是有限数字且大于 0", nameof(tickSize));
        }

        if (double.IsNaN(typicalAtr) || double.IsInfinity(typicalAtr) || typicalAtr <= 0)
        {
            throw new ArgumentException("typicalAtr 必须是有限数字且大于 0", nameof(typicalAtr));
        }

        if (slippageTicks < 0)
        {
            throw new ArgumentException("slippageTicks 不能为负数", nameof(slippageTicks));
        }

        var scenarios = new List<ProductFilterScenario>();
        double[] equities = [10000, 20000];
        var stopConfigs = new (string Name, double Distance)[]
        {
            ("3tick", tickSize * 3),
            ("5tick", tickSize * 5),
            ("10tick", tickSize * 10),
            ("0.5atr", typicalAtr * 0.5),
            ("1.0atr", typicalAtr * 1.0),
        };

        foreach (var equity in equities)
        {
            foreach (var (name, distance) in stopConfigs)
            {
                scenarios.Add(new ProductFilterScenario
                {
                    Name = $"{name}_{equity}",
                    AccountEquity = equity,
                    StopDistance = distance,
                    SlippageTicks = slippageTicks,
                });
            }
        }

        return new ProductFilterScenarioSet { Scenarios = scenarios };
    }
}
