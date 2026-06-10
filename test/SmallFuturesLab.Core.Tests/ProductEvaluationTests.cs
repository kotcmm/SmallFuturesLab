namespace SmallFuturesLab.Core.Tests;

public sealed class ProductEvaluationTests
{
    [Fact]
    public void TickValue_Equals_TickSize_Times_Multiplier()
    {
        var product = CreateProduct(tickSize: 1, multiplier: 10);
        var eval = new ProductEvaluation(product, 10_000, CreateCondition());

        Assert.Equal(10, eval.TickValue);
    }

    [Fact]
    public void MarginPerLot_Equals_Price_Times_Multiplier_Times_MarginRate()
    {
        var product = CreateProduct(price: 2500, multiplier: 10, marginRate: 0.1);
        var eval = new ProductEvaluation(product, 10_000, CreateCondition());

        Assert.Equal(2500, eval.MarginPerLot);
    }

    [Fact]
    public void MarginRateOfEquity_Equals_MarginPerLot_Times_Lots_Divided_By_AccountEquity()
    {
        var product = CreateProduct(price: 2500, multiplier: 10, marginRate: 0.1);
        var condition = CreateCondition(lots: 2);
        var eval = new ProductEvaluation(product, 10_000, condition);

        Assert.Equal(0.5, eval.MarginRateOfEquity, 6);
    }

    [Fact]
    public void StopRiskMoney_Equals_StopTicks_Times_TickValue_Times_Lots()
    {
        var product = CreateProduct(tickSize: 1, multiplier: 10);
        var condition = CreateCondition(stopTicks: 10, lots: 2);
        var eval = new ProductEvaluation(product, 10_000, condition);

        Assert.Equal(200, eval.StopRiskMoney);
    }

    [Fact]
    public void SlippageMoney_Equals_SlippageTicks_Times_TickValue_Times_Lots()
    {
        var product = CreateProduct(tickSize: 1, multiplier: 10);
        var condition = CreateCondition(slippageTicks: 2, lots: 2);
        var eval = new ProductEvaluation(product, 10_000, condition);

        Assert.Equal(40, eval.SlippageMoney);
    }

    [Fact]
    public void CostMoney_Equals_RoundTripFee_Times_Lots_Plus_SlippageMoney()
    {
        var product = CreateProduct(tickSize: 1, multiplier: 10, roundTripFee: 6);
        var condition = CreateCondition(slippageTicks: 2, lots: 1);
        var eval = new ProductEvaluation(product, 10_000, condition);

        Assert.Equal(26, eval.CostMoney);
    }

    [Fact]
    public void TotalRiskMoney_Equals_StopRiskMoney_Plus_CostMoney()
    {
        var product = CreateProduct(tickSize: 1, multiplier: 10, roundTripFee: 6);
        var condition = CreateCondition(stopTicks: 10, slippageTicks: 2, lots: 1);
        var eval = new ProductEvaluation(product, 10_000, condition);

        Assert.Equal(126, eval.TotalRiskMoney);
    }

    [Fact]
    public void RiskRate_Equals_TotalRiskMoney_Divided_By_AccountEquity()
    {
        var product = CreateProduct(tickSize: 1, multiplier: 10, roundTripFee: 6);
        var condition = CreateCondition(stopTicks: 10, slippageTicks: 2, lots: 1);
        var eval = new ProductEvaluation(product, 10_000, condition);

        Assert.Equal(0.0126, eval.RiskRate, 6);
    }

    [Fact]
    public void CostRatio_Equals_CostMoney_Divided_By_StopRiskMoney()
    {
        var product = CreateProduct(tickSize: 1, multiplier: 10, roundTripFee: 6);
        var condition = CreateCondition(stopTicks: 10, slippageTicks: 2, lots: 1);
        var eval = new ProductEvaluation(product, 10_000, condition);

        Assert.Equal(0.26, eval.CostRatio, 6);
    }

    [Fact]
    public void Evaluate_Returns_Rejected_When_RiskRate_Exceeds_RejectRiskRate()
    {
        var product = CreateProduct(tickSize: 1, multiplier: 10, roundTripFee: 6);
        var condition = CreateCondition(stopTicks: 100, slippageTicks: 0, lots: 1);
        var eval = new ProductEvaluation(product, 10_000, condition);
        var accountConfig = new AccountRiskConfig { AccountEquity = 10_000 };

        Assert.Equal(ProductEvaluationStatus.Rejected, eval.Evaluate(accountConfig));
    }

    [Fact]
    public void Evaluate_Returns_Rejected_When_MarginRateOfEquity_Exceeds_RejectMarginRate()
    {
        var product = CreateProduct(price: 10_000, multiplier: 10, marginRate: 0.6);
        var condition = CreateCondition(stopTicks: 10, slippageTicks: 2, lots: 1);
        var eval = new ProductEvaluation(product, 10_000, condition);
        var accountConfig = new AccountRiskConfig { AccountEquity = 10_000 };

        Assert.Equal(ProductEvaluationStatus.Rejected, eval.Evaluate(accountConfig));
    }

    [Fact]
    public void Evaluate_Returns_Rejected_When_CostRatio_Exceeds_RejectCostRatio()
    {
        var product = CreateProduct(tickSize: 1, multiplier: 10, roundTripFee: 100);
        var condition = CreateCondition(stopTicks: 10, slippageTicks: 0, lots: 1);
        var eval = new ProductEvaluation(product, 100_000, condition);
        var accountConfig = new AccountRiskConfig { AccountEquity = 100_000 };

        Assert.Equal(ProductEvaluationStatus.Rejected, eval.Evaluate(accountConfig));
    }

    [Fact]
    public void Evaluate_Returns_Caution_When_RiskRate_Enters_Caution_Zone()
    {
        var product = CreateProduct(tickSize: 1, multiplier: 10, roundTripFee: 6);
        var condition = CreateCondition(stopTicks: 15, slippageTicks: 0, lots: 1);
        var eval = new ProductEvaluation(product, 10_000, condition);
        var accountConfig = new AccountRiskConfig { AccountEquity = 10_000 };

        Assert.Equal(ProductEvaluationStatus.Caution, eval.Evaluate(accountConfig));
    }

    [Fact]
    public void Evaluate_Returns_Allowed_When_All_Thresholds_Not_Triggered()
    {
        var product = CreateProduct(tickSize: 1, multiplier: 10, roundTripFee: 6);
        var condition = CreateCondition(stopTicks: 5, slippageTicks: 0, lots: 1);
        var eval = new ProductEvaluation(product, 100_000, condition);
        var accountConfig = new AccountRiskConfig { AccountEquity = 100_000 };

        Assert.Equal(ProductEvaluationStatus.Allowed, eval.Evaluate(accountConfig));
    }

    private static Product CreateProduct(
        double price = 2500,
        double multiplier = 10,
        double marginRate = 0.1,
        double tickSize = 1,
        double roundTripFee = 6)
    {
        return new Product
        {
            Exchange = "TEST",
            Code = "MA",
            Contract = "MA2601",
            Name = "甲醇",
            Price = price,
            Multiplier = multiplier,
            TickSize = tickSize,
            MarginRate = marginRate,
            RoundTripFee = roundTripFee,
        };
    }

    private static FilterCondition CreateCondition(
        int stopTicks = 10,
        int slippageTicks = 2,
        int lots = 1)
    {
        return new FilterCondition
        {
            StopTicks = stopTicks,
            SlippageTicks = slippageTicks,
            Lots = lots,
        };
    }
}
