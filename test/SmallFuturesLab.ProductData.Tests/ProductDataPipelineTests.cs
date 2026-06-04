using SmallFuturesLab.ProductFilter;

namespace SmallFuturesLab.ProductData.Tests;

public class ProductDataPipelineTests
{
    private static readonly string FixtureDir = Path.Combine(
        AppContext.BaseDirectory,
        "..", "..", "..", "Fixtures");

    /// <summary>
    /// ProductDataSourceType 包含 ThirdPartyResearch / CtpAccountActual / ManualConfig。
    /// </summary>
    [Fact]
    public void ProductDataSourceType_ContainsRequiredValues()
    {
        Assert.True(Enum.IsDefined(typeof(ProductDataSourceType), ProductDataSourceType.ThirdPartyResearch));
        Assert.True(Enum.IsDefined(typeof(ProductDataSourceType), ProductDataSourceType.CtpAccountActual));
        Assert.True(Enum.IsDefined(typeof(ProductDataSourceType), ProductDataSourceType.ManualConfig));
    }

    /// <summary>
    /// TradingPlanetHtmlSource 解析本地 HTML fixture 后 DataSourceType = ThirdPartyResearch。
    /// </summary>
    [Fact]
    public void TradingPlanetHtmlSource_ParsesLocalHtml_WithThirdPartyResearchType()
    {
        var htmlPath = Path.Combine(FixtureDir, "trading_planet_sample.html");
        var source = new TradingPlanetHtmlSource();
        var records = source.Read(htmlPath);

        Assert.All(records, r => Assert.Equal(ProductDataSourceType.ThirdPartyResearch, r.DataSourceType));
    }

    /// <summary>
    /// TradingPlanetHtmlSource 解析本地 HTML fixture 后 NeedsReview = true。
    /// </summary>
    [Fact]
    public void TradingPlanetHtmlSource_ParsesLocalHtml_WithNeedsReviewTrue()
    {
        var htmlPath = Path.Combine(FixtureDir, "trading_planet_sample.html");
        var source = new TradingPlanetHtmlSource();
        var records = source.Read(htmlPath);

        Assert.All(records, r => Assert.True(r.NeedsReview));
    }

    /// <summary>
    /// TradingPlanetHtmlSource 能解析品种代码、合约代码、价格、保证金比例、开平合计手续费。
    /// </summary>
    [Fact]
    public void TradingPlanetHtmlSource_ParsesRequiredFields()
    {
        var htmlPath = Path.Combine(FixtureDir, "trading_planet_sample.html");
        var source = new TradingPlanetHtmlSource();
        var records = source.Read(htmlPath);

        Assert.Equal(2, records.Count);
        var first = records[0];
        Assert.Equal("MA", first.ProductCode);
        Assert.Equal("MA2501", first.ContractCode);
        Assert.Equal(2500, first.Price);
        Assert.Equal(0.10, first.MarginRate);
        Assert.Equal(6, first.RoundTripFeePerLot);
    }

    /// <summary>
    /// TradingPlanetHtmlSource 不会生成 CtpAccountActual。
    /// </summary>
    [Fact]
    public void TradingPlanetHtmlSource_NeverGeneratesCtpAccountActual()
    {
        var htmlPath = Path.Combine(FixtureDir, "trading_planet_sample.html");
        var source = new TradingPlanetHtmlSource();
        var records = source.Read(htmlPath);

        Assert.DoesNotContain(records, r => r.DataSourceType == ProductDataSourceType.CtpAccountActual);
    }

    /// <summary>
    /// LocalMarginFeeConfigSource 能读取本地 CSV fixture。
    /// </summary>
    [Fact]
    public void LocalMarginFeeConfigSource_ReadsLocalCsvFixture()
    {
        var csvPath = Path.Combine(FixtureDir, "margin_fee_config.csv");
        var source = new LocalMarginFeeConfigSource();
        var records = source.Read(csvPath);

        Assert.Equal(2, records.Count);
    }

    /// <summary>
    /// LocalMarginFeeConfigSource 输出 DataSourceType = ManualConfig。
    /// </summary>
    [Fact]
    public void LocalMarginFeeConfigSource_OutputDataSourceTypeIsManualConfig()
    {
        var csvPath = Path.Combine(FixtureDir, "margin_fee_config.csv");
        var source = new LocalMarginFeeConfigSource();
        var records = source.Read(csvPath);

        Assert.All(records, r => Assert.Equal(ProductDataSourceType.ManualConfig, r.DataSourceType));
    }

    /// <summary>
    /// LocalMarginFeeConfigSource 不会生成 CtpAccountActual。
    /// </summary>
    [Fact]
    public void LocalMarginFeeConfigSource_NeverGeneratesCtpAccountActual()
    {
        var csvPath = Path.Combine(FixtureDir, "margin_fee_config.csv");
        var source = new LocalMarginFeeConfigSource();
        var records = source.Read(csvPath);

        Assert.DoesNotContain(records, r => r.DataSourceType == ProductDataSourceType.CtpAccountActual);
    }

    /// <summary>
    /// ProductDataNormalizer 能把完整 ProductDataRecord 转成 ProductFilterRow。
    /// </summary>
    [Fact]
    public void Normalizer_ConvertsCompleteRecordToProductFilterRow()
    {
        var record = CreateCompleteRecord();
        var normalizer = new ProductDataNormalizer();
        var result = normalizer.Normalize(record, 10000, 12, 2, 20);

        Assert.True(result.IsSuccess);
        Assert.Equal("MA", result.Row.ProductCode);
        Assert.Equal("MA2501", result.Row.ContractCode);
        Assert.Equal(2500, result.Row.Price);
        Assert.Equal(10, result.Row.Multiplier);
        Assert.Equal(1, result.Row.TickSize);
    }

    /// <summary>
    /// ProductDataNormalizer 使用外部传入的 AccountEquity。
    /// </summary>
    [Fact]
    public void Normalizer_UsesExternalAccountEquity()
    {
        var record = CreateCompleteRecord();
        var normalizer = new ProductDataNormalizer();
        var result = normalizer.Normalize(record, 20000, 12, 2, 20);

        Assert.True(result.IsSuccess);
        Assert.Equal(20000, result.Row.AccountEquity);
    }

    /// <summary>
    /// ProductDataNormalizer 使用外部传入的 StopDistance。
    /// </summary>
    [Fact]
    public void Normalizer_UsesExternalStopDistance()
    {
        var record = CreateCompleteRecord();
        var normalizer = new ProductDataNormalizer();
        var result = normalizer.Normalize(record, 10000, 15, 2, 20);

        Assert.True(result.IsSuccess);
        Assert.Equal(15, result.Row.StopDistance);
    }

    /// <summary>
    /// ProductDataNormalizer 使用外部传入的 SlippageTicks。
    /// </summary>
    [Fact]
    public void Normalizer_UsesExternalSlippageTicks()
    {
        var record = CreateCompleteRecord();
        var normalizer = new ProductDataNormalizer();
        var result = normalizer.Normalize(record, 10000, 12, 3, 20);

        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Row.SlippageTicks);
    }

    /// <summary>
    /// ProductDataNormalizer 使用外部传入的 TypicalAtr。
    /// </summary>
    [Fact]
    public void Normalizer_UsesExternalTypicalAtr()
    {
        var record = CreateCompleteRecord();
        var normalizer = new ProductDataNormalizer();
        var result = normalizer.Normalize(record, 10000, 12, 2, 25);

        Assert.True(result.IsSuccess);
        Assert.Equal(25, result.Row.TypicalAtr);
    }

    /// <summary>
    /// ProductDataNormalizer 遇到缺失 Multiplier 时返回失败。
    /// </summary>
    [Fact]
    public void Normalizer_FailsWhenMultiplierIsMissing()
    {
        var record = CreateCompleteRecord() with { Multiplier = null };
        var normalizer = new ProductDataNormalizer();
        var result = normalizer.Normalize(record, 10000, 12, 2, 20);

        Assert.False(result.IsSuccess);
    }

    /// <summary>
    /// ProductDataNormalizer 遇到缺失 TickSize 时返回失败。
    /// </summary>
    [Fact]
    public void Normalizer_FailsWhenTickSizeIsMissing()
    {
        var record = CreateCompleteRecord() with { TickSize = null };
        var normalizer = new ProductDataNormalizer();
        var result = normalizer.Normalize(record, 10000, 12, 2, 20);

        Assert.False(result.IsSuccess);
    }

    /// <summary>
    /// NeedsReview = true 时，输出 Reasons 包含"数据需复核"。
    /// </summary>
    [Fact]
    public void Normalizer_IncludesReviewWarningWhenNeedsReviewIsTrue()
    {
        var record = CreateCompleteRecord() with { NeedsReview = true };
        var normalizer = new ProductDataNormalizer();
        var result = normalizer.Normalize(record, 10000, 12, 2, 20);

        Assert.True(result.IsSuccess);
        Assert.Contains("数据需复核", result.Row.Reasons);
    }

    /// <summary>
    /// ProductFilterCsvExporter 写出的表头与 ProductFilterCsvHeader.ExpectedHeaders 一致。
    /// </summary>
    [Fact]
    public void CsvExporter_WritesHeaderMatchingTemplate()
    {
        var record = CreateCompleteRecord();
        var normalizer = new ProductDataNormalizer();
        var result = normalizer.Normalize(record, 10000, 12, 2, 20);

        var exporter = new ProductFilterCsvExporter();
        var tempPath = Path.Combine(Path.GetTempPath(), $"test_export_{Guid.NewGuid()}.csv");
        exporter.Export(tempPath, new[] { result.Row });

        var lines = File.ReadAllLines(tempPath);
        var expectedHeader = string.Join(",", ProductFilterCsvHeader.ExpectedHeaders);
        Assert.Equal(expectedHeader, lines[0]);
    }

    /// <summary>
    /// 标准化流程不产生 Allowed / Caution / Rejected 判断。
    /// </summary>
    [Fact]
    public void Normalizer_DoesNotProduceResultStatus()
    {
        var record = CreateCompleteRecord();
        var normalizer = new ProductDataNormalizer();
        var result = normalizer.Normalize(record, 10000, 12, 2, 20);

        Assert.True(result.IsSuccess);
        Assert.Equal(default, result.Row.Result);
    }

    /// <summary>
    /// 标准化流程不生成交易建议措辞。
    /// </summary>
    [Fact]
    public void Normalizer_DoesNotContainTradingAdvice()
    {
        var record = CreateCompleteRecord();
        var normalizer = new ProductDataNormalizer();
        var result = normalizer.Normalize(record, 10000, 12, 2, 20);

        Assert.True(result.IsSuccess);
        Assert.DoesNotContain("推荐交易", result.Row.Reasons);
        Assert.DoesNotContain("可以买入", result.Row.Reasons);
        Assert.DoesNotContain("可以做多", result.Row.Reasons);
        Assert.DoesNotContain("可以做空", result.Row.Reasons);
    }

    /// <summary>
    /// 多条 ProductDataRecord 可以批量标准化为多条 ProductFilterRow。
    /// </summary>
    [Fact]
    public void Normalizer_BatchConvertsMultipleRecords()
    {
        var records = new[]
        {
            CreateCompleteRecord() with { ProductCode = "MA" },
            CreateCompleteRecord() with { ProductCode = "RB" },
        };

        var normalizer = new ProductDataNormalizer();
        var results = records.Select(r => normalizer.Normalize(r, 10000, 12, 2, 20)).ToList();

        Assert.All(results, r => Assert.True(r.IsSuccess));
        Assert.Equal("MA", results[0].Row.ProductCode);
        Assert.Equal("RB", results[1].Row.ProductCode);
    }

    private static ProductDataRecord CreateCompleteRecord()
    {
        return new ProductDataRecord
        {
            Exchange = "CZCE",
            ProductName = "甲醇",
            ProductCode = "MA",
            ContractCode = "MA2501",
            Price = 2500,
            Multiplier = 10,
            TickSize = 1,
            MarginRate = 0.10,
            MarginPerLot = 2500,
            RoundTripFeePerLot = 6,
            OpenFeePerLot = 3,
            CloseYesterdayFeePerLot = 3,
            CloseTodayFeePerLot = 3,
            Volume = 123456,
            OpenInterest = 500000,
            IsMainContract = true,
            DataDate = "2024-01-01",
            DataSource = "交易星球手续费页面",
            DataSourceType = ProductDataSourceType.ThirdPartyResearch,
            NeedsReview = true,
        };
    }
}
