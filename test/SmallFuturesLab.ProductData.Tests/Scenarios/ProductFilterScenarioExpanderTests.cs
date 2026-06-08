using SmallFuturesLab.ProductData.Models;
using SmallFuturesLab.ProductData.Scenarios;
using SmallFuturesLab.ProductFilter;
using Xunit;

namespace SmallFuturesLab.ProductData.Tests;

/// <summary>
/// ProductFilterScenarioExpander 及其相关类型的测试。
/// </summary>
public class ProductFilterScenarioExpanderTests
{
    private static ProductDataRecord CreateCompleteRecord() => new()
    {
        Exchange = "CZCE",
        ProductName = "甲醇",
        ProductCode = "MA",
        ContractCode = "MA2501",
        Price = 2500,
        Multiplier = 10,
        TickSize = 1,
        MarginRate = 0.10,
        RoundTripFeePerLot = 6,
        TypicalAtr = 20,
        LiquidityLevel = LiquidityLevel.Good,
        BookContinuityLevel = BookContinuityLevel.Good,
        RolloverClarity = RolloverClarity.Good,
        DataDate = "2024-01-01",
        DataSource = "测试本地数据",
        NeedsReview = true,
    };

    #region ProductFilterScenarioSet.CreateDefault

    [Fact]
    public void CreateDefault_Generates_10_Scenarios()
    {
        var set = ProductFilterScenarioSet.CreateDefault(tickSize: 1, typicalAtr: 20, slippageTicks: 2);

        Assert.Equal(10, set.Scenarios.Count);
    }

    [Fact]
    public void CreateDefault_Contains_Both_AccountEquity_Values()
    {
        var set = ProductFilterScenarioSet.CreateDefault(tickSize: 1, typicalAtr: 20, slippageTicks: 2);
        var equities = set.Scenarios.Select(s => s.AccountEquity).Distinct().OrderBy(x => x).ToList();

        Assert.Equal(new[] { 10000.0, 20000.0 }, equities);
    }

    [Fact]
    public void CreateDefault_Contains_Five_StopDistance_Types()
    {
        var set = ProductFilterScenarioSet.CreateDefault(tickSize: 1, typicalAtr: 20, slippageTicks: 2);
        var names = set.Scenarios.Select(s => s.Name).ToList();

        Assert.Contains("3tick_10000", names);
        Assert.Contains("5tick_10000", names);
        Assert.Contains("10tick_10000", names);
        Assert.Contains("0.5atr_10000", names);
        Assert.Contains("1.0atr_10000", names);
    }

    [Fact]
    public void CreateDefault_Throws_When_TickSize_Not_Positive()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            ProductFilterScenarioSet.CreateDefault(tickSize: 0, typicalAtr: 20, slippageTicks: 2));
        Assert.Contains("tickSize", ex.Message);
    }

    [Fact]
    public void CreateDefault_Throws_When_TypicalAtr_Not_Positive()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            ProductFilterScenarioSet.CreateDefault(tickSize: 1, typicalAtr: 0, slippageTicks: 2));
        Assert.Contains("typicalAtr", ex.Message);
    }

    [Fact]
    public void CreateDefault_Throws_When_SlippageTicks_Negative()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            ProductFilterScenarioSet.CreateDefault(tickSize: 1, typicalAtr: 20, slippageTicks: -1));
        Assert.Contains("slippageTicks", ex.Message);
    }

    #endregion

    #region ProductFilterScenario validation

    [Fact]
    public void Scenario_Throws_When_Name_Empty()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            new ProductFilterScenario { Name = "", AccountEquity = 10000, StopDistance = 10, SlippageTicks = 2 });
        Assert.Contains("Name", ex.Message);
    }

    [Fact]
    public void Scenario_Throws_When_AccountEquity_Not_Positive()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            new ProductFilterScenario { Name = "test", AccountEquity = 0, StopDistance = 10, SlippageTicks = 2 });
        Assert.Contains("AccountEquity", ex.Message);
    }

    [Fact]
    public void Scenario_Throws_When_StopDistance_Not_Positive()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            new ProductFilterScenario { Name = "test", AccountEquity = 10000, StopDistance = 0, SlippageTicks = 2 });
        Assert.Contains("StopDistance", ex.Message);
    }

    [Fact]
    public void Scenario_Throws_When_SlippageTicks_Negative()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            new ProductFilterScenario { Name = "test", AccountEquity = 10000, StopDistance = 10, SlippageTicks = -1 });
        Assert.Contains("SlippageTicks", ex.Message);
    }

    #endregion

    #region ProductFilterScenarioExpander

    [Fact]
    public void Expander_Expands_Complete_Record_Into_10_Rows()
    {
        var record = CreateCompleteRecord();
        var set = ProductFilterScenarioSet.CreateDefault(tickSize: 1, typicalAtr: 20, slippageTicks: 2);
        var expander = new ProductFilterScenarioExpander();

        var result = expander.Expand(record, set);

        Assert.Equal(10, result.Rows.Count);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Each_Row_Preserves_Core_Fields()
    {
        var record = CreateCompleteRecord();
        var set = ProductFilterScenarioSet.CreateDefault(tickSize: 1, typicalAtr: 20, slippageTicks: 2);
        var expander = new ProductFilterScenarioExpander();

        var result = expander.Expand(record, set);

        Assert.All(result.Rows, row =>
        {
            Assert.Equal("MA", row.ProductCode);
            Assert.Equal("MA2501", row.ContractCode);
            Assert.Equal(2500, row.Price);
            Assert.Equal(10, row.Multiplier);
            Assert.Equal(1, row.TickSize);
            Assert.Equal(0.10, row.MarginRate);
            Assert.Equal(6, row.RoundTripFeePerLot);
        });
    }

    [Fact]
    public void Each_Row_Has_Varying_AccountEquity_And_StopDistance()
    {
        var record = CreateCompleteRecord();
        var set = ProductFilterScenarioSet.CreateDefault(tickSize: 1, typicalAtr: 20, slippageTicks: 2);
        var expander = new ProductFilterScenarioExpander();

        var result = expander.Expand(record, set);

        var equities = result.Rows.Select(r => r.AccountEquity).Distinct().OrderBy(x => x).ToList();
        var distances = result.Rows.Select(r => r.StopDistance).Distinct().OrderBy(x => x).ToList();

        Assert.Equal(new[] { 10000.0, 20000.0 }, equities);
        Assert.Equal(new[] { 3.0, 5.0, 10.0, 20.0 }, distances);
    }

    [Fact]
    public void Each_Row_Preserves_Liquidity_Fields()
    {
        var record = CreateCompleteRecord();
        var set = ProductFilterScenarioSet.CreateDefault(tickSize: 1, typicalAtr: 20, slippageTicks: 2);
        var expander = new ProductFilterScenarioExpander();

        var result = expander.Expand(record, set);

        Assert.All(result.Rows, row =>
        {
            Assert.Equal(LiquidityLevel.Good, row.LiquidityLevel);
            Assert.Equal(BookContinuityLevel.Good, row.BookContinuityLevel);
            Assert.Equal(RolloverClarity.Good, row.RolloverClarity);
        });
    }

    [Fact]
    public void NeedsReview_True_Adds_Review_Reason()
    {
        var record = CreateCompleteRecord();
        var set = ProductFilterScenarioSet.CreateDefault(tickSize: 1, typicalAtr: 20, slippageTicks: 2);
        var expander = new ProductFilterScenarioExpander();

        var result = expander.Expand(record, set);

        Assert.All(result.Rows, row =>
        {
            Assert.Contains("数据需复核", row.Reasons);
        });
    }

    [Fact]
    public void Normalization_Failure_Returns_ExpandError()
    {
        var record = CreateCompleteRecord() with { Price = -1 };
        var set = ProductFilterScenarioSet.CreateDefault(tickSize: 1, typicalAtr: 20, slippageTicks: 2);
        var expander = new ProductFilterScenarioExpander();

        var result = expander.Expand(record, set);

        Assert.Empty(result.Rows);
        Assert.Equal(10, result.Errors.Count);
        Assert.All(result.Errors, e =>
        {
            Assert.Equal("MA", e.ProductCode);
            Assert.Equal("MA2501", e.ContractCode);
            Assert.NotEmpty(e.ScenarioName);
            Assert.NotEmpty(e.Reason);
        });
    }

    [Fact]
    public void Single_Scenario_Failure_Does_Not_Block_Others()
    {
        var record = CreateCompleteRecord() with { ProductCode = "" };
        var set = ProductFilterScenarioSet.CreateDefault(tickSize: 1, typicalAtr: 20, slippageTicks: 2);
        var expander = new ProductFilterScenarioExpander();

        var result = expander.Expand(record, set);

        Assert.Empty(result.Rows);
        Assert.Equal(10, result.Errors.Count);
        Assert.All(result.Errors, e => Assert.Contains("ProductCode", e.Reason));
    }

    [Fact]
    public void Expander_Does_Not_Produce_Allowed_Caution_Rejected()
    {
        var record = CreateCompleteRecord();
        var set = ProductFilterScenarioSet.CreateDefault(tickSize: 1, typicalAtr: 20, slippageTicks: 2);
        var expander = new ProductFilterScenarioExpander();

        var result = expander.Expand(record, set);

        Assert.All(result.Rows, row =>
        {
            // Result 仍为默认值，说明展开器未写入 Allowed / Caution / Rejected
            Assert.Equal(default(ProductFilterResultStatus), row.Result);
            Assert.Equal(0, row.TickValue);
            Assert.Equal(0, row.MarginPerLot);
            Assert.Equal(0, row.AtrMoneyPerLot);
            Assert.Equal(0, row.StopRiskMoney);
            Assert.Equal(0, row.RiskRate);
            Assert.Equal(0, row.MarginRateOfEquity);
            Assert.Equal(0, row.CostRatio);
        });
    }

    [Fact]
    public void Expander_Does_Not_Generate_Trading_Advice()
    {
        var record = CreateCompleteRecord() with { NeedsReview = false };
        var set = ProductFilterScenarioSet.CreateDefault(tickSize: 1, typicalAtr: 20, slippageTicks: 2);
        var expander = new ProductFilterScenarioExpander();

        var result = expander.Expand(record, set);

        Assert.All(result.Rows, row =>
        {
            Assert.DoesNotContain("进入后续周期研究", row.Reasons);
            Assert.DoesNotContain("谨慎观察", row.Reasons);
            Assert.DoesNotContain("当前账户规模排除", row.Reasons);
            Assert.DoesNotContain("Allowed", row.Reasons);
            Assert.DoesNotContain("Caution", row.Reasons);
            Assert.DoesNotContain("Rejected", row.Reasons);
        });
    }

    [Fact]
    public void Expander_Does_Not_Manually_Calculate_Formula_Fields()
    {
        var record = CreateCompleteRecord();
        var set = ProductFilterScenarioSet.CreateDefault(tickSize: 1, typicalAtr: 20, slippageTicks: 2);
        var expander = new ProductFilterScenarioExpander();

        var result = expander.Expand(record, set);

        Assert.All(result.Rows, row =>
        {
            Assert.Equal(0, row.TickValue);
            Assert.Equal(0, row.MarginPerLot);
            Assert.Equal(0, row.AtrMoneyPerLot);
            Assert.Equal(0, row.StopRiskMoney);
            Assert.Equal(0, row.SlippageMoney);
            Assert.Equal(0, row.CostMoney);
            Assert.Equal(0, row.TotalRiskMoney);
            Assert.Equal(0, row.RiskRate);
            Assert.Equal(0, row.MarginRateOfEquity);
            Assert.Equal(0, row.CostRatio);
        });
    }

    #endregion
}
