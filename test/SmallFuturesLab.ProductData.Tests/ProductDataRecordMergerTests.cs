using SmallFuturesLab.ProductFilter;
using Xunit;

namespace SmallFuturesLab.ProductData.Tests;

/// <summary>
/// ProductDataRecordMerger 及其相关类型的测试。
/// </summary>
public class ProductDataRecordMergerTests
{
    private static ProductDataRecord CreateSpecRecord() => new()
    {
        Exchange = "CZCE",
        ProductName = "甲醇",
        ProductCode = "MA",
        ContractCode = "MA2501",
        Multiplier = 10,
        TickSize = 1,
        Volume = 123456,
        OpenInterest = 500000,
        DataDate = "2024-01-01",
        DataSource = "测试合约规格",
        DataSourceType = ProductDataSourceType.ManualConfig,
        NeedsReview = true,
    };

    private static ProductDataRecord CreateFeeRecord() => new()
    {
        ProductCode = "MA",
        ContractCode = "MA2501",
        MarginRate = 0.10,
        RoundTripFeePerLot = 6,
        OpenFeePerLot = 3,
        CloseYesterdayFeePerLot = 3,
        CloseTodayFeePerLot = 3,
        Volume = 123456,
        OpenInterest = 500000,
        DataDate = "2024-01-01",
        DataSource = "测试保证金手续费",
        DataSourceType = ProductDataSourceType.ManualConfig,
        NeedsReview = true,
    };

    private static ProductDataRecord CreateMarketRecord() => new()
    {
        ProductCode = "MA",
        ContractCode = "MA2501",
        Price = 2500,
        TypicalAtr = 20,
        Volume = 123456,
        OpenInterest = 500000,
        LiquidityLevel = LiquidityLevel.Good,
        BookContinuityLevel = BookContinuityLevel.Good,
        RolloverClarity = RolloverClarity.Good,
        DataDate = "2024-01-01",
        DataSource = "测试行情统计",
        DataSourceType = ProductDataSourceType.MarketDataApi,
        NeedsReview = true,
    };

    #region ProductDataMergeKey

    [Fact]
    public void MergeKey_From_Record_Creates_Key()
    {
        var record = CreateSpecRecord();
        var key = ProductDataMergeKey.From(record);

        Assert.Equal("MA", key.ProductCode);
        Assert.Equal("MA2501", key.ContractCode);
    }

    [Fact]
    public void MergeKey_Throws_When_ProductCode_Empty()
    {
        var record = CreateSpecRecord() with { ProductCode = "" };
        var ex = Assert.Throws<ArgumentException>(() => ProductDataMergeKey.From(record));
        Assert.Contains("ProductCode", ex.Message);
    }

    [Fact]
    public void MergeKey_Throws_When_ContractCode_Empty()
    {
        var record = CreateSpecRecord() with { ContractCode = "" };
        var ex = Assert.Throws<ArgumentException>(() => ProductDataMergeKey.From(record));
        Assert.Contains("ContractCode", ex.Message);
    }

    [Fact]
    public void MergeKey_Equality_Based_On_Fields()
    {
        var key1 = new ProductDataMergeKey { ProductCode = "MA", ContractCode = "MA2501" };
        var key2 = new ProductDataMergeKey { ProductCode = "MA", ContractCode = "MA2501" };
        var key3 = new ProductDataMergeKey { ProductCode = "RB", ContractCode = "RB2501" };

        Assert.Equal(key1, key2);
        Assert.NotEqual(key1, key3);
        Assert.Equal(key1.GetHashCode(), key2.GetHashCode());
    }

    #endregion

    #region ProductDataMergeResult

    [Fact]
    public void MergeResult_IsSuccess_When_No_Errors()
    {
        var result = new ProductDataMergeResult { Records = [CreateSpecRecord()], Errors = [] };
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void MergeResult_IsNotSuccess_When_Has_Errors()
    {
        var result = new ProductDataMergeResult
        {
            Records = [],
            Errors = [new ProductDataMergeError { ProductCode = "MA", ContractCode = "MA2501", FieldName = "Price", Reason = "冲突" }],
        };
        Assert.False(result.IsSuccess);
    }

    #endregion

    #region Basic Merge Behavior

    [Fact]
    public void Empty_Input_Returns_Empty_Records_And_Empty_Errors()
    {
        var merger = new ProductDataRecordMerger();
        var result = merger.Merge(Array.Empty<ProductDataRecord>());

        Assert.Empty(result.Records);
        Assert.Empty(result.Errors);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Single_Complete_Record_Passes_Through()
    {
        var merger = new ProductDataRecordMerger();
        var record = CreateSpecRecord();
        var result = merger.Merge([record]);

        Assert.Single(result.Records);
        Assert.Empty(result.Errors);
        Assert.Equal("MA", result.Records[0].ProductCode);
        Assert.Equal("MA2501", result.Records[0].ContractCode);
    }

    [Fact]
    public void Two_Different_Keys_Output_Separately()
    {
        var merger = new ProductDataRecordMerger();
        var record1 = CreateSpecRecord();
        var record2 = CreateSpecRecord() with { ProductCode = "RB", ContractCode = "RB2501" };
        var result = merger.Merge([record1, record2]);

        Assert.Equal(2, result.Records.Count);
        Assert.Empty(result.Errors);
        Assert.Contains(result.Records, r => r.ProductCode == "MA");
        Assert.Contains(result.Records, r => r.ProductCode == "RB");
    }

    [Fact]
    public void Spec_And_Market_Merge_Successfully()
    {
        var merger = new ProductDataRecordMerger();
        var result = merger.Merge([CreateSpecRecord(), CreateMarketRecord()]);

        Assert.Single(result.Records);
        Assert.Empty(result.Errors);

        var merged = result.Records[0];
        Assert.Equal("CZCE", merged.Exchange);
        Assert.Equal("甲醇", merged.ProductName);
        Assert.Equal(10, merged.Multiplier);
        Assert.Equal(1, merged.TickSize);
        Assert.Equal(2500, merged.Price);
        Assert.Equal(20, merged.TypicalAtr);
        Assert.Equal(123456, merged.Volume);
        Assert.Equal(500000, merged.OpenInterest);
        Assert.Equal(LiquidityLevel.Good, merged.LiquidityLevel);
    }

    [Fact]
    public void Spec_Fee_And_Market_Merge_Successfully()
    {
        var merger = new ProductDataRecordMerger();
        var result = merger.Merge([CreateSpecRecord(), CreateFeeRecord(), CreateMarketRecord()]);

        Assert.Single(result.Records);
        Assert.Empty(result.Errors);

        var merged = result.Records[0];
        Assert.Equal(0.10, merged.MarginRate);
        Assert.Equal(6, merged.RoundTripFeePerLot);
        Assert.Equal(3, merged.OpenFeePerLot);
        Assert.Equal(3, merged.CloseYesterdayFeePerLot);
        Assert.Equal(3, merged.CloseTodayFeePerLot);
    }

    #endregion

    #region Empty Key Validation

    [Fact]
    public void Empty_ProductCode_Returns_Error_Not_In_Records()
    {
        var merger = new ProductDataRecordMerger();
        var record = CreateSpecRecord() with { ProductCode = "" };
        var result = merger.Merge([record]);

        Assert.Empty(result.Records);
        Assert.Single(result.Errors);
        Assert.Equal("", result.Errors[0].ProductCode);
        Assert.Equal("MA2501", result.Errors[0].ContractCode);
    }

    [Fact]
    public void Empty_ContractCode_Returns_Error_Not_In_Records()
    {
        var merger = new ProductDataRecordMerger();
        var record = CreateSpecRecord() with { ContractCode = "" };
        var result = merger.Merge([record]);

        Assert.Empty(result.Records);
        Assert.Single(result.Errors);
        Assert.Equal("MA", result.Errors[0].ProductCode);
        Assert.Equal("", result.Errors[0].ContractCode);
    }

    #endregion

    #region Field Conflicts

    [Fact]
    public void Price_Same_Values_Merge_Success()
    {
        var merger = new ProductDataRecordMerger();
        var r1 = CreateMarketRecord() with { Price = 2500 };
        var r2 = CreateMarketRecord() with { Price = 2500, DataSource = "另一来源" };
        var result = merger.Merge([r1, r2]);

        Assert.Single(result.Records);
        Assert.Empty(result.Errors);
        Assert.Equal(2500, result.Records[0].Price);
    }

    [Fact]
    public void Price_Conflict_Returns_Error()
    {
        var merger = new ProductDataRecordMerger();
        var r1 = CreateMarketRecord() with { Price = 2500 };
        var r2 = CreateMarketRecord() with { Price = 2600, DataSource = "另一来源" };
        var result = merger.Merge([r1, r2]);

        Assert.Empty(result.Records);
        Assert.Single(result.Errors);
        Assert.Equal("Price", result.Errors[0].FieldName);
        Assert.Contains("冲突", result.Errors[0].Reason);
    }

    [Fact]
    public void MarginRate_Conflict_Returns_Error()
    {
        var merger = new ProductDataRecordMerger();
        var r1 = CreateFeeRecord() with { MarginRate = 0.10 };
        var r2 = CreateFeeRecord() with { MarginRate = 0.12, DataSource = "另一来源" };
        var result = merger.Merge([r1, r2]);

        Assert.Empty(result.Records);
        Assert.Single(result.Errors);
        Assert.Equal("MarginRate", result.Errors[0].FieldName);
    }

    [Fact]
    public void RoundTripFeePerLot_Conflict_Returns_Error()
    {
        var merger = new ProductDataRecordMerger();
        var r1 = CreateFeeRecord() with { RoundTripFeePerLot = 6 };
        var r2 = CreateFeeRecord() with { RoundTripFeePerLot = 8, DataSource = "另一来源" };
        var result = merger.Merge([r1, r2]);

        Assert.Empty(result.Records);
        Assert.Single(result.Errors);
        Assert.Equal("RoundTripFeePerLot", result.Errors[0].FieldName);
    }

    #endregion

    #region Single Source Fields

    [Fact]
    public void Multiplier_Only_One_Source_Merge_Success()
    {
        var merger = new ProductDataRecordMerger();
        var spec = CreateSpecRecord();
        var fee = CreateFeeRecord() with { Multiplier = null };
        var result = merger.Merge([spec, fee]);

        Assert.Single(result.Records);
        Assert.Empty(result.Errors);
        Assert.Equal(10, result.Records[0].Multiplier);
    }

    [Fact]
    public void TickSize_Only_One_Source_Merge_Success()
    {
        var merger = new ProductDataRecordMerger();
        var spec = CreateSpecRecord();
        var fee = CreateFeeRecord() with { TickSize = null };
        var result = merger.Merge([spec, fee]);

        Assert.Single(result.Records);
        Assert.Empty(result.Errors);
        Assert.Equal(1, result.Records[0].TickSize);
    }

    #endregion

    #region Enum Fields

    [Fact]
    public void LiquidityLevel_Unknown_And_Good_Merges_To_Good()
    {
        var merger = new ProductDataRecordMerger();
        var r1 = CreateMarketRecord() with { LiquidityLevel = LiquidityLevel.Unknown };
        var r2 = CreateMarketRecord() with { LiquidityLevel = LiquidityLevel.Good, DataSource = "另一来源" };
        var result = merger.Merge([r1, r2]);

        Assert.Single(result.Records);
        Assert.Empty(result.Errors);
        Assert.Equal(LiquidityLevel.Good, result.Records[0].LiquidityLevel);
    }

    [Fact]
    public void LiquidityLevel_Conflict_Returns_Error()
    {
        var merger = new ProductDataRecordMerger();
        var r1 = CreateMarketRecord() with { LiquidityLevel = LiquidityLevel.Good };
        var r2 = CreateMarketRecord() with { LiquidityLevel = LiquidityLevel.Medium, DataSource = "另一来源" };
        var result = merger.Merge([r1, r2]);

        Assert.Empty(result.Records);
        Assert.Single(result.Errors);
        Assert.Equal("LiquidityLevel", result.Errors[0].FieldName);
    }

    [Fact]
    public void BookContinuityLevel_Conflict_Returns_Error()
    {
        var merger = new ProductDataRecordMerger();
        var r1 = CreateMarketRecord() with { BookContinuityLevel = BookContinuityLevel.Good };
        var r2 = CreateMarketRecord() with { BookContinuityLevel = BookContinuityLevel.Medium, DataSource = "另一来源" };
        var result = merger.Merge([r1, r2]);

        Assert.Empty(result.Records);
        Assert.Single(result.Errors);
        Assert.Equal("BookContinuityLevel", result.Errors[0].FieldName);
    }

    [Fact]
    public void RolloverClarity_Conflict_Returns_Error()
    {
        var merger = new ProductDataRecordMerger();
        var r1 = CreateMarketRecord() with { RolloverClarity = RolloverClarity.Good };
        var r2 = CreateMarketRecord() with { RolloverClarity = RolloverClarity.Medium, DataSource = "另一来源" };
        var result = merger.Merge([r1, r2]);

        Assert.Empty(result.Records);
        Assert.Single(result.Errors);
        Assert.Equal("RolloverClarity", result.Errors[0].FieldName);
    }

    #endregion

    #region DataSource and NeedsReview

    [Fact]
    public void DataSource_Merges_Unique_Sources()
    {
        var merger = new ProductDataRecordMerger();
        var result = merger.Merge([CreateSpecRecord(), CreateFeeRecord(), CreateMarketRecord()]);

        Assert.Single(result.Records);
        var dataSource = result.Records[0].DataSource;
        Assert.Contains("测试合约规格", dataSource);
        Assert.Contains("测试保证金手续费", dataSource);
        Assert.Contains("测试行情统计", dataSource);
    }

    [Fact]
    public void DataSource_Deduplicates_Same_Source()
    {
        var merger = new ProductDataRecordMerger();
        var r1 = CreateSpecRecord() with { DataSource = "相同来源" };
        var r2 = CreateFeeRecord() with { DataSource = "相同来源" };
        var result = merger.Merge([r1, r2]);

        Assert.Single(result.Records);
        Assert.Equal("相同来源", result.Records[0].DataSource);
    }

    [Fact]
    public void NeedsReview_Any_True_Merges_To_True()
    {
        var merger = new ProductDataRecordMerger();
        var r1 = CreateSpecRecord() with { NeedsReview = false };
        var r2 = CreateFeeRecord() with { NeedsReview = true };
        var result = merger.Merge([r1, r2]);

        Assert.Single(result.Records);
        Assert.True(result.Records[0].NeedsReview);
    }

    [Fact]
    public void NeedsReview_All_False_Merges_To_False()
    {
        var merger = new ProductDataRecordMerger();
        var r1 = CreateSpecRecord() with { NeedsReview = false };
        var r2 = CreateFeeRecord() with { NeedsReview = false };
        var result = merger.Merge([r1, r2]);

        Assert.Single(result.Records);
        Assert.False(result.Records[0].NeedsReview);
    }

    [Fact]
    public void DataSourceType_Different_Keeps_First_No_Ctp_Priority()
    {
        var merger = new ProductDataRecordMerger();
        var r1 = CreateSpecRecord() with { DataSourceType = ProductDataSourceType.ManualConfig };
        var r2 = CreateMarketRecord() with { DataSourceType = ProductDataSourceType.MarketDataApi };
        var result = merger.Merge([r1, r2]);

        Assert.Single(result.Records);
        Assert.Equal(ProductDataSourceType.ManualConfig, result.Records[0].DataSourceType);
        Assert.NotEqual(ProductDataSourceType.CtpAccountActual, result.Records[0].DataSourceType);
    }

    #endregion

    #region Volume and OpenInterest Non-Negative

    [Fact]
    public void Volume_Zero_Only_Source_Merges_To_Zero_No_Error()
    {
        var merger = new ProductDataRecordMerger();
        var r1 = CreateMarketRecord() with { Volume = 0, DataSource = "来源A" };
        var result = merger.Merge([r1]);

        Assert.Single(result.Records);
        Assert.Empty(result.Errors);
        Assert.Equal(0, result.Records[0].Volume);
    }

    [Fact]
    public void OpenInterest_Zero_Only_Source_Merges_To_Zero_No_Error()
    {
        var merger = new ProductDataRecordMerger();
        var r1 = CreateMarketRecord() with { OpenInterest = 0, DataSource = "来源A" };
        var result = merger.Merge([r1]);

        Assert.Single(result.Records);
        Assert.Empty(result.Errors);
        Assert.Equal(0, result.Records[0].OpenInterest);
    }

    [Fact]
    public void Volume_Zero_And_100_Conflict()
    {
        var merger = new ProductDataRecordMerger();
        var r1 = CreateMarketRecord() with { Volume = 0, DataSource = "来源A" };
        var r2 = CreateMarketRecord() with { Volume = 100, DataSource = "来源B" };
        var result = merger.Merge([r1, r2]);

        Assert.Empty(result.Records);
        Assert.Single(result.Errors);
        Assert.Equal("Volume", result.Errors[0].FieldName);
    }

    [Fact]
    public void OpenInterest_Zero_And_100_Conflict()
    {
        var merger = new ProductDataRecordMerger();
        var r1 = CreateMarketRecord() with { OpenInterest = 0, DataSource = "来源A" };
        var r2 = CreateMarketRecord() with { OpenInterest = 100, DataSource = "来源B" };
        var result = merger.Merge([r1, r2]);

        Assert.Empty(result.Records);
        Assert.Single(result.Errors);
        Assert.Equal("OpenInterest", result.Errors[0].FieldName);
    }

    [Fact]
    public void Volume_Negative_Treated_As_Invalid()
    {
        var merger = new ProductDataRecordMerger();
        var r1 = CreateMarketRecord() with { Volume = -1, DataSource = "来源A" };
        var r2 = CreateMarketRecord() with { Volume = 123456, DataSource = "来源B" };
        var result = merger.Merge([r1, r2]);

        Assert.Single(result.Records);
        Assert.Empty(result.Errors);
        Assert.Equal(123456, result.Records[0].Volume);
    }

    [Fact]
    public void OpenInterest_Negative_Treated_As_Invalid()
    {
        var merger = new ProductDataRecordMerger();
        var r1 = CreateMarketRecord() with { OpenInterest = -1, DataSource = "来源A" };
        var r2 = CreateMarketRecord() with { OpenInterest = 500000, DataSource = "来源B" };
        var result = merger.Merge([r1, r2]);

        Assert.Single(result.Records);
        Assert.Empty(result.Errors);
        Assert.Equal(500000, result.Records[0].OpenInterest);
    }

    #endregion

    #region DataSourceType Inconsistency Tracing

    [Fact]
    public void DataSourceType_Different_Merge_Success()
    {
        var merger = new ProductDataRecordMerger();
        var r1 = CreateSpecRecord() with { DataSourceType = ProductDataSourceType.ManualConfig };
        var r2 = CreateMarketRecord() with { DataSourceType = ProductDataSourceType.MarketDataApi };
        var result = merger.Merge([r1, r2]);

        Assert.Single(result.Records);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void DataSourceType_Different_DataSource_Contains_Inconsistency_Note()
    {
        var merger = new ProductDataRecordMerger();
        var r1 = CreateSpecRecord() with { DataSourceType = ProductDataSourceType.ManualConfig };
        var r2 = CreateMarketRecord() with { DataSourceType = ProductDataSourceType.MarketDataApi };
        var result = merger.Merge([r1, r2]);

        Assert.Single(result.Records);
        var dataSource = result.Records[0].DataSource;
        Assert.Contains("DataSourceType不一致", dataSource);
    }

    [Fact]
    public void DataSourceType_Different_DataSource_Contains_Type_Names()
    {
        var merger = new ProductDataRecordMerger();
        var r1 = CreateSpecRecord() with { DataSourceType = ProductDataSourceType.ManualConfig, DataSource = "来源A" };
        var r2 = CreateMarketRecord() with { DataSourceType = ProductDataSourceType.MarketDataApi, DataSource = "来源B" };
        var result = merger.Merge([r1, r2]);

        Assert.Single(result.Records);
        var dataSource = result.Records[0].DataSource;
        Assert.Contains("ManualConfig", dataSource);
        Assert.Contains("MarketDataApi", dataSource);
    }

    [Fact]
    public void DataSourceType_Same_No_Inconsistency_Note()
    {
        var merger = new ProductDataRecordMerger();
        var r1 = CreateSpecRecord() with { DataSourceType = ProductDataSourceType.ManualConfig, DataSource = "来源A" };
        var r2 = CreateFeeRecord() with { DataSourceType = ProductDataSourceType.ManualConfig, DataSource = "来源B" };
        var result = merger.Merge([r1, r2]);

        Assert.Single(result.Records);
        var dataSource = result.Records[0].DataSource;
        Assert.DoesNotContain("DataSourceType不一致", dataSource);
    }

    #endregion

    #region Does Not Do Prohibited Things

    [Fact]
    public void Merger_Does_Not_Call_ProductFilterCalculator()
    {
        var merger = new ProductDataRecordMerger();
        var result = merger.Merge([CreateSpecRecord()]);

        Assert.Single(result.Records);
        // 合并器输出的是 ProductDataRecord，不是 ProductFilterRow，自然没有调用 ProductFilterCalculator
        Assert.Equal(default, result.Records[0].MarginPerLot);
    }

    [Fact]
    public void Merger_Does_Not_Produce_Result_Status()
    {
        var merger = new ProductDataRecordMerger();
        var result = merger.Merge([CreateSpecRecord()]);

        Assert.Single(result.Records);
        // ProductDataRecord 没有 Result 字段
        Assert.IsType<ProductDataRecord>(result.Records[0]);
    }

    [Fact]
    public void Merger_Does_Not_Generate_Trading_Advice()
    {
        var merger = new ProductDataRecordMerger();
        var result = merger.Merge([CreateSpecRecord()]);

        Assert.Single(result.Records);
        // ProductDataRecord 没有 Reasons 字段，只有 DataSource
        Assert.Equal("测试合约规格", result.Records[0].DataSource);
    }

    #endregion

    #region Integration with Expander

    [Fact]
    public void Merged_Record_Can_Be_Expanded_To_10_Rows()
    {
        var merger = new ProductDataRecordMerger();
        var mergeResult = merger.Merge([CreateSpecRecord(), CreateFeeRecord(), CreateMarketRecord()]);

        Assert.Single(mergeResult.Records);
        Assert.Empty(mergeResult.Errors);

        var record = mergeResult.Records[0];
        var scenarioSet = ProductFilterScenarioSet.CreateDefault(tickSize: 1, typicalAtr: 20, slippageTicks: 2);
        var expander = new ProductFilterScenarioExpander();
        var expandResult = expander.Expand(record, scenarioSet);

        Assert.Equal(10, expandResult.Rows.Count);
        Assert.Empty(expandResult.Errors);
    }

    #endregion

    #region Failure Isolation

    [Fact]
    public void Failed_Key_Not_In_Records()
    {
        var merger = new ProductDataRecordMerger();
        var r1 = CreateMarketRecord() with { Price = 2500 };
        var r2 = CreateMarketRecord() with { Price = 2600, DataSource = "另一来源" };
        var result = merger.Merge([r1, r2]);

        Assert.Empty(result.Records);
        Assert.Single(result.Errors);
    }

    [Fact]
    public void One_Key_Failure_Does_Not_Block_Another()
    {
        var merger = new ProductDataRecordMerger();
        var goodKey = CreateSpecRecord() with { ProductCode = "RB", ContractCode = "RB2501" };
        var badKey1 = CreateMarketRecord() with { Price = 2500 };
        var badKey2 = CreateMarketRecord() with { Price = 2600, DataSource = "另一来源" };
        var result = merger.Merge([goodKey, badKey1, badKey2]);

        Assert.Single(result.Records);
        Assert.Single(result.Errors);
        Assert.Equal("RB", result.Records[0].ProductCode);
        Assert.Equal("RB2501", result.Records[0].ContractCode);
    }

    #endregion
}
