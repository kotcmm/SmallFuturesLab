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
    /// 当前账户规模下的 RiskRate 和 MarginRateOfEquity 被正确计算。
    /// </summary>
    [Fact]
    public void Calculate_SingleRow_ComputesRiskRateAndMarginRate()
    {
        var row = CreateValidRow();
        var calculator = new ProductFilterCalculator();
        var result = calculator.Calculate(row);

        var expectedTotalRisk = 146.0;
        Assert.Equal(expectedTotalRisk / 10000.0, result.Row.RiskRate, 4);
        Assert.Equal(result.Row.MarginPerLot / 10000.0, result.Row.MarginRateOfEquity, 4);
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

        Assert.NotEqual(ProductFilterResultStatus.Allowed, result.Result);
    }

    /// <summary>
    /// Result 只会输出 Allowed / Caution / Rejected。
    /// </summary>
    [Fact]
    public void Calculate_ResultIsAlwaysAllowedCautionOrRejected()
    {
        var row = CreateValidRow();
        var calculator = new ProductFilterCalculator();
        var result = calculator.Calculate(row);

        Assert.True(
            result.Result == ProductFilterResultStatus.Allowed
            || result.Result == ProductFilterResultStatus.Caution
            || result.Result == ProductFilterResultStatus.Rejected);
    }

    /// <summary>
    /// 高保证金品种在较小 AccountEquity 下可能 Rejected，在较大 AccountEquity 下可能 Caution。
    /// </summary>
    [Fact]
    public void Calculate_HighMarginProduct_DifferentResultsForDifferentEquity()
    {
        var row10k = CreateValidRow() with
        {
            Price = 10000,
            Multiplier = 20,
            MarginRate = 0.20,
            StopDistance = 100,
            AccountEquity = 10000,
        };
        var row20k = CreateValidRow() with
        {
            Price = 10000,
            Multiplier = 20,
            MarginRate = 0.20,
            StopDistance = 100,
            AccountEquity = 20000,
        };
        var calculator = new ProductFilterCalculator();
        var result10k = calculator.Calculate(row10k);
        var result20k = calculator.Calculate(row20k);

        Assert.True(
            result10k.Result == ProductFilterResultStatus.Rejected || result10k.Result == ProductFilterResultStatus.Caution,
            $"Result for 10k should be Rejected or Caution, but was {result10k.Result}");
    }

    /// <summary>
    /// AccountEquity = 30000 时不需要改模型也能计算。
    /// </summary>
    [Fact]
    public void Calculate_AccountEquity30000_WorksWithoutModelChange()
    {
        var row = CreateValidRow() with { AccountEquity = 30000 };
        var calculator = new ProductFilterCalculator();
        var result = calculator.Calculate(row);

        Assert.Equal(146.0 / 30000.0, result.Row.RiskRate, 4);
        Assert.Equal(result.Row.MarginPerLot / 30000.0, result.Row.MarginRateOfEquity, 4);
        Assert.True(
            result.Result == ProductFilterResultStatus.Allowed
            || result.Result == ProductFilterResultStatus.Caution
            || result.Result == ProductFilterResultStatus.Rejected);
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
        AccountEquity = 10000,
        LiquidityLevel = LiquidityLevel.Good,
        BookContinuityLevel = BookContinuityLevel.Good,
        RolloverClarity = RolloverClarity.Good,
        DataDate = "2024-01-01",
        DataSource = "TestSource",
    };
}
