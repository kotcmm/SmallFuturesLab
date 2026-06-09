using SmallFuturesLab.Core.Accounts;
using SmallFuturesLab.Core.Filtering;
using SmallFuturesLab.Core.Products;
using SmallFuturesLab.Core.Risk;

namespace SmallFuturesLab.Core.Tests;

public sealed class ProductFilterCalculatorTests
{
    [Fact]
    public void Calculate_Computes_Per_Lot_Risk()
    {
        var product = CreateProduct(marginPerLot: 1_500, tickValue: 10, fee: 6);
        var account = new AccountProfile { Equity = 10_000 };
        var scenario = new FilterScenario { Lots = 1, StopTicks = 10, SlippageTicks = 2 };

        var decision = new ProductFilterCalculator().Calculate(product, account, scenario, RiskPolicy.Default);

        Assert.Equal(1_500, decision.MarginPerLot);
        Assert.Equal(0.15, decision.MarginRateOfEquity, 6);
        Assert.Equal(100, decision.StopRiskMoney);
        Assert.Equal(20, decision.SlippageMoney);
        Assert.Equal(26, decision.CostMoney);
        Assert.Equal(126, decision.TotalRiskMoney);
        Assert.Equal(0.0126, decision.RiskRate, 6);
    }

    [Fact]
    public void Calculate_Rejects_When_Margin_Too_High()
    {
        var product = CreateProduct(marginPerLot: 7_000, tickValue: 10, fee: 6);
        var account = new AccountProfile { Equity = 10_000 };
        var scenario = new FilterScenario { Lots = 1, StopTicks = 10, SlippageTicks = 2 };

        var decision = new ProductFilterCalculator().Calculate(product, account, scenario, RiskPolicy.Default);

        Assert.Equal(ProductFilterStatus.Rejected, decision.Status);
        Assert.Contains(decision.Reasons, x => x.Contains("保证金占用", StringComparison.Ordinal));
    }

    [Fact]
    public void Product_Filter_Result_Is_A_Batch_Result()
    {
        var product = CreateProduct(marginPerLot: 1_500, tickValue: 10, fee: 6);
        var account = new AccountProfile { Equity = 10_000 };
        var scenario = new FilterScenario { Lots = 1, StopTicks = 10, SlippageTicks = 2 };
        var decision = new ProductFilterCalculator().Calculate(product, account, scenario, RiskPolicy.Default);

        var result = new ProductFilterResult
        {
            Decisions = new[] { decision },
        };

        Assert.Single(result.Decisions);
    }

    private static ProductInfo CreateProduct(double marginPerLot, double tickValue, double fee)
    {
        return new ProductInfo
        {
            Identity = new ProductIdentity
            {
                Exchange = "TEST",
                ProductCode = "MA",
                ContractCode = "MA2601",
                ProductName = "甲醇",
            },
            Economics = new PerLotEconomics
            {
                Price = 2500,
                MarginPerLot = marginPerLot,
                TickValue = tickValue,
                RoundTripFeePerLot = fee,
            },
        };
    }
}
