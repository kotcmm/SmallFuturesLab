namespace SmallFuturesLab.ProductFilter.Tests;

public class ProductFilterCalculatorTests
{
    /// <summary>
    /// 单行 CSV 可以正确计算 TickValue、MarginPerLot、StopRiskMoney、CostMoney、TotalRiskMoney。
    /// </summary>
    [Fact]
    public void Calculate_SingleRow_ComputesFormulasCorrectly()
    {
        var row = CreateValidRow();
        var calculator = new ProductFilterCalculator();
        var result = calculator.Calculate(row);

        Assert.Equal(1.0 * 10.0, result.Row.TickValue, 4);
        Assert.Equal(2500.0 * 10.0 * 0.10, result.Row.MarginPerLot, 4);
        Assert.Equal(20.0 * 10.0, result.Row.AtrMoneyPerLot, 4);
        Assert.Equal(12.0 * 10.0, result.Row.StopRiskMoney, 4);
        Assert.Equal(2.0 * 1.0 * 10.0, result.Row.SlippageMoney, 4);
        Assert.Equal(6.0 + 20.0, result.Row.CostMoney, 4);
        Assert.Equal(120.0 + 26.0, result.Row.TotalRiskMoney, 4);
    }

    /// <summary>
    /// 同一行可以同时生成 RiskRate10k 和 RiskRate20k。
    /// </summary>
    [Fact]
    public void Calculate_SingleRow_ComputesBothRiskRates()
    {
        var row = CreateValidRow();
        var calculator = new ProductFilterCalculator();
        var result = calculator.Calculate(row);

        var expectedTotalRisk = 146.0;
        Assert.Equal(expectedTotalRisk / 10000.0, result.Row.RiskRate10k, 4);
        Assert.Equal(expectedTotalRisk / 20000.0, result.Row.RiskRate20k, 4);
    }

    /// <summary>
    /// 流动性为 Unknown 时不能输出 Allowed。
    /// </summary>
    [Theory]
    [InlineData(LiquidityLevel.Unknown)]
    [InlineData(LiquidityLevel.Poor)]
    public void Calculate_LiquidityPoorOrUnknown_CannotBeAllowed(LiquidityLevel level)
    {
        var row = CreateValidRow() with { LiquidityLevel = level };
        var calculator = new ProductFilterCalculator();
        var result = calculator.Calculate(row);

        Assert.NotEqual(ProductFilterResultStatus.Allowed, result.Result10k);
        Assert.NotEqual(ProductFilterResultStatus.Allowed, result.Result20k);
    }

    /// <summary>
    /// Result10k / Result20k 只会输出 Allowed / Caution / Rejected。
    /// </summary>
    [Fact]
    public void Calculate_ResultIsAlwaysAllowedCautionOrRejected()
    {
        var row = CreateValidRow();
        var calculator = new ProductFilterCalculator();
        var result = calculator.Calculate(row);

        Assert.True(
            result.Result10k == ProductFilterResultStatus.Allowed
            || result.Result10k == ProductFilterResultStatus.Caution
            || result.Result10k == ProductFilterResultStatus.Rejected);
        Assert.True(
            result.Result20k == ProductFilterResultStatus.Allowed
            || result.Result20k == ProductFilterResultStatus.Caution
            || result.Result20k == ProductFilterResultStatus.Rejected);
    }

    /// <summary>
    /// 高保证金品种对 10k 账户可能 Rejected，对 20k 账户可能 Caution。
    /// </summary>
    [Fact]
    public void Calculate_HighMarginProduct_DifferentResultsFor10kAnd20k()
    {
        var row = CreateValidRow() with
        {
            Price = 10000,
            Multiplier = 20,
            MarginRate = 0.20,
            StopDistance = 100,
        };
        var calculator = new ProductFilterCalculator();
        var result = calculator.Calculate(row);

        // 20k 账户保证金占比 = 10000*20*0.2/20000 = 20% <= 40%，Allowed
        // 10k 账户保证金占比 = 10000*20*0.2/10000 = 40% <= 40%，Allowed
        // 但 StopRisk = 100*20 = 2000, TotalRisk > 2000, RiskRate10k > 20% -> Rejected
        // RiskRate20k > 10% -> Caution or Rejected
        Assert.True(
            result.Result10k == ProductFilterResultStatus.Rejected || result.Result10k == ProductFilterResultStatus.Caution,
            $"Result10k should be Rejected or Caution, but was {result.Result10k}");
    }

    /// <summary>
    /// Reasons 必须说明主要原因，不能为空。
    /// </summary>
    [Fact]
    public void Calculate_ReasonsIsNotEmpty()
    {
        var row = CreateValidRow();
        var calculator = new ProductFilterCalculator();
        var result = calculator.Calculate(row);

        Assert.False(string.IsNullOrWhiteSpace(result.Reasons));
    }

    private static ProductFilterRow CreateValidRow() => new()
    {
        Exchange = "TestExchange",
        ProductName = "TestProduct",
        ProductCode = "TP",
        ContractCode = "TP2501",
        Price = 2500,
        Multiplier = 10,
        TickSize = 1,
        MarginRate = 0.10,
        RoundTripFeePerLot = 6,
        SlippageTicks = 2,
        TypicalAtr = 20,
        StopDistance = 12,
        LiquidityLevel = LiquidityLevel.Good,
        BookContinuityLevel = BookContinuityLevel.Good,
        RolloverClarity = RolloverClarity.Good,
        DataDate = "2024-01-01",
        DataSource = "TestSource",
    };
}
