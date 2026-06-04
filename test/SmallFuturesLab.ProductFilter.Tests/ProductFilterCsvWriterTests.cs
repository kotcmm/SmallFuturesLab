namespace SmallFuturesLab.ProductFilter.Tests;

public class ProductFilterCsvWriterTests
{
    private static readonly string TempFile = Path.Combine(Path.GetTempPath(), $"test_writer_{Guid.NewGuid()}.csv");

    /// <summary>
    /// 写出 CSV 后表头必须与模板一致。
    /// </summary>
    [Fact]
    public void Write_CsvHeaderMatchesTemplate()
    {
        var writer = new ProductFilterCsvWriter();
        var rows = new List<ProductFilterRow>
        {
            CreateMinimalRow(),
        };

        writer.Write(TempFile, rows);
        var lines = File.ReadAllLines(TempFile);
        var expectedHeader = string.Join(",", ProductFilterCsvHeader.ExpectedHeaders);

        Assert.Equal(expectedHeader, lines[0]);
    }

    /// <summary>
    /// 写出 CSV 后内容包含 AccountEquity / RiskRate / Result。
    /// </summary>
    [Fact]
    public void Write_CsvContainsAccountEquityRiskRateResult()
    {
        var writer = new ProductFilterCsvWriter();
        var rows = new List<ProductFilterRow>
        {
            CreateMinimalRow() with { AccountEquity = 15000, RiskRate = 0.012, Result = ProductFilterResultStatus.Caution },
        };

        writer.Write(TempFile, rows);
        var content = File.ReadAllText(TempFile);

        Assert.Contains("15000", content);
        Assert.Contains("0.012", content);
        Assert.Contains("Caution", content);
    }

    /// <summary>
    /// 写出 CSV 时自动创建不存在的目录。
    /// </summary>
    [Fact]
    public void Write_CreatesDirectoryIfNotExists()
    {
        var writer = new ProductFilterCsvWriter();
        var dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var filePath = Path.Combine(dir, "output.csv");
        var rows = new List<ProductFilterRow> { CreateMinimalRow() };

        writer.Write(filePath, rows);

        Assert.True(File.Exists(filePath));
    }

    /// <summary>
    /// 空列表写出后只包含表头。
    /// </summary>
    [Fact]
    public void Write_EmptyListWritesHeaderOnly()
    {
        var writer = new ProductFilterCsvWriter();
        writer.Write(TempFile, new List<ProductFilterRow>());
        var lines = File.ReadAllLines(TempFile);

        Assert.Single(lines);
    }

    private static ProductFilterRow CreateMinimalRow()
    {
        return new ProductFilterRow
        {
            Exchange = "Test",
            ProductName = "A",
            ProductCode = "A",
            ContractCode = "A2501",
            Price = 2500,
            Multiplier = 10,
            TickSize = 1,
            MarginRate = 0.1,
            RoundTripFeePerLot = 6,
            SlippageTicks = 2,
            TypicalAtr = 20,
            StopDistance = 12,
            AccountEquity = 10000,
            LiquidityLevel = LiquidityLevel.Good,
            BookContinuityLevel = BookContinuityLevel.Good,
            RolloverClarity = RolloverClarity.Good,
            DataDate = "2024-01-01",
            DataSource = "Test",
        };
    }
}
