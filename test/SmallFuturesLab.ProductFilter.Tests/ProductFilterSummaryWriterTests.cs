namespace SmallFuturesLab.ProductFilter.Tests;

public class ProductFilterSummaryWriterTests
{
    /// <summary>
    /// Summary 能按 AccountEquity 分组统计 Allowed / Caution / Rejected 数量。
    /// </summary>
    [Fact]
    public void WriteSummary_CountsResultsByAccountEquity()
    {
        var results = new List<ProductFilterCalculationResult>
        {
            CreateResult("A", 10000, ProductFilterResultStatus.Allowed),
            CreateResult("A", 20000, ProductFilterResultStatus.Allowed),
            CreateResult("B", 10000, ProductFilterResultStatus.Caution),
            CreateResult("B", 20000, ProductFilterResultStatus.Allowed),
            CreateResult("C", 10000, ProductFilterResultStatus.Rejected),
            CreateResult("C", 20000, ProductFilterResultStatus.Caution),
        };

        var writer = new ProductFilterSummaryWriter();
        var summary = writer.GenerateSummary(results);

        Assert.Equal(6, summary.TotalRecords);
        Assert.Equal(3, summary.UniqueProducts);

        Assert.Equal(1, summary.ByAccountEquity[10000].AllowedCount);
        Assert.Equal(1, summary.ByAccountEquity[10000].CautionCount);
        Assert.Equal(1, summary.ByAccountEquity[10000].RejectedCount);

        Assert.Equal(2, summary.ByAccountEquity[20000].AllowedCount);
        Assert.Equal(1, summary.ByAccountEquity[20000].CautionCount);
        Assert.Equal(0, summary.ByAccountEquity[20000].RejectedCount);
    }

    /// <summary>
    /// Summary 能列出需要复核的数据。
    /// </summary>
    [Fact]
    public void WriteSummary_ListsRecordsNeedingReview()
    {
        var results = new List<ProductFilterCalculationResult>
        {
            CreateResult("A", 10000, ProductFilterResultStatus.Allowed, LiquidityLevel.Unknown),
            CreateResult("B", 20000, ProductFilterResultStatus.Caution, LiquidityLevel.Unknown),
        };

        var writer = new ProductFilterSummaryWriter();
        var summary = writer.GenerateSummary(results);

        Assert.Equal(2, summary.NeedsReview.Count);
    }

    /// <summary>
    /// Summary Markdown 输出必须包含统计和列表。
    /// </summary>
    [Fact]
    public void WriteSummary_MarkdownContainsRequiredSections()
    {
        var results = new List<ProductFilterCalculationResult>
        {
            CreateResult("A", 10000, ProductFilterResultStatus.Allowed),
        };

        var writer = new ProductFilterSummaryWriter();
        var markdown = writer.WriteMarkdown(results);

        Assert.Contains("总记录数", markdown);
        Assert.Contains("进入后续周期研究", markdown);
        Assert.Contains("谨慎观察", markdown);
        Assert.Contains("当前账户规模排除", markdown);
    }

    /// <summary>
    /// Summary 不得宣称任何品种可以实盘交易。
    /// </summary>
    [Fact]
    public void WriteSummary_DoesNotContainTradingAdvice()
    {
        var results = new List<ProductFilterCalculationResult>
        {
            CreateResult("A", 10000, ProductFilterResultStatus.Allowed),
        };

        var writer = new ProductFilterSummaryWriter();
        var markdown = writer.WriteMarkdown(results);

        Assert.DoesNotContain("推荐交易", markdown);
        Assert.DoesNotContain("可以买入", markdown);
        Assert.DoesNotContain("可以做多", markdown);
        Assert.DoesNotContain("可以做空", markdown);
    }

    /// <summary>
    /// Markdown 中不包含生成时间，不依赖当前时间。
    /// </summary>
    [Fact]
    public void WriteSummary_DoesNotContainGenerationTime()
    {
        var results = new List<ProductFilterCalculationResult>
        {
            CreateResult("A", 10000, ProductFilterResultStatus.Allowed),
        };

        var writer = new ProductFilterSummaryWriter();
        var markdown = writer.WriteMarkdown(results);

        Assert.DoesNotContain("生成时间", markdown);
        Assert.DoesNotContain("UTC", markdown);
    }

    private static ProductFilterCalculationResult CreateResult(
        string name,
        double accountEquity,
        ProductFilterResultStatus result,
        LiquidityLevel liquidity = LiquidityLevel.Good)
    {
        var row = new ProductFilterRow
        {
            Exchange = "Test",
            ProductName = name,
            ProductCode = name,
            ContractCode = $"{name}2501",
            Price = 2500,
            Multiplier = 10,
            TickSize = 1,
            MarginRate = 0.10,
            RoundTripFeePerLot = 6,
            SlippageTicks = 2,
            TypicalAtr = 20,
            StopDistance = 12,
            AccountEquity = accountEquity,
            LiquidityLevel = liquidity,
            BookContinuityLevel = BookContinuityLevel.Good,
            RolloverClarity = RolloverClarity.Good,
            DataDate = "2024-01-01",
            DataSource = "Test",
        };
        return new ProductFilterCalculationResult(row, result, "测试原因");
    }
}
