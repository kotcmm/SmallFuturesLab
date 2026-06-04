using SmallFuturesLab.ProductFilter;

namespace SmallFuturesLab.ProductData.Tests;

public class LocalMarketStatSourceTests
{
    private static readonly string FixtureDir = Path.Combine(
        AppContext.BaseDirectory,
        "..", "..", "..", "Fixtures");

    /// <summary>
    /// LocalMarketStatSource 能读取本地 CSV fixture。
    /// </summary>
    [Fact]
    public void ReadsLocalCsvFixture()
    {
        var csvPath = Path.Combine(FixtureDir, "local_market_stat.csv");
        var source = new LocalMarketStatSource();
        var result = source.Read(csvPath);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Records.Count);
    }

    /// <summary>
    /// LocalMarketStatSource 输出 DataSourceType = MarketDataApi。
    /// </summary>
    [Fact]
    public void OutputDataSourceTypeIsMarketDataApi()
    {
        var csvPath = Path.Combine(FixtureDir, "local_market_stat.csv");
        var source = new LocalMarketStatSource();
        var result = source.Read(csvPath);

        Assert.True(result.IsSuccess);
        Assert.All(result.Records, r => Assert.Equal(ProductDataSourceType.MarketDataApi, r.DataSourceType));
    }

    /// <summary>
    /// LocalMarketStatSource 不会生成 CtpAccountActual。
    /// </summary>
    [Fact]
    public void NeverGeneratesCtpAccountActual()
    {
        var csvPath = Path.Combine(FixtureDir, "local_market_stat.csv");
        var source = new LocalMarketStatSource();
        var result = source.Read(csvPath);

        Assert.DoesNotContain(result.Records, r => r.DataSourceType == ProductDataSourceType.CtpAccountActual);
    }

    /// <summary>
    /// LocalMarketStatSource 能解析 ProductCode / ContractCode / Price / TypicalAtr / Volume / OpenInterest。
    /// </summary>
    [Fact]
    public void ParsesRequiredFields()
    {
        var csvPath = Path.Combine(FixtureDir, "local_market_stat.csv");
        var source = new LocalMarketStatSource();
        var result = source.Read(csvPath);

        Assert.True(result.IsSuccess);
        var first = result.Records[0];
        Assert.Equal("MA", first.ProductCode);
        Assert.Equal("MA2501", first.ContractCode);
        Assert.Equal(2500, first.Price);
        Assert.Equal(20, first.TypicalAtr);
        Assert.Equal(123456, first.Volume);
        Assert.Equal(500000, first.OpenInterest);
    }

    /// <summary>
    /// LocalMarketStatSource 能解析 LiquidityLevel / BookContinuityLevel / RolloverClarity。
    /// </summary>
    [Fact]
    public void ParsesEnumFields()
    {
        var csvPath = Path.Combine(FixtureDir, "local_market_stat.csv");
        var source = new LocalMarketStatSource();
        var result = source.Read(csvPath);

        Assert.True(result.IsSuccess);
        var first = result.Records[0];
        Assert.Equal(LiquidityLevel.Good, first.LiquidityLevel);
        Assert.Equal(BookContinuityLevel.Good, first.BookContinuityLevel);
        Assert.Equal(RolloverClarity.Good, first.RolloverClarity);

        var second = result.Records[1];
        Assert.Equal(LiquidityLevel.Medium, second.LiquidityLevel);
        Assert.Equal(BookContinuityLevel.Medium, second.BookContinuityLevel);
        Assert.Equal(RolloverClarity.Medium, second.RolloverClarity);
    }

    /// <summary>
    /// LocalMarketStatSource Price 不可解析时返回错误。
    /// </summary>
    [Fact]
    public void PriceCannotParse_ReturnsError()
    {
        var csv = CreateCsvWithRow(price: "bad");
        var path = WriteTempCsv(csv);
        var source = new LocalMarketStatSource();
        var result = source.Read(path);

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.FieldName == "Price");
    }

    /// <summary>
    /// LocalMarketStatSource TypicalAtr 不可解析时返回错误。
    /// </summary>
    [Fact]
    public void TypicalAtrCannotParse_ReturnsError()
    {
        var csv = CreateCsvWithRow(typicalAtr: "bad");
        var path = WriteTempCsv(csv);
        var source = new LocalMarketStatSource();
        var result = source.Read(path);

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.FieldName == "TypicalAtr");
    }

    /// <summary>
    /// LocalMarketStatSource Volume 不可解析时返回错误。
    /// </summary>
    [Fact]
    public void VolumeCannotParse_ReturnsError()
    {
        var csv = CreateCsvWithRow(volume: "bad");
        var path = WriteTempCsv(csv);
        var source = new LocalMarketStatSource();
        var result = source.Read(path);

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.FieldName == "Volume");
    }

    /// <summary>
    /// LocalMarketStatSource OpenInterest 不可解析时返回错误。
    /// </summary>
    [Fact]
    public void OpenInterestCannotParse_ReturnsError()
    {
        var csv = CreateCsvWithRow(openInterest: "bad");
        var path = WriteTempCsv(csv);
        var source = new LocalMarketStatSource();
        var result = source.Read(path);

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.FieldName == "OpenInterest");
    }

    /// <summary>
    /// LocalMarketStatSource LiquidityLevel 非法时返回错误。
    /// </summary>
    [Fact]
    public void LiquidityLevelInvalid_ReturnsError()
    {
        var csv = CreateCsvWithRow(liquidityLevel: "Excellent");
        var path = WriteTempCsv(csv);
        var source = new LocalMarketStatSource();
        var result = source.Read(path);

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.FieldName == "LiquidityLevel");
    }

    /// <summary>
    /// LocalMarketStatSource BookContinuityLevel 非法时返回错误。
    /// </summary>
    [Fact]
    public void BookContinuityLevelInvalid_ReturnsError()
    {
        var csv = CreateCsvWithRow(bookContinuityLevel: "Excellent");
        var path = WriteTempCsv(csv);
        var source = new LocalMarketStatSource();
        var result = source.Read(path);

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.FieldName == "BookContinuityLevel");
    }

    /// <summary>
    /// LocalMarketStatSource RolloverClarity 非法时返回错误。
    /// </summary>
    [Fact]
    public void RolloverClarityInvalid_ReturnsError()
    {
        var csv = CreateCsvWithRow(rolloverClarity: "Excellent");
        var path = WriteTempCsv(csv);
        var source = new LocalMarketStatSource();
        var result = source.Read(path);

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.FieldName == "RolloverClarity");
    }

    /// <summary>
    /// LocalMarketStatSource NeedsReview 非法时返回错误。
    /// </summary>
    [Fact]
    public void NeedsReviewCannotParse_ReturnsError()
    {
        var csv = CreateCsvWithRow(needsReview: "maybe");
        var path = WriteTempCsv(csv);
        var source = new LocalMarketStatSource();
        var result = source.Read(path);

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.FieldName == "NeedsReview");
    }

    /// <summary>
    /// LocalMarketStatSource 字段数量不足时返回错误。
    /// </summary>
    [Fact]
    public void RowWithMissingColumns_ReturnsError()
    {
        var csv = Header + "\nCZCE,甲醇,MA,MA2501,2500";
        var path = WriteTempCsv(csv);
        var source = new LocalMarketStatSource();
        var result = source.Read(path);

        Assert.False(result.IsSuccess);
    }

    /// <summary>
    /// LocalMarketStatSource 必填表头缺失时返回错误。
    /// </summary>
    [Fact]
    public void MissingRequiredHeader_ReturnsError()
    {
        var csv = "Exchange,ProductName,ProductCode,ContractCode,Price,TypicalAtr,Volume,OpenInterest,LiquidityLevel,BookContinuityLevel,RolloverClarity,DataDate,DataSource\n" +
                  "CZCE,甲醇,MA,MA2501,2500,20,123456,500000,Good,Good,Good,2024-01-01,本地行情统计";
        var path = WriteTempCsv(csv);
        var source = new LocalMarketStatSource();
        var result = source.Read(path);

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.FieldName == "表头校验");
    }

    /// <summary>
    /// LocalMarketStatSource 坏行不会进入 Records。
    /// </summary>
    [Fact]
    public void BadRowDoesNotEnterRecords()
    {
        var csv = CreateCsvWithRow(price: "bad");
        var path = WriteTempCsv(csv);
        var source = new LocalMarketStatSource();
        var result = source.Read(path);

        Assert.False(result.IsSuccess);
        Assert.Empty(result.Records);
    }

    /// <summary>
    /// LocalMarketStatSource Price 不可解析时不创建含 0 的记录。
    /// </summary>
    [Fact]
    public void PriceCannotParse_DoesNotCreateZeroRecord()
    {
        var csv = CreateCsvWithRow(price: "bad");
        var path = WriteTempCsv(csv);
        var source = new LocalMarketStatSource();
        var result = source.Read(path);

        Assert.False(result.IsSuccess);
        Assert.Empty(result.Records);
        var error = result.Errors.First();
        Assert.True(error.RowNumber > 0);
        Assert.Equal("Price", error.FieldName);
        Assert.NotEmpty(error.Reason);
    }

    /// <summary>
    /// LocalMarketStatSource TypicalAtr 不可解析时不创建含 0 的记录。
    /// </summary>
    [Fact]
    public void TypicalAtrCannotParse_DoesNotCreateZeroRecord()
    {
        var csv = CreateCsvWithRow(typicalAtr: "bad");
        var path = WriteTempCsv(csv);
        var source = new LocalMarketStatSource();
        var result = source.Read(path);

        Assert.False(result.IsSuccess);
        Assert.Empty(result.Records);
        var error = result.Errors.First();
        Assert.True(error.RowNumber > 0);
        Assert.Equal("TypicalAtr", error.FieldName);
        Assert.NotEmpty(error.Reason);
    }

    /// <summary>
    /// LocalMarketStatSource Volume 不可解析时不创建含 0 的记录。
    /// </summary>
    [Fact]
    public void VolumeCannotParse_DoesNotCreateZeroRecord()
    {
        var csv = CreateCsvWithRow(volume: "bad");
        var path = WriteTempCsv(csv);
        var source = new LocalMarketStatSource();
        var result = source.Read(path);

        Assert.False(result.IsSuccess);
        Assert.Empty(result.Records);
        var error = result.Errors.First();
        Assert.True(error.RowNumber > 0);
        Assert.Equal("Volume", error.FieldName);
        Assert.NotEmpty(error.Reason);
    }

    /// <summary>
    /// LocalMarketStatSource OpenInterest 不可解析时不创建含 0 的记录。
    /// </summary>
    [Fact]
    public void OpenInterestCannotParse_DoesNotCreateZeroRecord()
    {
        var csv = CreateCsvWithRow(openInterest: "bad");
        var path = WriteTempCsv(csv);
        var source = new LocalMarketStatSource();
        var result = source.Read(path);

        Assert.False(result.IsSuccess);
        Assert.Empty(result.Records);
        var error = result.Errors.First();
        Assert.True(error.RowNumber > 0);
        Assert.Equal("OpenInterest", error.FieldName);
        Assert.NotEmpty(error.Reason);
    }

    private static readonly string Header =
        "Exchange,ProductName,ProductCode,ContractCode,Price,TypicalAtr,Volume,OpenInterest,LiquidityLevel,BookContinuityLevel,RolloverClarity,DataDate,DataSource,NeedsReview";

    private static string CreateCsvWithRow(
        string price = "2500",
        string typicalAtr = "20",
        string volume = "123456",
        string openInterest = "500000",
        string liquidityLevel = "Good",
        string bookContinuityLevel = "Good",
        string rolloverClarity = "Good",
        string needsReview = "true")
    {
        return $"{Header}\nCZCE,甲醇,MA,MA2501,{price},{typicalAtr},{volume},{openInterest},{liquidityLevel},{bookContinuityLevel},{rolloverClarity},2024-01-01,本地行情统计,{needsReview}";
    }

    private static string WriteTempCsv(string csv)
    {
        var path = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.csv");
        File.WriteAllText(path, csv, System.Text.Encoding.UTF8);
        return path;
    }
}
