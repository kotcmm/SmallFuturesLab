namespace SmallFuturesLab.ProductFilter.Tests;

public class ProductFilterSummaryWriterTests
{
    /// <summary>
    /// Summary 能统计 Allowed / Caution / Rejected 数量。
    /// </summary>
    [Fact]
    public void WriteSummary_CountsResultsCorrectly()
    {
        var results = new List<ProductFilterCalculationResult>
        {
            CreateResult("A", ProductFilterResultStatus.Allowed, ProductFilterResultStatus.Allowed),
            CreateResult("B", ProductFilterResultStatus.Caution, ProductFilterResultStatus.Allowed),
            CreateResult("C", ProductFilterResultStatus.Rejected, ProductFilterResultStatus.Caution),
        };

        var writer = new ProductFilterSummaryWriter();
        var summary = writer.GenerateSummary(results);

        Assert.Equal(3, summary.TotalRecords);
        Assert.Equal(1, summary.AllowedCount10k);
        Assert.Equal(1, summary.CautionCount10k);
        Assert.Equal(1, summary.RejectedCount10k);
        Assert.Equal(2, summary.AllowedCount20k);
        Assert.Equal(1, summary.CautionCount20k);
        Assert.Equal(0, summary.RejectedCount20k);
    }

    /// <summary>
    /// Summary 能列出需要复核的数据。
    /// </summary>
    [Fact]
    public void WriteSummary_ListsRecordsNeedingReview()
    {
        var results = new List<ProductFilterCalculationResult>
        {
            CreateResult("A", ProductFilterResultStatus.Allowed, ProductFilterResultStatus.Allowed, LiquidityLevel.Unknown),
            CreateResult("B", ProductFilterResultStatus.Caution, ProductFilterResultStatus.Caution, LiquidityLevel.Unknown),
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
            CreateResult("A", ProductFilterResultStatus.Allowed, ProductFilterResultStatus.Allowed),
        };

        var writer = new ProductFilterSummaryWriter();
        var markdown = writer.WriteMarkdown(results);

        Assert.Contains("总记录数", markdown);
        Assert.Contains("10,000 元账户", markdown);
        Assert.Contains("20,000 元账户", markdown);
    }

    /// <summary>
    /// Summary 不得宣称任何品种可以实盘交易。
    /// </summary>
    [Fact]
    public void WriteSummary_DoesNotContainTradingAdvice()
    {
        var results = new List<ProductFilterCalculationResult>
        {
            CreateResult("A", ProductFilterResultStatus.Allowed, ProductFilterResultStatus.Allowed),
        };

        var writer = new ProductFilterSummaryWriter();
        var markdown = writer.WriteMarkdown(results);

        Assert.DoesNotContain("推荐交易", markdown);
        Assert.DoesNotContain("可以买入", markdown);
        Assert.DoesNotContain("可以做多", markdown);
        Assert.DoesNotContain("可以做空", markdown);
    }

    private static ProductFilterCalculationResult CreateResult(
        string name,
        ProductFilterResultStatus result10k,
        ProductFilterResultStatus result20k,
        LiquidityLevel liquidity = LiquidityLevel.Good)
    {
        var row = new ProductFilterRow
        {
            Exchange = "Test",
            ProductName = name,
            ProductCode = "T",
            ContractCode = "T2501",
            Price = 2500,
            Multiplier = 10,
            TickSize = 1,
            MarginRate = 0.10,
            RoundTripFeePerLot = 6,
            SlippageTicks = 2,
            TypicalAtr = 20,
            StopDistance = 12,
            LiquidityLevel = liquidity,
            BookContinuityLevel = BookContinuityLevel.Good,
            RolloverClarity = RolloverClarity.Good,
            DataDate = "2024-01-01",
            DataSource = "Test",
        };
        return new ProductFilterCalculationResult(row, result10k, result20k, "测试原因");
    }
}
