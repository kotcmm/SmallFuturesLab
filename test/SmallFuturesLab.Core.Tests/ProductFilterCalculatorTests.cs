using SmallFuturesLab.Core.Models;
using SmallFuturesLab.Core.Services;

namespace SmallFuturesLab.Core.Tests;

public class ProductFilterCalculatorTests
{
    [Fact]
    public void Calculate_Uses_TickValueReference_When_Multiplier_And_TickSize_Missing()
    {
        var product = CreateProduct() with
        {
            TickValue = 10,
            Multiplier = 0,
            TickSize = 0,
        };

        var result = new ProductFilterCalculator().Calculate(product, new AccountRiskSetting
        {
            AccountEquity = 10_000,
            StopTicks = 10,
            SlippageTicks = 2,
        });

        Assert.Equal(10, result.TickValue);
        Assert.Equal(100, result.StopRiskMoney);
        Assert.Equal(20, result.SlippageMoney);
    }

    [Fact]
    public void Calculate_Rejects_When_Margin_Is_Too_High()
    {
        var product = CreateProduct() with { MarginPerLot = 8_000 };

        var result = new ProductFilterCalculator().Calculate(product, new AccountRiskSetting
        {
            AccountEquity = 10_000,
        });

        Assert.Equal(ProductFilterStatus.Rejected, result.Status);
        Assert.Contains("保证金", result.Reasons);
    }

    [Fact]
    public void Calculate_Returns_Caution_When_Risk_Exceeds_Normal_But_Not_Extreme()
    {
        var product = CreateProduct() with
        {
            TickValue = 15,
            RoundTripFeePerLot = 10,
            MarginPerLot = 2_000,
        };

        var result = new ProductFilterCalculator().Calculate(product, new AccountRiskSetting
        {
            AccountEquity = 10_000,
            StopTicks = 10,
            SlippageTicks = 2,
        });

        Assert.Equal(ProductFilterStatus.Caution, result.Status);
        Assert.True(result.RiskRate > 0.01);
        Assert.True(result.RiskRate <= 0.02);
    }

    private static ProductInfo CreateProduct() => new()
    {
        Exchange = "测试交易所",
        ProductName = "测试品种",
        ProductCode = "MA",
        ContractCode = "MA509",
        Price = 2500,
        TickValue = 10,
        MarginPerLot = 2_500,
        RoundTripFeePerLot = 6,
    };
}
