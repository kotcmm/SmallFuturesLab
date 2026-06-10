namespace SmallFuturesLab.Core.Tests;

public sealed class ProductFilterTests
{
    [Fact]
    public void TickValue_Equals_TickSize_Times_Multiplier()
    {
        var contract = CreateContract(tickSize: 1, multiplier: 10);

        Assert.Equal(10, contract.TickValue);
    }

    [Fact]
    public void MarginPerLot_Equals_Price_Times_Multiplier_Times_MarginRate()
    {
        var contract = CreateContract(price: 2500, multiplier: 10, marginRate: 0.1);

        Assert.Equal(2500, contract.MarginPerLot);
    }

    [Fact]
    public void StopRiskMoney_Equals_StopTicks_Times_TickValue_Times_Lots()
    {
        var contract = CreateContract(tickSize: 1, multiplier: 10, stopTicks: 10, lots: 2);

        Assert.Equal(200, contract.StopRiskMoney);
    }

    [Fact]
    public void SlippageMoney_Equals_SlippageTicks_Times_TickValue_Times_Lots()
    {
        var contract = CreateContract(tickSize: 1, multiplier: 10, slippageTicks: 2, lots: 2);

        Assert.Equal(40, contract.SlippageMoney);
    }

    [Fact]
    public void CostMoney_Equals_RoundTripFee_Times_Lots_Plus_SlippageMoney()
    {
        var contract = CreateContract(tickSize: 1, multiplier: 10, roundTripFee: 6, slippageTicks: 2, lots: 1);

        Assert.Equal(26, contract.CostMoney);
    }

    [Fact]
    public void TotalRiskMoney_Equals_StopRiskMoney_Plus_CostMoney()
    {
        var contract = CreateContract(tickSize: 1, multiplier: 10, roundTripFee: 6, stopTicks: 10, slippageTicks: 2, lots: 1);

        Assert.Equal(126, contract.TotalRiskMoney);
    }

    [Fact]
    public void RiskRate_Equals_TotalRiskMoney_Divided_By_AccountEquity()
    {
        var contract = CreateContract(tickSize: 1, multiplier: 10, roundTripFee: 6, stopTicks: 10, slippageTicks: 2, lots: 1);
        var config = new RiskConfig { AccountEquity = 10_000 };

        var result = new ProductFilter().Evaluate(contract, config);

        Assert.Equal(0.0126, result.RiskRate, 6);
    }

    [Fact]
    public void MarginRate_Equals_MarginMoney_Divided_By_AccountEquity()
    {
        var contract = CreateContract(price: 2500, multiplier: 10, marginRate: 0.1, lots: 1);
        var config = new RiskConfig { AccountEquity = 10_000 };

        var result = new ProductFilter().Evaluate(contract, config);

        Assert.Equal(0.25, result.MarginRate, 6);
    }

    [Fact]
    public void CostRatio_Equals_CostMoney_Divided_By_StopRiskMoney()
    {
        var contract = CreateContract(tickSize: 1, multiplier: 10, roundTripFee: 6, stopTicks: 10, slippageTicks: 2, lots: 1);
        var config = new RiskConfig { AccountEquity = 10_000 };

        var result = new ProductFilter().Evaluate(contract, config);

        Assert.Equal(0.26, result.CostRatio, 6);
    }

    [Fact]
    public void Rejected_When_RiskRate_Exceeds_RejectRiskRate()
    {
        var contract = CreateContract(tickSize: 1, multiplier: 10, roundTripFee: 6, stopTicks: 100, slippageTicks: 0, lots: 1);
        var config = new RiskConfig { AccountEquity = 10_000 };

        var result = new ProductFilter().Evaluate(contract, config);

        Assert.Equal(ProductFilterStatus.Rejected, result.Status);
        Assert.Contains(result.Reasons, r => r.Contains("单笔风险", StringComparison.Ordinal));
    }

    [Fact]
    public void Rejected_When_MarginRate_Exceeds_RejectMarginRate()
    {
        var contract = CreateContract(price: 10_000, multiplier: 10, marginRate: 0.6, lots: 1);
        var config = new RiskConfig { AccountEquity = 10_000 };

        var result = new ProductFilter().Evaluate(contract, config);

        Assert.Equal(ProductFilterStatus.Rejected, result.Status);
        Assert.Contains(result.Reasons, r => r.Contains("保证金占用", StringComparison.Ordinal));
    }

    [Fact]
    public void Rejected_When_CostRatio_Exceeds_RejectCostRatio()
    {
        var contract = CreateContract(tickSize: 1, multiplier: 10, roundTripFee: 100, stopTicks: 10, slippageTicks: 0, lots: 1);
        var config = new RiskConfig { AccountEquity = 100_000 };

        var result = new ProductFilter().Evaluate(contract, config);

        Assert.Equal(ProductFilterStatus.Rejected, result.Status);
        Assert.Contains(result.Reasons, r => r.Contains("成本占止损风险", StringComparison.Ordinal));
    }

    [Fact]
    public void Caution_When_RiskRate_Enters_Caution_Zone()
    {
        var contract = CreateContract(tickSize: 1, multiplier: 10, roundTripFee: 6, stopTicks: 15, slippageTicks: 0, lots: 1);
        var config = new RiskConfig { AccountEquity = 10_000 };

        var result = new ProductFilter().Evaluate(contract, config);

        Assert.Equal(ProductFilterStatus.Caution, result.Status);
        Assert.Contains(result.Reasons, r => r.Contains("单笔风险", StringComparison.Ordinal) && r.Contains("谨慎区", StringComparison.Ordinal));
    }

    [Fact]
    public void Allowed_When_All_Thresholds_Not_Triggered()
    {
        var contract = CreateContract(tickSize: 1, multiplier: 10, roundTripFee: 6, stopTicks: 5, slippageTicks: 0, lots: 1);
        var config = new RiskConfig { AccountEquity = 100_000 };

        var result = new ProductFilter().Evaluate(contract, config);

        Assert.Equal(ProductFilterStatus.Allowed, result.Status);
        Assert.Contains(result.Reasons, r => r.Contains("可接受", StringComparison.Ordinal));
    }

    [Fact]
    public void Rejected_With_Reasons_When_Input_Invalid()
    {
        var contract = CreateContract(price: -1);
        var config = new RiskConfig { AccountEquity = 10_000 };

        var result = new ProductFilter().Evaluate(contract, config);

        Assert.Equal(ProductFilterStatus.Rejected, result.Status);
        Assert.Contains(result.Reasons, r => r.Contains("价格", StringComparison.Ordinal));
    }

    private static FuturesContract CreateContract(
        double price = 2500,
        double multiplier = 10,
        double marginRate = 0.1,
        double tickSize = 1,
        double roundTripFee = 6,
        int stopTicks = 10,
        int slippageTicks = 2,
        int lots = 1)
    {
        return new FuturesContract
        {
            Exchange = "TEST",
            ProductCode = "MA",
            ContractCode = "MA2601",
            ProductName = "甲醇",
            Price = price,
            Multiplier = multiplier,
            TickSize = tickSize,
            MarginRate = marginRate,
            RoundTripFee = roundTripFee,
            StopTicks = stopTicks,
            SlippageTicks = slippageTicks,
            Lots = lots,
        };
    }
}
