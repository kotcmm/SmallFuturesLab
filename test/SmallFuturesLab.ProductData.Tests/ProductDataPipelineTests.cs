using System.Text;
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
        var result = source.Read(htmlPath);

        Assert.True(result.IsSuccess);
        Assert.All(result.Records, r => Assert.Equal(ProductDataSourceType.ThirdPartyResearch, r.DataSourceType));
    }

    /// <summary>
    /// TradingPlanetHtmlSource 解析本地 HTML fixture 后 NeedsReview = true。
    /// </summary>
    [Fact]
    public void TradingPlanetHtmlSource_ParsesLocalHtml_WithNeedsReviewTrue()
    {
        var htmlPath = Path.Combine(FixtureDir, "trading_planet_sample.html");
        var source = new TradingPlanetHtmlSource();
        var result = source.Read(htmlPath);

        Assert.True(result.IsSuccess);
        Assert.All(result.Records, r => Assert.True(r.NeedsReview));
    }

    /// <summary>
    /// TradingPlanetHtmlSource 能解析品种代码、合约代码、价格、保证金比例、开平合计手续费。
    /// </summary>
    [Fact]
    public void TradingPlanetHtmlSource_ParsesRequiredFields()
    {
        var htmlPath = Path.Combine(FixtureDir, "trading_planet_sample.html");
        var source = new TradingPlanetHtmlSource();
        var result = source.Read(htmlPath);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Records.Count);
        var first = result.Records[0];
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
        var result = source.Read(htmlPath);

        Assert.DoesNotContain(result.Records, r => r.DataSourceType == ProductDataSourceType.CtpAccountActual);
    }

    /// <summary>
    /// TradingPlanetHtmlSource Price 不可解析时返回错误。
    /// </summary>
    [Fact]
    public void TradingPlanetHtmlSource_PriceCannotParse_ReturnsError()
    {
        var html = CreateHtmlWithRow(price: "not_a_number");
        var path = WriteTempHtml(html);
        var source = new TradingPlanetHtmlSource();
        var result = source.Read(path);

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.FieldName == "Price");
    }

    /// <summary>
    /// TradingPlanetHtmlSource MarginRate 不可解析时返回错误。
    /// </summary>
    [Fact]
    public void TradingPlanetHtmlSource_MarginRateCannotParse_ReturnsError()
    {
        var html = CreateHtmlWithRow(marginRate: "bad");
        var path = WriteTempHtml(html);
        var source = new TradingPlanetHtmlSource();
        var result = source.Read(path);

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.FieldName == "MarginRate");
    }

    /// <summary>
    /// TradingPlanetHtmlSource 开平合计手续费不可解析时返回错误。
    /// </summary>
    [Fact]
    public void TradingPlanetHtmlSource_RoundTripFeeCannotParse_ReturnsError()
    {
        var html = CreateHtmlWithRow(roundTripFee: "xxx");
        var path = WriteTempHtml(html);
        var source = new TradingPlanetHtmlSource();
        var result = source.Read(path);

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.FieldName == "RoundTripFeePerLot");
    }

    /// <summary>
    /// TradingPlanetHtmlSource 坏行不会进入 Records。
    /// </summary>
    [Fact]
    public void TradingPlanetHtmlSource_BadRowDoesNotEnterRecords()
    {
        var html = CreateHtmlWithRow(price: "bad");
        var path = WriteTempHtml(html);
        var source = new TradingPlanetHtmlSource();
        var result = source.Read(path);

        Assert.False(result.IsSuccess);
        Assert.Empty(result.Records);
    }

    /// <summary>
    /// TradingPlanetHtmlSource 多行错误时返回多个错误。
    /// </summary>
    [Fact]
    public void TradingPlanetHtmlSource_MultipleBadRows_ReturnsMultipleErrors()
    {
        var html = CreateHtmlWithTwoRows(price1: "bad", price2: "also_bad");
        var path = WriteTempHtml(html);
        var source = new TradingPlanetHtmlSource();
        var result = source.Read(path);

        Assert.False(result.IsSuccess);
        Assert.True(result.Errors.Count >= 2);
    }

    /// <summary>
    /// TradingPlanetHtmlSource Price 不可解析时不创建含 0 的记录。
    /// </summary>
    [Fact]
    public void TradingPlanetHtmlSource_PriceCannotParse_DoesNotCreateZeroRecord()
    {
        var html = CreateHtmlWithRow(price: "bad");
        var path = WriteTempHtml(html);
        var source = new TradingPlanetHtmlSource();
        var result = source.Read(path);

        Assert.False(result.IsSuccess);
        Assert.Empty(result.Records);
        var error = result.Errors.First();
        Assert.True(error.RowNumber > 0);
        Assert.Equal("Price", error.FieldName);
        Assert.NotEmpty(error.Reason);
    }

    /// <summary>
    /// TradingPlanetHtmlSource MarginRate 不可解析时不创建含 0 的记录。
    /// </summary>
    [Fact]
    public void TradingPlanetHtmlSource_MarginRateCannotParse_DoesNotCreateZeroRecord()
    {
        var html = CreateHtmlWithRow(marginRate: "bad");
        var path = WriteTempHtml(html);
        var source = new TradingPlanetHtmlSource();
        var result = source.Read(path);

        Assert.False(result.IsSuccess);
        Assert.Empty(result.Records);
        var error = result.Errors.First();
        Assert.True(error.RowNumber > 0);
        Assert.Equal("MarginRate", error.FieldName);
        Assert.NotEmpty(error.Reason);
    }

    /// <summary>
    /// TradingPlanetHtmlSource 开平合计手续费不可解析时不创建含 0 的记录。
    /// </summary>
    [Fact]
    public void TradingPlanetHtmlSource_RoundTripFeeCannotParse_DoesNotCreateZeroRecord()
    {
        var html = CreateHtmlWithRow(roundTripFee: "bad");
        var path = WriteTempHtml(html);
        var source = new TradingPlanetHtmlSource();
        var result = source.Read(path);

        Assert.False(result.IsSuccess);
        Assert.Empty(result.Records);
        var error = result.Errors.First();
        Assert.True(error.RowNumber > 0);
        Assert.Equal("RoundTripFeePerLot", error.FieldName);
        Assert.NotEmpty(error.Reason);
    }

    /// <summary>
    /// LocalMarginFeeConfigSource 能读取本地 CSV fixture。
    /// </summary>
    [Fact]
    public void LocalMarginFeeConfigSource_ReadsLocalCsvFixture()
    {
        var csvPath = Path.Combine(FixtureDir, "margin_fee_config.csv");
        var source = new LocalMarginFeeConfigSource();
        var result = source.Read(csvPath);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Records.Count);
    }

    /// <summary>
    /// LocalMarginFeeConfigSource 输出 DataSourceType = ManualConfig。
    /// </summary>
    [Fact]
    public void LocalMarginFeeConfigSource_OutputDataSourceTypeIsManualConfig()
    {
        var csvPath = Path.Combine(FixtureDir, "margin_fee_config.csv");
        var source = new LocalMarginFeeConfigSource();
        var result = source.Read(csvPath);

        Assert.True(result.IsSuccess);
        Assert.All(result.Records, r => Assert.Equal(ProductDataSourceType.ManualConfig, r.DataSourceType));
    }

    /// <summary>
    /// LocalMarginFeeConfigSource 不会生成 CtpAccountActual。
    /// </summary>
    [Fact]
    public void LocalMarginFeeConfigSource_NeverGeneratesCtpAccountActual()
    {
        var csvPath = Path.Combine(FixtureDir, "margin_fee_config.csv");
        var source = new LocalMarginFeeConfigSource();
        var result = source.Read(csvPath);

        Assert.DoesNotContain(result.Records, r => r.DataSourceType == ProductDataSourceType.CtpAccountActual);
    }

    /// <summary>
    /// LocalMarginFeeConfigSource MarginRate 不可解析时返回错误。
    /// </summary>
    [Fact]
    public void LocalMarginFeeConfigSource_MarginRateCannotParse_ReturnsError()
    {
        var csv = "Exchange,ProductCode,MarginRate,RoundTripFeePerLot,DataDate,DataSource,NeedsReview\nCZCE,MA,bad,6,2024-01-01,Test,true";
        var path = WriteTempCsv(csv);
        var source = new LocalMarginFeeConfigSource();
        var result = source.Read(path);

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.FieldName == "MarginRate");
    }

    /// <summary>
    /// LocalMarginFeeConfigSource RoundTripFeePerLot 不可解析时返回错误。
    /// </summary>
    [Fact]
    public void LocalMarginFeeConfigSource_RoundTripFeeCannotParse_ReturnsError()
    {
        var csv = "Exchange,ProductCode,MarginRate,RoundTripFeePerLot,DataDate,DataSource,NeedsReview\nCZCE,MA,0.1,bad,2024-01-01,Test,true";
        var path = WriteTempCsv(csv);
        var source = new LocalMarginFeeConfigSource();
        var result = source.Read(path);

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.FieldName == "RoundTripFeePerLot");
    }

    /// <summary>
    /// LocalMarginFeeConfigSource NeedsReview 不可解析时返回错误。
    /// </summary>
    [Fact]
    public void LocalMarginFeeConfigSource_NeedsReviewCannotParse_ReturnsError()
    {
        var csv = "Exchange,ProductCode,MarginRate,RoundTripFeePerLot,DataDate,DataSource,NeedsReview\nCZCE,MA,0.1,6,2024-01-01,Test,maybe";
        var path = WriteTempCsv(csv);
        var source = new LocalMarginFeeConfigSource();
        var result = source.Read(path);

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.FieldName == "NeedsReview");
    }

    /// <summary>
    /// LocalMarginFeeConfigSource 坏行不会进入 Records。
    /// </summary>
    [Fact]
    public void LocalMarginFeeConfigSource_BadRowDoesNotEnterRecords()
    {
        var csv = "Exchange,ProductCode,MarginRate,RoundTripFeePerLot,DataDate,DataSource,NeedsReview\nCZCE,MA,bad,6,2024-01-01,Test,true";
        var path = WriteTempCsv(csv);
        var source = new LocalMarginFeeConfigSource();
        var result = source.Read(path);

        Assert.False(result.IsSuccess);
        Assert.Empty(result.Records);
    }

    /// <summary>
    /// LocalMarginFeeConfigSource 字段数量不足时返回错误。
    /// </summary>
    [Fact]
    public void LocalMarginFeeConfigSource_RowWithMissingColumns_ReturnsError()
    {
        var csv = "Exchange,ProductCode,MarginRate,RoundTripFeePerLot,DataDate,DataSource,NeedsReview\nCZCE,MA,0.1";
        var path = WriteTempCsv(csv);
        var source = new LocalMarginFeeConfigSource();
        var result = source.Read(path);

        Assert.False(result.IsSuccess);
    }

    /// <summary>
    /// LocalMarginFeeConfigSource MarginRate 不可解析时不创建含 0 的记录。
    /// </summary>
    [Fact]
    public void LocalMarginFeeConfigSource_MarginRateCannotParse_DoesNotCreateZeroRecord()
    {
        var csv = "Exchange,ProductCode,MarginRate,RoundTripFeePerLot,DataDate,DataSource,NeedsReview\nCZCE,MA,bad,6,2024-01-01,Test,true";
        var path = WriteTempCsv(csv);
        var source = new LocalMarginFeeConfigSource();
        var result = source.Read(path);

        Assert.False(result.IsSuccess);
        Assert.Empty(result.Records);
        var error = result.Errors.First();
        Assert.True(error.RowNumber > 0);
        Assert.Equal("MarginRate", error.FieldName);
        Assert.NotEmpty(error.Reason);
    }

    /// <summary>
    /// LocalMarginFeeConfigSource RoundTripFeePerLot 不可解析时不创建含 0 的记录。
    /// </summary>
    [Fact]
    public void LocalMarginFeeConfigSource_RoundTripFeeCannotParse_DoesNotCreateZeroRecord()
    {
        var csv = "Exchange,ProductCode,MarginRate,RoundTripFeePerLot,DataDate,DataSource,NeedsReview\nCZCE,MA,0.1,bad,2024-01-01,Test,true";
        var path = WriteTempCsv(csv);
        var source = new LocalMarginFeeConfigSource();
        var result = source.Read(path);

        Assert.False(result.IsSuccess);
        Assert.Empty(result.Records);
        var error = result.Errors.First();
        Assert.True(error.RowNumber > 0);
        Assert.Equal("RoundTripFeePerLot", error.FieldName);
        Assert.NotEmpty(error.Reason);
    }

    /// <summary>
    /// LocalMarginFeeConfigSource NeedsReview 不可解析时不创建含 false 的记录。
    /// </summary>
    [Fact]
    public void LocalMarginFeeConfigSource_NeedsReviewCannotParse_DoesNotCreateFalseRecord()
    {
        var csv = "Exchange,ProductCode,MarginRate,RoundTripFeePerLot,DataDate,DataSource,NeedsReview\nCZCE,MA,0.1,6,2024-01-01,Test,maybe";
        var path = WriteTempCsv(csv);
        var source = new LocalMarginFeeConfigSource();
        var result = source.Read(path);

        Assert.False(result.IsSuccess);
        Assert.Empty(result.Records);
        var error = result.Errors.First();
        Assert.True(error.RowNumber > 0);
        Assert.Equal("NeedsReview", error.FieldName);
        Assert.NotEmpty(error.Reason);
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

    /// <summary>
    /// ProductDataNormalizer ProductCode 为空时返回失败。
    /// </summary>
    [Fact]
    public void Normalizer_FailsWhenProductCodeIsEmpty()
    {
        var record = CreateCompleteRecord() with { ProductCode = "" };
        var normalizer = new ProductDataNormalizer();
        var result = normalizer.Normalize(record, 10000, 12, 2, 20);

        Assert.False(result.IsSuccess);
        Assert.Contains("ProductCode", result.Error);
    }

    /// <summary>
    /// ProductDataNormalizer ContractCode 为空时返回失败。
    /// </summary>
    [Fact]
    public void Normalizer_FailsWhenContractCodeIsEmpty()
    {
        var record = CreateCompleteRecord() with { ContractCode = "" };
        var normalizer = new ProductDataNormalizer();
        var result = normalizer.Normalize(record, 10000, 12, 2, 20);

        Assert.False(result.IsSuccess);
        Assert.Contains("ContractCode", result.Error);
    }

    /// <summary>
    /// ProductDataNormalizer Price 小于等于 0 时返回失败。
    /// </summary>
    [Fact]
    public void Normalizer_FailsWhenPriceIsZeroOrNegative()
    {
        var record = CreateCompleteRecord() with { Price = 0 };
        var normalizer = new ProductDataNormalizer();
        var result = normalizer.Normalize(record, 10000, 12, 2, 20);

        Assert.False(result.IsSuccess);
        Assert.Contains("Price", result.Error);
    }

    /// <summary>
    /// ProductDataNormalizer AccountEquity 小于等于 0 时返回失败。
    /// </summary>
    [Fact]
    public void Normalizer_FailsWhenAccountEquityIsZeroOrNegative()
    {
        var record = CreateCompleteRecord();
        var normalizer = new ProductDataNormalizer();
        var result = normalizer.Normalize(record, 0, 12, 2, 20);

        Assert.False(result.IsSuccess);
        Assert.Contains("AccountEquity", result.Error);
    }

    /// <summary>
    /// ProductDataNormalizer StopDistance 小于等于 0 时返回失败。
    /// </summary>
    [Fact]
    public void Normalizer_FailsWhenStopDistanceIsZeroOrNegative()
    {
        var record = CreateCompleteRecord();
        var normalizer = new ProductDataNormalizer();
        var result = normalizer.Normalize(record, 10000, 0, 2, 20);

        Assert.False(result.IsSuccess);
        Assert.Contains("StopDistance", result.Error);
    }

    /// <summary>
    /// ProductDataNormalizer SlippageTicks 为负数时返回失败。
    /// </summary>
    [Fact]
    public void Normalizer_FailsWhenSlippageTicksIsNegative()
    {
        var record = CreateCompleteRecord();
        var normalizer = new ProductDataNormalizer();
        var result = normalizer.Normalize(record, 10000, 12, -1, 20);

        Assert.False(result.IsSuccess);
        Assert.Contains("SlippageTicks", result.Error);
    }

    /// <summary>
    /// ProductDataNormalizer TypicalAtr 为负数时返回失败。
    /// </summary>
    [Fact]
    public void Normalizer_FailsWhenTypicalAtrIsNegative()
    {
        var record = CreateCompleteRecord();
        var normalizer = new ProductDataNormalizer();
        var result = normalizer.Normalize(record, 10000, 12, 2, -5);

        Assert.False(result.IsSuccess);
        Assert.Contains("TypicalAtr", result.Error);
    }

    /// <summary>
    /// ProductDataNormalizer DataDate 为空时返回失败。
    /// </summary>
    [Fact]
    public void Normalizer_FailsWhenDataDateIsEmpty()
    {
        var record = CreateCompleteRecord() with { DataDate = "" };
        var normalizer = new ProductDataNormalizer();
        var result = normalizer.Normalize(record, 10000, 12, 2, 20);

        Assert.False(result.IsSuccess);
        Assert.Contains("DataDate", result.Error);
    }

    /// <summary>
    /// ProductDataNormalizer DataSource 为空时返回失败。
    /// </summary>
    [Fact]
    public void Normalizer_FailsWhenDataSourceIsEmpty()
    {
        var record = CreateCompleteRecord() with { DataSource = "" };
        var normalizer = new ProductDataNormalizer();
        var result = normalizer.Normalize(record, 10000, 12, 2, 20);

        Assert.False(result.IsSuccess);
        Assert.Contains("DataSource", result.Error);
    }

    /// <summary>
    /// ProductDataNormalizer MarginRate 为负数时返回失败。
    /// </summary>
    [Fact]
    public void Normalizer_FailsWhenMarginRateIsNegative()
    {
        var record = CreateCompleteRecord() with { MarginRate = -0.1 };
        var normalizer = new ProductDataNormalizer();
        var result = normalizer.Normalize(record, 10000, 12, 2, 20);

        Assert.False(result.IsSuccess);
        Assert.Contains("MarginRate", result.Error);
    }

    /// <summary>
    /// ProductDataNormalizer RoundTripFeePerLot 为负数时返回失败。
    /// </summary>
    [Fact]
    public void Normalizer_FailsWhenRoundTripFeePerLotIsNegative()
    {
        var record = CreateCompleteRecord() with { RoundTripFeePerLot = -1 };
        var normalizer = new ProductDataNormalizer();
        var result = normalizer.Normalize(record, 10000, 12, 2, 20);

        Assert.False(result.IsSuccess);
        Assert.Contains("RoundTripFeePerLot", result.Error);
    }

    /// <summary>
    /// ProductDataNormalizer Price 为 NaN 时返回失败。
    /// </summary>
    [Fact]
    public void Normalizer_FailsWhenPriceIsNaN()
    {
        var record = CreateCompleteRecord() with { Price = double.NaN };
        var normalizer = new ProductDataNormalizer();
        var result = normalizer.Normalize(record, 10000, 12, 2, 20);

        Assert.False(result.IsSuccess);
        Assert.Contains("Price", result.Error);
    }

    /// <summary>
    /// ProductDataNormalizer Price 为 Infinity 时返回失败。
    /// </summary>
    [Fact]
    public void Normalizer_FailsWhenPriceIsInfinity()
    {
        var record = CreateCompleteRecord() with { Price = double.PositiveInfinity };
        var normalizer = new ProductDataNormalizer();
        var result = normalizer.Normalize(record, 10000, 12, 2, 20);

        Assert.False(result.IsSuccess);
        Assert.Contains("Price", result.Error);
    }

    /// <summary>
    /// ProductDataNormalizer MarginRate 为 0 时返回失败（数据质量严格口径要求 > 0）。
    /// </summary>
    [Fact]
    public void Normalizer_FailsWhenMarginRateIsZero()
    {
        var record = CreateCompleteRecord() with { MarginRate = 0 };
        var normalizer = new ProductDataNormalizer();
        var result = normalizer.Normalize(record, 10000, 12, 2, 20);

        Assert.False(result.IsSuccess);
        Assert.Contains("MarginRate", result.Error);
    }

    /// <summary>
    /// ProductDataNormalizer RoundTripFeePerLot 为 0 时返回失败（数据质量严格口径要求 > 0）。
    /// </summary>
    [Fact]
    public void Normalizer_FailsWhenRoundTripFeePerLotIsZero()
    {
        var record = CreateCompleteRecord() with { RoundTripFeePerLot = 0 };
        var normalizer = new ProductDataNormalizer();
        var result = normalizer.Normalize(record, 10000, 12, 2, 20);

        Assert.False(result.IsSuccess);
        Assert.Contains("RoundTripFeePerLot", result.Error);
    }

    /// <summary>
    /// ProductDataNormalizer AccountEquity 为 NaN 时返回失败。
    /// </summary>
    [Fact]
    public void Normalizer_FailsWhenAccountEquityIsNaN()
    {
        var record = CreateCompleteRecord();
        var normalizer = new ProductDataNormalizer();
        var result = normalizer.Normalize(record, double.NaN, 12, 2, 20);

        Assert.False(result.IsSuccess);
        Assert.Contains("AccountEquity", result.Error);
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

    private static string WriteTempHtml(string html)
    {
        var path = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.html");
        File.WriteAllText(path, html, Encoding.UTF8);
        return path;
    }

    private static string WriteTempCsv(string csv)
    {
        var path = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.csv");
        File.WriteAllText(path, csv, Encoding.UTF8);
        return path;
    }

    private static string CreateHtmlWithRow(
        string price = "2500",
        string marginRate = "0.10",
        string roundTripFee = "6")
    {
        return $@"<!DOCTYPE html>
<html><body><table><tbody>
<tr>
  <td>甲醇</td>
  <td>MA</td>
  <td>MA2501</td>
  <td>{price}</td>
  <td>123456</td>
  <td>{marginRate}</td>
  <td>2500</td>
  <td>3</td>
  <td>3</td>
  <td>3</td>
  <td>{roundTripFee}</td>
  <td>是</td>
</tr>
</tbody></table></body></html>";
    }

    private static string CreateHtmlWithTwoRows(
        string price1 = "2500",
        string price2 = "3500")
    {
        return $@"<!DOCTYPE html>
<html><body><table><tbody>
<tr>
  <td>甲醇</td><td>MA</td><td>MA2501</td><td>{price1}</td>
  <td>123456</td><td>0.10</td><td>2500</td>
  <td>3</td><td>3</td><td>3</td><td>6</td><td>是</td>
</tr>
<tr>
  <td>螺纹钢</td><td>RB</td><td>RB2501</td><td>{price2}</td>
  <td>789012</td><td>0.12</td><td>4200</td>
  <td>4.5</td><td>4.5</td><td>4.5</td><td>9</td><td>是</td>
</tr>
</tbody></table></body></html>";
    }
}
