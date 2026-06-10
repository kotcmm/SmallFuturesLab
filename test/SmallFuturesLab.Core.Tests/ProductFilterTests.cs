namespace SmallFuturesLab.Core.Tests;

public sealed class ProductFilterTests
{
    [Fact]
    public void Product_TickValue_Equals_TickSize_Times_Multiplier()
    {
        var product = CreateProduct(tickSize: 1, multiplier: 10);

        Assert.Equal(10, product.TickValue);
    }

    [Fact]
    public void Product_MarginPerLot_Equals_Price_Times_Multiplier_Times_MarginRate()
    {
        var product = CreateProduct(price: 2500, multiplier: 10, marginRate: 0.1);

        Assert.Equal(2500, product.MarginPerLot);
    }

    [Fact]
    public void ProductRisk_MarginMoney_Equals_MarginPerLot_Times_Lots()
    {
        var product = CreateProduct(price: 2500, multiplier: 10, marginRate: 0.1);
        var config = new RiskConfig { AccountEquity = 10_000 };
        var condition = new FilterCondition { StopTicks = 10, SlippageTicks = 2, Lots = 2 };
        var risk = new ProductRisk { Product = product, RiskConfig = config, Condition = condition };

        Assert.Equal(5000, risk.MarginMoney);
    }

    [Fact]
    public void ProductRisk_MarginRate_Equals_MarginMoney_Divided_By_AccountEquity()
    {
        var product = CreateProduct(price: 2500, multiplier: 10, marginRate: 0.1);
        var config = new RiskConfig { AccountEquity = 10_000 };
        var condition = new FilterCondition { StopTicks = 10, SlippageTicks = 2, Lots = 1 };
        var risk = new ProductRisk { Product = product, RiskConfig = config, Condition = condition };

        Assert.Equal(0.25, risk.MarginRate, 6);
    }

    [Fact]
    public void ProductRisk_StopRiskMoney_Equals_StopTicks_Times_TickValue_Times_Lots()
    {
        var product = CreateProduct(tickSize: 1, multiplier: 10);
        var config = new RiskConfig { AccountEquity = 10_000 };
        var condition = new FilterCondition { StopTicks = 10, SlippageTicks = 2, Lots = 2 };
        var risk = new ProductRisk { Product = product, RiskConfig = config, Condition = condition };

        Assert.Equal(200, risk.StopRiskMoney);
    }

    [Fact]
    public void ProductRisk_SlippageMoney_Equals_SlippageTicks_Times_TickValue_Times_Lots()
    {
        var product = CreateProduct(tickSize: 1, multiplier: 10);
        var config = new RiskConfig { AccountEquity = 10_000 };
        var condition = new FilterCondition { StopTicks = 10, SlippageTicks = 2, Lots = 2 };
        var risk = new ProductRisk { Product = product, RiskConfig = config, Condition = condition };

        Assert.Equal(40, risk.SlippageMoney);
    }

    [Fact]
    public void ProductRisk_CostMoney_Equals_RoundTripFee_Times_Lots_Plus_SlippageMoney()
    {
        var product = CreateProduct(tickSize: 1, multiplier: 10, roundTripFee: 6);
        var config = new RiskConfig { AccountEquity = 10_000 };
        var condition = new FilterCondition { StopTicks = 10, SlippageTicks = 2, Lots = 1 };
        var risk = new ProductRisk { Product = product, RiskConfig = config, Condition = condition };

        Assert.Equal(26, risk.CostMoney);
    }

    [Fact]
    public void ProductRisk_TotalRiskMoney_Equals_StopRiskMoney_Plus_CostMoney()
    {
        var product = CreateProduct(tickSize: 1, multiplier: 10, roundTripFee: 6);
        var config = new RiskConfig { AccountEquity = 10_000 };
        var condition = new FilterCondition { StopTicks = 10, SlippageTicks = 2, Lots = 1 };
        var risk = new ProductRisk { Product = product, RiskConfig = config, Condition = condition };

        Assert.Equal(126, risk.TotalRiskMoney);
    }

    [Fact]
    public void ProductRisk_RiskRate_Equals_TotalRiskMoney_Divided_By_AccountEquity()
    {
        var product = CreateProduct(tickSize: 1, multiplier: 10, roundTripFee: 6);
        var config = new RiskConfig { AccountEquity = 10_000 };
        var condition = new FilterCondition { StopTicks = 10, SlippageTicks = 2, Lots = 1 };
        var risk = new ProductRisk { Product = product, RiskConfig = config, Condition = condition };

        Assert.Equal(0.0126, risk.RiskRate, 6);
    }

    [Fact]
    public void ProductRisk_CostRatio_Equals_CostMoney_Divided_By_StopRiskMoney()
    {
        var product = CreateProduct(tickSize: 1, multiplier: 10, roundTripFee: 6);
        var config = new RiskConfig { AccountEquity = 10_000 };
        var condition = new FilterCondition { StopTicks = 10, SlippageTicks = 2, Lots = 1 };
        var risk = new ProductRisk { Product = product, RiskConfig = config, Condition = condition };

        Assert.Equal(0.26, risk.CostRatio, 6);
    }

    [Fact]
    public void Rejected_When_RiskRate_Exceeds_RejectRiskRate()
    {
        var product = CreateProduct(tickSize: 1, multiplier: 10, roundTripFee: 6);
        var config = new RiskConfig { AccountEquity = 10_000 };
        var condition = new FilterCondition { StopTicks = 100, SlippageTicks = 0, Lots = 1 };

        var result = new ProductFilter().Evaluate(product, config, condition);

        Assert.Equal(ProductFilterStatus.Rejected, result.Status);
        Assert.Contains(result.Reasons, r => r.Contains("单笔风险", StringComparison.Ordinal));
    }

    [Fact]
    public void Rejected_When_MarginRate_Exceeds_RejectMarginRate()
    {
        var product = CreateProduct(price: 10_000, multiplier: 10, marginRate: 0.6);
        var config = new RiskConfig { AccountEquity = 10_000 };
        var condition = new FilterCondition { StopTicks = 10, SlippageTicks = 2, Lots = 1 };

        var result = new ProductFilter().Evaluate(product, config, condition);

        Assert.Equal(ProductFilterStatus.Rejected, result.Status);
        Assert.Contains(result.Reasons, r => r.Contains("保证金占用", StringComparison.Ordinal));
    }

    [Fact]
    public void Rejected_When_CostRatio_Exceeds_RejectCostRatio()
    {
        var product = CreateProduct(tickSize: 1, multiplier: 10, roundTripFee: 100);
        var config = new RiskConfig { AccountEquity = 100_000 };
        var condition = new FilterCondition { StopTicks = 10, SlippageTicks = 0, Lots = 1 };

        var result = new ProductFilter().Evaluate(product, config, condition);

        Assert.Equal(ProductFilterStatus.Rejected, result.Status);
        Assert.Contains(result.Reasons, r => r.Contains("成本占止损风险", StringComparison.Ordinal));
    }

    [Fact]
    public void Caution_When_RiskRate_Enters_Caution_Zone()
    {
        var product = CreateProduct(tickSize: 1, multiplier: 10, roundTripFee: 6);
        var config = new RiskConfig { AccountEquity = 10_000 };
        var condition = new FilterCondition { StopTicks = 15, SlippageTicks = 0, Lots = 1 };

        var result = new ProductFilter().Evaluate(product, config, condition);

        Assert.Equal(ProductFilterStatus.Caution, result.Status);
        Assert.Contains(result.Reasons, r => r.Contains("单笔风险", StringComparison.Ordinal) && r.Contains("谨慎区", StringComparison.Ordinal));
    }

    [Fact]
    public void Allowed_When_All_Thresholds_Not_Triggered()
    {
        var product = CreateProduct(tickSize: 1, multiplier: 10, roundTripFee: 6);
        var config = new RiskConfig { AccountEquity = 100_000 };
        var condition = new FilterCondition { StopTicks = 5, SlippageTicks = 0, Lots = 1 };

        var result = new ProductFilter().Evaluate(product, config, condition);

        Assert.Equal(ProductFilterStatus.Allowed, result.Status);
        Assert.Contains(result.Reasons, r => r.Contains("可接受", StringComparison.Ordinal));
    }

    [Fact]
    public void Rejected_With_Reasons_When_Product_Input_Invalid()
    {
        var product = CreateProduct(price: -1);
        var config = new RiskConfig { AccountEquity = 10_000 };
        var condition = new FilterCondition { StopTicks = 10, SlippageTicks = 2, Lots = 1 };

        var result = new ProductFilter().Evaluate(product, config, condition);

        Assert.Equal(ProductFilterStatus.Rejected, result.Status);
        Assert.Contains(result.Reasons, r => r.Contains("价格", StringComparison.Ordinal));
    }

    [Fact]
    public void Rejected_With_Reasons_When_RiskConfig_Input_Invalid()
    {
        var product = CreateProduct();
        var config = new RiskConfig { AccountEquity = 0 };
        var condition = new FilterCondition { StopTicks = 10, SlippageTicks = 2, Lots = 1 };

        var result = new ProductFilter().Evaluate(product, config, condition);

        Assert.Equal(ProductFilterStatus.Rejected, result.Status);
        Assert.Contains(result.Reasons, r => r.Contains("账户权益", StringComparison.Ordinal));
    }

    [Fact]
    public void Rejected_With_Reasons_When_FilterCondition_Input_Invalid()
    {
        var product = CreateProduct();
        var config = new RiskConfig { AccountEquity = 10_000 };
        var condition = new FilterCondition { StopTicks = 0, SlippageTicks = 2, Lots = 1 };

        var result = new ProductFilter().Evaluate(product, config, condition);

        Assert.Equal(ProductFilterStatus.Rejected, result.Status);
        Assert.Contains(result.Reasons, r => r.Contains("止损", StringComparison.Ordinal));
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
}
