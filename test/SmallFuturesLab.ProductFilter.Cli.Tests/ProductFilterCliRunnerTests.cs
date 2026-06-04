namespace SmallFuturesLab.ProductFilter.Cli.Tests;

public class ProductFilterCliRunnerTests
{
    private static string CreateTempDir() => Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

    private static string CreateValidCsv(string dir)
    {
        Directory.CreateDirectory(dir);
        var path = Path.Combine(dir, "input.csv");
        var lines = new[]
        {
            "Exchange,ProductName,ProductCode,ContractCode,Price,Multiplier,TickSize,TickValue,MarginRate,MarginPerLot,RoundTripFeePerLot,SlippageTicks,TypicalAtr,AtrMoneyPerLot,StopDistance,StopRiskMoney,SlippageMoney,CostMoney,TotalRiskMoney,AccountEquity,RiskRate,MarginRateOfEquity,CostRatio,LiquidityLevel,BookContinuityLevel,RolloverClarity,Result,Reasons,DataDate,DataSource",
            "TestExchange,TestA,TA,TA2501,2500,10,1,,0.10,,6,2,20,,13,,,,,10000,,,,Good,Good,Good,,,2024-01-01,Test",
            "TestExchange,TestA,TA,TA2501,2500,10,1,,0.10,,6,2,20,,13,,,,,20000,,,,Good,Good,Good,,,2024-01-01,Test",
        };
        File.WriteAllLines(path, lines);
        return path;
    }

    private static string CreateInvalidCsv(string dir)
    {
        Directory.CreateDirectory(dir);
        var path = Path.Combine(dir, "invalid.csv");
        var lines = new[]
        {
            "Exchange,ProductName,ProductCode,ContractCode,Price,Multiplier,TickSize,TickValue,MarginRate,MarginPerLot,RoundTripFeePerLot,SlippageTicks,TypicalAtr,AtrMoneyPerLot,StopDistance,StopRiskMoney,SlippageMoney,CostMoney,TotalRiskMoney,AccountEquity,RiskRate,MarginRateOfEquity,CostRatio,LiquidityLevel,BookContinuityLevel,RolloverClarity,Result,Reasons,DataDate,DataSource",
            "TestExchange,TestA,TA,TA2501,-2500,10,1,,0.10,,6,2,20,,12,,,,,10000,,,,Good,Good,Good,,,2024-01-01,Test",
        };
        File.WriteAllLines(path, lines);
        return path;
    }

    /// <summary>
    /// 缺少 input 参数时返回失败并输出可读错误。
    /// </summary>
    [Fact]
    public void Run_MissingInput_ReturnsFailure()
    {
        var runner = new ProductFilterCliRunner();
        var outputDir = CreateTempDir();
        var exitCode = runner.Run(new[]
        {
            "product-filter", "run",
            "--output", Path.Combine(outputDir, "out.csv"),
            "--summary", Path.Combine(outputDir, "sum.md"),
        });

        Assert.NotEqual(0, exitCode);
    }

    /// <summary>
    /// 缺少 output 参数时返回失败并输出可读错误。
    /// </summary>
    [Fact]
    public void Run_MissingOutput_ReturnsFailure()
    {
        var runner = new ProductFilterCliRunner();
        var inputDir = CreateTempDir();
        var inputPath = CreateValidCsv(inputDir);
        var exitCode = runner.Run(new[]
        {
            "product-filter", "run",
            "--input", inputPath,
            "--summary", Path.Combine(inputDir, "sum.md"),
        });

        Assert.NotEqual(0, exitCode);
    }

    /// <summary>
    /// 缺少 summary 参数时返回失败并输出可读错误。
    /// </summary>
    [Fact]
    public void Run_MissingSummary_ReturnsFailure()
    {
        var runner = new ProductFilterCliRunner();
        var inputDir = CreateTempDir();
        var inputPath = CreateValidCsv(inputDir);
        var exitCode = runner.Run(new[]
        {
            "product-filter", "run",
            "--input", inputPath,
            "--output", Path.Combine(inputDir, "out.csv"),
        });

        Assert.NotEqual(0, exitCode);
    }

    /// <summary>
    /// input 文件不存在时返回失败并输出可读错误。
    /// </summary>
    [Fact]
    public void Run_InputNotFound_ReturnsFailure()
    {
        var runner = new ProductFilterCliRunner();
        var dir = CreateTempDir();
        var exitCode = runner.Run(new[]
        {
            "product-filter", "run",
            "--input", Path.Combine(dir, "not_exist.csv"),
            "--output", Path.Combine(dir, "out.csv"),
            "--summary", Path.Combine(dir, "sum.md"),
        });

        Assert.NotEqual(0, exitCode);
    }

    /// <summary>
    /// 有效 CSV 可以生成 output CSV 文件。
    /// </summary>
    [Fact]
    public void Run_ValidCsv_GeneratesOutputCsv()
    {
        var runner = new ProductFilterCliRunner();
        var dir = CreateTempDir();
        var inputPath = CreateValidCsv(dir);
        var outputPath = Path.Combine(dir, "out.csv");
        var summaryPath = Path.Combine(dir, "sum.md");

        var exitCode = runner.Run(new[]
        {
            "product-filter", "run",
            "--input", inputPath,
            "--output", outputPath,
            "--summary", summaryPath,
        });

        Assert.Equal(0, exitCode);
        Assert.True(File.Exists(outputPath));
    }

    /// <summary>
    /// 有效 CSV 可以生成 summary markdown 文件。
    /// </summary>
    [Fact]
    public void Run_ValidCsv_GeneratesSummaryMarkdown()
    {
        var runner = new ProductFilterCliRunner();
        var dir = CreateTempDir();
        var inputPath = CreateValidCsv(dir);
        var outputPath = Path.Combine(dir, "out.csv");
        var summaryPath = Path.Combine(dir, "sum.md");

        var exitCode = runner.Run(new[]
        {
            "product-filter", "run",
            "--input", inputPath,
            "--output", outputPath,
            "--summary", summaryPath,
        });

        Assert.Equal(0, exitCode);
        Assert.True(File.Exists(summaryPath));
    }

    /// <summary>
    /// 无效 CSV 返回失败并输出所有错误。
    /// </summary>
    [Fact]
    public void Run_InvalidCsv_ReturnsFailureWithErrors()
    {
        var runner = new ProductFilterCliRunner();
        var dir = CreateTempDir();
        var inputPath = CreateInvalidCsv(dir);
        var outputPath = Path.Combine(dir, "out.csv");
        var summaryPath = Path.Combine(dir, "sum.md");

        var exitCode = runner.Run(new[]
        {
            "product-filter", "run",
            "--input", inputPath,
            "--output", outputPath,
            "--summary", summaryPath,
        });

        Assert.NotEqual(0, exitCode);
        Assert.False(File.Exists(outputPath));
        Assert.False(File.Exists(summaryPath));
    }

    /// <summary>
    /// 输出 CSV 表头必须与模板一致。
    /// </summary>
    [Fact]
    public void Run_OutputCsvHeaderMatchesTemplate()
    {
        var runner = new ProductFilterCliRunner();
        var dir = CreateTempDir();
        var inputPath = CreateValidCsv(dir);
        var outputPath = Path.Combine(dir, "out.csv");
        var summaryPath = Path.Combine(dir, "sum.md");

        runner.Run(new[]
        {
            "product-filter", "run",
            "--input", inputPath,
            "--output", outputPath,
            "--summary", summaryPath,
        });

        var lines = File.ReadAllLines(outputPath);
        var expectedHeader = string.Join(",", ProductFilterCsvHeader.ExpectedHeaders);
        Assert.Equal(expectedHeader, lines[0]);
    }

    /// <summary>
    /// 输出 CSV 内容包含 AccountEquity / RiskRate / Result。
    /// </summary>
    [Fact]
    public void Run_OutputCsvContainsRequiredFields()
    {
        var runner = new ProductFilterCliRunner();
        var dir = CreateTempDir();
        var inputPath = CreateValidCsv(dir);
        var outputPath = Path.Combine(dir, "out.csv");
        var summaryPath = Path.Combine(dir, "sum.md");

        runner.Run(new[]
        {
            "product-filter", "run",
            "--input", inputPath,
            "--output", outputPath,
            "--summary", summaryPath,
        });

        var content = File.ReadAllText(outputPath);
        Assert.Contains("10000", content);
        Assert.Contains("Allowed", content);
    }

    /// <summary>
    /// Summary 不包含交易建议措辞。
    /// </summary>
    [Fact]
    public void Run_SummaryDoesNotContainTradingAdvice()
    {
        var runner = new ProductFilterCliRunner();
        var dir = CreateTempDir();
        var inputPath = CreateValidCsv(dir);
        var outputPath = Path.Combine(dir, "out.csv");
        var summaryPath = Path.Combine(dir, "sum.md");

        runner.Run(new[]
        {
            "product-filter", "run",
            "--input", inputPath,
            "--output", outputPath,
            "--summary", summaryPath,
        });

        var content = File.ReadAllText(summaryPath);
        Assert.DoesNotContain("推荐交易", content);
        Assert.DoesNotContain("可以买入", content);
        Assert.DoesNotContain("可以做多", content);
        Assert.DoesNotContain("可以做空", content);
    }

    /// <summary>
    /// 输出目录不存在时可以自动创建。
    /// </summary>
    [Fact]
    public void Run_CreatesOutputDirectoryIfNotExists()
    {
        var runner = new ProductFilterCliRunner();
        var dir = CreateTempDir();
        var inputPath = CreateValidCsv(dir);
        var nestedDir = Path.Combine(dir, "nested", "deep");
        var outputPath = Path.Combine(nestedDir, "out.csv");
        var summaryPath = Path.Combine(nestedDir, "sum.md");

        var exitCode = runner.Run(new[]
        {
            "product-filter", "run",
            "--input", inputPath,
            "--output", outputPath,
            "--summary", summaryPath,
        });

        Assert.Equal(0, exitCode);
        Assert.True(File.Exists(outputPath));
    }

    /// <summary>
    /// CLI 输出不依赖当前时间。
    /// </summary>
    [Fact]
    public void Run_OutputDoesNotDependOnCurrentTime()
    {
        var runner = new ProductFilterCliRunner();
        var dir = CreateTempDir();
        var inputPath = CreateValidCsv(dir);
        var outputPath = Path.Combine(dir, "out.csv");
        var summaryPath = Path.Combine(dir, "sum.md");

        runner.Run(new[]
        {
            "product-filter", "run",
            "--input", inputPath,
            "--output", outputPath,
            "--summary", summaryPath,
        });

        var summary = File.ReadAllText(summaryPath);
        Assert.DoesNotContain("生成时间", summary);
        Assert.DoesNotContain(DateTime.Now.Year.ToString(), summary);
    }
}
