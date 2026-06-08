using SmallFuturesLab.ProductData.Sources;
using SmallFuturesLab.ProductData.Exporting;
using SmallFuturesLab.ProductData.Models;
using SmallFuturesLab.ProductData.Abstractions;
using SmallFuturesLab.ProductData.Pipeline;
using SmallFuturesLab.ProductData.Reading;
using SmallFuturesLab.ProductData.Scenarios;
using System.Globalization;
using SmallFuturesLab.ProductFilter;
using Xunit;

namespace SmallFuturesLab.ProductData.Tests;

/// <summary>
/// ProductDataLocalCompositionPipeline 及其相关类型的测试。
/// 覆盖本地组合管线的读取、合并、展开和错误隔离。
/// </summary>
public class ProductDataLocalCompositionPipelineTests
{
    #region 内嵌数据源适配器

    /// <summary>
    /// 测试用保证金手续费 CSV 数据源适配器。
    /// </summary>
    private class PipelineMarginFeeSource : IProductDataSource
    {
        /// <summary>
        /// 从本地 CSV 文件中读取保证金手续费配置。
        /// </summary>
        /// <param name="filePath">本地 CSV 文件路径。</param>
        /// <returns>读取结果，包含记录和错误。</returns>
        public ProductDataReadResult Read(string filePath)
        {
            var records = new List<ProductDataRecord>();
            var errors = new List<ProductDataReadError>();
            var lines = File.ReadAllLines(filePath);
            if (lines.Length < 2)
            {
                return new ProductDataReadResult { Records = records, Errors = errors };
            }

            var headers = lines[0].Split(',');
            var headerIndex = headers.Select((h, i) => (h.Trim(), i)).ToDictionary(x => x.Item1, x => x.Item2);

            for (int i = 1; i < lines.Length; i++)
            {
                var rowNumber = i + 1;
                var values = lines[i].Split(',');
                if (values.Length < headers.Length)
                {
                    errors.Add(new ProductDataReadError
                    {
                        RowNumber = rowNumber,
                        FieldName = "行数据",
                        Reason = $"字段数量不足，期望 {headers.Length} 个，实际 {values.Length} 个",
                    });
                    continue;
                }

                var rowErrors = new List<ProductDataReadError>();
                var marginRate = TryParseDouble(GetValue(values, headerIndex, "MarginRate"), "MarginRate", rowNumber, rowErrors);
                var roundTripFee = TryParseDouble(GetValue(values, headerIndex, "RoundTripFeePerLot"), "RoundTripFeePerLot", rowNumber, rowErrors);
                var volume = TryParseDouble(GetValue(values, headerIndex, "Volume"), "Volume", rowNumber, rowErrors);
                var openInterest = TryParseDouble(GetValue(values, headerIndex, "OpenInterest"), "OpenInterest", rowNumber, rowErrors);
                var needsReview = TryParseBool(GetValue(values, headerIndex, "NeedsReview"), "NeedsReview", rowNumber, rowErrors);

                if (rowErrors.Count > 0 || !marginRate.HasValue || !roundTripFee.HasValue || !volume.HasValue || !openInterest.HasValue || !needsReview.HasValue)
                {
                    errors.AddRange(rowErrors);
                    continue;
                }

                var record = new ProductDataRecord
                {
                    Exchange = GetValue(values, headerIndex, "Exchange"),
                    ProductCode = GetValue(values, headerIndex, "ProductCode"),
                    ContractCode = GetValue(values, headerIndex, "ContractCode"),
                    MarginRate = marginRate.Value,
                    RoundTripFeePerLot = roundTripFee.Value,
                    Volume = volume.Value,
                    OpenInterest = openInterest.Value,
                    DataDate = GetValue(values, headerIndex, "DataDate"),
                    DataSource = GetValue(values, headerIndex, "DataSource"),
                    NeedsReview = needsReview.Value,
                    DataSourceType = ProductDataSourceType.ManualConfig,
                };

                records.Add(record);
            }

            return new ProductDataReadResult { Records = records, Errors = errors };
        }
    }

    /// <summary>
    /// 测试用合约规格 CSV 数据源适配器。
    /// </summary>
    private class PipelineContractSpecSource : IProductDataSource
    {
        /// <summary>
        /// 从本地 CSV 文件中读取合约规格数据。
        /// </summary>
        /// <param name="filePath">本地 CSV 文件路径。</param>
        /// <returns>读取结果，包含记录和错误。</returns>
        public ProductDataReadResult Read(string filePath)
        {
            var records = new List<ProductDataRecord>();
            var errors = new List<ProductDataReadError>();
            var lines = File.ReadAllLines(filePath);
            if (lines.Length < 2)
            {
                return new ProductDataReadResult { Records = records, Errors = errors };
            }

            var headers = lines[0].Split(',');
            var headerIndex = headers.Select((h, i) => (h.Trim(), i)).ToDictionary(x => x.Item1, x => x.Item2);

            for (int i = 1; i < lines.Length; i++)
            {
                var rowNumber = i + 1;
                var values = lines[i].Split(',');
                if (values.Length < headers.Length)
                {
                    errors.Add(new ProductDataReadError
                    {
                        RowNumber = rowNumber,
                        FieldName = "行数据",
                        Reason = $"字段数量不足，期望 {headers.Length} 个，实际 {values.Length} 个",
                    });
                    continue;
                }

                var rowErrors = new List<ProductDataReadError>();
                var multiplier = TryParseDouble(GetValue(values, headerIndex, "Multiplier"), "Multiplier", rowNumber, rowErrors);
                var tickSize = TryParseDouble(GetValue(values, headerIndex, "TickSize"), "TickSize", rowNumber, rowErrors);
                var volume = TryParseDouble(GetValue(values, headerIndex, "Volume"), "Volume", rowNumber, rowErrors);
                var openInterest = TryParseDouble(GetValue(values, headerIndex, "OpenInterest"), "OpenInterest", rowNumber, rowErrors);
                var needsReview = TryParseBool(GetValue(values, headerIndex, "NeedsReview"), "NeedsReview", rowNumber, rowErrors);

                if (rowErrors.Count > 0 || !multiplier.HasValue || !tickSize.HasValue || !volume.HasValue || !openInterest.HasValue || !needsReview.HasValue)
                {
                    errors.AddRange(rowErrors);
                    continue;
                }

                var record = new ProductDataRecord
                {
                    Exchange = GetValue(values, headerIndex, "Exchange"),
                    ProductName = GetValue(values, headerIndex, "ProductName"),
                    ProductCode = GetValue(values, headerIndex, "ProductCode"),
                    ContractCode = GetValue(values, headerIndex, "ContractCode"),
                    Multiplier = multiplier.Value,
                    TickSize = tickSize.Value,
                    Volume = volume.Value,
                    OpenInterest = openInterest.Value,
                    DataDate = GetValue(values, headerIndex, "DataDate"),
                    DataSource = GetValue(values, headerIndex, "DataSource"),
                    NeedsReview = needsReview.Value,
                    DataSourceType = ProductDataSourceType.ManualConfig,
                };

                records.Add(record);
            }

            return new ProductDataReadResult { Records = records, Errors = errors };
        }
    }

    #endregion

    #region 共享辅助方法

    private static string GetValue(string[] values, Dictionary<string, int> index, string field)
    {
        if (index.TryGetValue(field, out var idx) && idx < values.Length)
            return values[idx].Trim();
        return string.Empty;
    }

    private static double? TryParseDouble(string value, string fieldName, int rowNumber, List<ProductDataReadError> errors)
    {
        if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
            return result;

        errors.Add(new ProductDataReadError
        {
            RowNumber = rowNumber,
            FieldName = fieldName,
            Reason = $"不可解析为数字: '{value}'",
        });
        return null;
    }

    private static bool? TryParseBool(string value, string fieldName, int rowNumber, List<ProductDataReadError> errors)
    {
        if (bool.TryParse(value, out var result))
            return result;

        errors.Add(new ProductDataReadError
        {
            RowNumber = rowNumber,
            FieldName = fieldName,
            Reason = $"不可解析为布尔值: '{value}'",
        });
        return null;
    }

    #endregion

    #region 辅助方法

    private static string GetFixturePath(string fileName)
    {
        var directory = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "Fixtures");
        return Path.Combine(directory, fileName);
    }

    private static ProductDataSourceInput CreateContractSpecInput(string filePath)
    {
        return new ProductDataSourceInput
        {
            Name = "合约规格",
            Source = new PipelineContractSpecSource(),
            FilePath = filePath,
        };
    }

    private static ProductDataSourceInput CreateMarginFeeInput(string filePath)
    {
        return new ProductDataSourceInput
        {
            Name = "保证金手续费",
            Source = new PipelineMarginFeeSource(),
            FilePath = filePath,
        };
    }

    private static ProductDataSourceInput CreateMarketStatInput(string filePath)
    {
        return new ProductDataSourceInput
        {
            Name = "行情统计",
            Source = new LocalMarketStatSource(),
            FilePath = filePath,
        };
    }

    private static ProductFilterScenarioSet GetDefaultScenarioSet()
    {
        return ProductFilterScenarioSet.CreateDefault(tickSize: 1, typicalAtr: 20, slippageTicks: 2);
    }

    #endregion

    #region ProductDataSourceInput 校验

    [Fact]
    public void ProductDataSourceInput_Name_Empty_Throws_ArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(() => new ProductDataSourceInput
        {
            Name = "",
            Source = new PipelineContractSpecSource(),
            FilePath = "test.csv",
        });
        Assert.Contains("Name", ex.Message);
    }

    [Fact]
    public void ProductDataSourceInput_Source_Null_Throws_ArgumentNullException()
    {
        var ex = Assert.Throws<ArgumentNullException>(() => new ProductDataSourceInput
        {
            Name = "测试",
            Source = null!,
            FilePath = "test.csv",
        });
        Assert.Contains("Source", ex.Message);
    }

    [Fact]
    public void ProductDataSourceInput_FilePath_Empty_Throws_ArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(() => new ProductDataSourceInput
        {
            Name = "测试",
            Source = new PipelineContractSpecSource(),
            FilePath = "",
        });
        Assert.Contains("FilePath", ex.Message);
    }

    #endregion

    #region 空输入

    [Fact]
    public void Run_EmptyInputs_ReturnsEmptyResult()
    {
        var pipeline = new ProductDataLocalCompositionPipeline();
        var result = pipeline.Run([], GetDefaultScenarioSet());

        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.Empty(result.MergedRecords);
        Assert.Empty(result.Rows);
        Assert.Empty(result.Errors);
    }

    #endregion

    #region 完整管线流程

    [Fact]
    public void Run_ThreeFixtures_MergesIntoOneCompleteRecord()
    {
        var inputs = new[]
        {
            CreateContractSpecInput(GetFixturePath("pipeline_contract_spec.csv")),
            CreateMarginFeeInput(GetFixturePath("pipeline_margin_fee.csv")),
            CreateMarketStatInput(GetFixturePath("pipeline_market_stat.csv")),
        };

        var pipeline = new ProductDataLocalCompositionPipeline();
        var result = pipeline.Run(inputs, GetDefaultScenarioSet());

        var maRecord = result.MergedRecords.FirstOrDefault(r => r.ProductCode == "MA" && r.ContractCode == "MA2501");
        Assert.NotNull(maRecord);
        Assert.Equal("CZCE", maRecord.Exchange);
        Assert.Equal("甲醇", maRecord.ProductName);
        Assert.Equal(2500, maRecord.Price);
        Assert.Equal(10, maRecord.Multiplier);
        Assert.Equal(1, maRecord.TickSize);
        Assert.Equal(0.10, maRecord.MarginRate);
        Assert.Equal(6, maRecord.RoundTripFeePerLot);
        Assert.Equal(20, maRecord.TypicalAtr);
    }

    [Fact]
    public void Run_ThreeFixtures_ExpandsInto10Rows()
    {
        var inputs = new[]
        {
            CreateContractSpecInput(GetFixturePath("pipeline_contract_spec.csv")),
            CreateMarginFeeInput(GetFixturePath("pipeline_margin_fee.csv")),
            CreateMarketStatInput(GetFixturePath("pipeline_market_stat.csv")),
        };

        var pipeline = new ProductDataLocalCompositionPipeline();
        var result = pipeline.Run(inputs, GetDefaultScenarioSet());

        var maRows = result.Rows.Where(r => r.ProductCode == "MA" && r.ContractCode == "MA2501").ToList();
        Assert.Equal(10, maRows.Count);
    }

    [Fact]
    public void Run_ExpandedRows_ContainExpectedFields()
    {
        var inputs = new[]
        {
            CreateContractSpecInput(GetFixturePath("pipeline_contract_spec.csv")),
            CreateMarginFeeInput(GetFixturePath("pipeline_margin_fee.csv")),
            CreateMarketStatInput(GetFixturePath("pipeline_market_stat.csv")),
        };

        var pipeline = new ProductDataLocalCompositionPipeline();
        var result = pipeline.Run(inputs, GetDefaultScenarioSet());

        var maRow = result.Rows.First(r => r.ProductCode == "MA" && r.ContractCode == "MA2501");
        Assert.Equal("MA", maRow.ProductCode);
        Assert.Equal("MA2501", maRow.ContractCode);
        Assert.Equal(2500, maRow.Price);
        Assert.Equal(10, maRow.Multiplier);
        Assert.Equal(1, maRow.TickSize);
        Assert.Equal(0.10, maRow.MarginRate);
        Assert.Equal(6, maRow.RoundTripFeePerLot);
    }

    [Fact]
    public void Run_ExpandedRows_ContainBothAccountEquities()
    {
        var inputs = new[]
        {
            CreateContractSpecInput(GetFixturePath("pipeline_contract_spec.csv")),
            CreateMarginFeeInput(GetFixturePath("pipeline_margin_fee.csv")),
            CreateMarketStatInput(GetFixturePath("pipeline_market_stat.csv")),
        };

        var pipeline = new ProductDataLocalCompositionPipeline();
        var result = pipeline.Run(inputs, GetDefaultScenarioSet());

        var equities = result.Rows.Select(r => r.AccountEquity).Distinct().OrderBy(x => x).ToList();
        Assert.Equal(new[] { 10000.0, 20000.0 }, equities);
    }

    [Fact]
    public void Run_ExpandedRows_ContainFiveStopDistances()
    {
        var inputs = new[]
        {
            CreateContractSpecInput(GetFixturePath("pipeline_contract_spec.csv")),
            CreateMarginFeeInput(GetFixturePath("pipeline_margin_fee.csv")),
            CreateMarketStatInput(GetFixturePath("pipeline_market_stat.csv")),
        };

        var pipeline = new ProductDataLocalCompositionPipeline();
        var result = pipeline.Run(inputs, GetDefaultScenarioSet());

        var maRows = result.Rows.Where(r => r.ProductCode == "MA" && r.ContractCode == "MA2501").ToList();
        Assert.Contains(maRows, r => r.StopDistance == 3);
        Assert.Contains(maRows, r => r.StopDistance == 5);
        Assert.Contains(maRows, r => r.StopDistance == 10);
        Assert.Contains(maRows, r => r.StopDistance == 10); // 0.5 * 20 = 10
        Assert.Contains(maRows, r => r.StopDistance == 20); // 1.0 * 20 = 20
    }

    #endregion

    #region 错误隔离

    [Fact]
    public void Run_ReadErrors_ConvertedToPipelineErrors()
    {
        var inputs = new[]
        {
            CreateContractSpecInput(GetFixturePath("pipeline_contract_spec.csv")),
            CreateMarginFeeInput(GetFixturePath("pipeline_margin_fee.csv")),
            CreateMarketStatInput(GetFixturePath("pipeline_market_stat.csv")),
        };

        var pipeline = new ProductDataLocalCompositionPipeline();
        var result = pipeline.Run(inputs, GetDefaultScenarioSet());

        var readErrors = result.Errors.Where(e => e.Stage == "Read").ToList();
        Assert.NotEmpty(readErrors);
        Assert.Contains(readErrors, e => e.SourceName == "保证金手续费");
        Assert.Contains(readErrors, e => e.Reason.Contains("bad"));
    }

    [Fact]
    public void Run_MergeErrors_ConvertedToPipelineErrors()
    {
        var inputs = new[]
        {
            CreateContractSpecInput(GetFixturePath("pipeline_contract_spec.csv")),
            CreateMarginFeeInput(GetFixturePath("pipeline_margin_fee.csv")),
            CreateMarketStatInput(GetFixturePath("pipeline_market_stat.csv")),
        };

        var pipeline = new ProductDataLocalCompositionPipeline();
        var result = pipeline.Run(inputs, GetDefaultScenarioSet());

        var mergeErrors = result.Errors.Where(e => e.Stage == "Merge").ToList();
        Assert.NotEmpty(mergeErrors);
        Assert.Contains(mergeErrors, e => e.ProductCode == "RB" && e.ContractCode == "RB2501");
    }

    [Fact]
    public void Run_ExpandErrors_ConvertedToPipelineErrors()
    {
        var inputs = new[]
        {
            CreateContractSpecInput(GetFixturePath("pipeline_contract_spec.csv")),
            CreateMarginFeeInput(GetFixturePath("pipeline_margin_fee.csv")),
            CreateMarketStatInput(GetFixturePath("pipeline_market_stat.csv")),
        };

        var pipeline = new ProductDataLocalCompositionPipeline();
        var result = pipeline.Run(inputs, GetDefaultScenarioSet());

        var expandErrors = result.Errors.Where(e => e.Stage == "Expand").ToList();
        Assert.NotEmpty(expandErrors);
        Assert.Contains(expandErrors, e => e.ProductCode == "MA" && e.ContractCode == "MA2502");
    }

    [Fact]
    public void Run_BadKeyDoesNotAffectGoodKey()
    {
        var inputs = new[]
        {
            CreateContractSpecInput(GetFixturePath("pipeline_contract_spec.csv")),
            CreateMarginFeeInput(GetFixturePath("pipeline_margin_fee.csv")),
            CreateMarketStatInput(GetFixturePath("pipeline_market_stat.csv")),
        };

        var pipeline = new ProductDataLocalCompositionPipeline();
        var result = pipeline.Run(inputs, GetDefaultScenarioSet());

        Assert.Contains(result.Rows, r => r.ProductCode == "MA" && r.ContractCode == "MA2501");
        Assert.Contains(result.Errors, e => e.ProductCode == "RB" && e.ContractCode == "RB2501");
        Assert.Contains(result.Errors, e => e.ProductCode == "MA" && e.ContractCode == "MA2502");
    }

    #endregion

    #region DataSourceType 与 NeedsReview

    [Fact]
    public void Run_DataSourceTypeConflict_PreservedInRowDataSource()
    {
        var inputs = new[]
        {
            CreateContractSpecInput(GetFixturePath("pipeline_contract_spec.csv")),
            CreateMarginFeeInput(GetFixturePath("pipeline_margin_fee.csv")),
            CreateMarketStatInput(GetFixturePath("pipeline_market_stat.csv")),
        };

        var pipeline = new ProductDataLocalCompositionPipeline();
        var result = pipeline.Run(inputs, GetDefaultScenarioSet());

        var maRow = result.Rows.First(r => r.ProductCode == "MA" && r.ContractCode == "MA2501");
        Assert.Contains("DataSourceType不一致", maRow.DataSource);
    }

    [Fact]
    public void Run_NeedsReview_True_ContainsDataNeedReviewReason()
    {
        var inputs = new[]
        {
            CreateContractSpecInput(GetFixturePath("pipeline_contract_spec.csv")),
            CreateMarginFeeInput(GetFixturePath("pipeline_margin_fee.csv")),
            CreateMarketStatInput(GetFixturePath("pipeline_market_stat.csv")),
        };

        var pipeline = new ProductDataLocalCompositionPipeline();
        var result = pipeline.Run(inputs, GetDefaultScenarioSet());

        var maRows = result.Rows.Where(r => r.ProductCode == "MA" && r.ContractCode == "MA2501").ToList();
        Assert.All(maRows, r => Assert.Contains("数据需复核", r.Reasons));
    }

    #endregion

    #region 不计算、不判断、不建议

    [Fact]
    public void Run_Rows_DoNotContainAllowedCautionRejected()
    {
        var inputs = new[]
        {
            CreateContractSpecInput(GetFixturePath("pipeline_contract_spec.csv")),
            CreateMarginFeeInput(GetFixturePath("pipeline_margin_fee.csv")),
            CreateMarketStatInput(GetFixturePath("pipeline_market_stat.csv")),
        };

        var pipeline = new ProductDataLocalCompositionPipeline();
        var result = pipeline.Run(inputs, GetDefaultScenarioSet());

        Assert.All(result.Rows, r => Assert.Equal(default(ProductFilterResultStatus), r.Result));
    }

    [Fact]
    public void Run_Rows_DoNotContainTradingAdvice()
    {
        var inputs = new[]
        {
            CreateContractSpecInput(GetFixturePath("pipeline_contract_spec.csv")),
            CreateMarginFeeInput(GetFixturePath("pipeline_margin_fee.csv")),
            CreateMarketStatInput(GetFixturePath("pipeline_market_stat.csv")),
        };

        var pipeline = new ProductDataLocalCompositionPipeline();
        var result = pipeline.Run(inputs, GetDefaultScenarioSet());

        Assert.All(result.Rows, r =>
        {
            Assert.DoesNotContain("推荐", r.Reasons);
            Assert.DoesNotContain("买入", r.Reasons);
            Assert.DoesNotContain("做多", r.Reasons);
            Assert.DoesNotContain("做空", r.Reasons);
        });
    }

    [Theory]
    [InlineData(nameof(ProductFilterRow.TickValue))]
    [InlineData(nameof(ProductFilterRow.MarginPerLot))]
    [InlineData(nameof(ProductFilterRow.AtrMoneyPerLot))]
    [InlineData(nameof(ProductFilterRow.StopRiskMoney))]
    [InlineData(nameof(ProductFilterRow.SlippageMoney))]
    [InlineData(nameof(ProductFilterRow.CostMoney))]
    [InlineData(nameof(ProductFilterRow.TotalRiskMoney))]
    [InlineData(nameof(ProductFilterRow.RiskRate))]
    [InlineData(nameof(ProductFilterRow.MarginRateOfEquity))]
    [InlineData(nameof(ProductFilterRow.CostRatio))]
    public void Run_Rows_DoNotCalculateFormulaFields(string propertyName)
    {
        var inputs = new[]
        {
            CreateContractSpecInput(GetFixturePath("pipeline_contract_spec.csv")),
            CreateMarginFeeInput(GetFixturePath("pipeline_margin_fee.csv")),
            CreateMarketStatInput(GetFixturePath("pipeline_market_stat.csv")),
        };

        var pipeline = new ProductDataLocalCompositionPipeline();
        var result = pipeline.Run(inputs, GetDefaultScenarioSet());

        var property = typeof(ProductFilterRow).GetProperty(propertyName);
        Assert.NotNull(property);

        Assert.All(result.Rows, row =>
        {
            var value = property.GetValue(row);
            if (value is double d)
            {
                Assert.Equal(0.0, d);
            }
        });
    }

    #endregion

    #region CSV 导出验证

    [Fact]
    public void Run_Rows_CanBeExportedToTempDirectory()
    {
        var inputs = new[]
        {
            CreateContractSpecInput(GetFixturePath("pipeline_contract_spec.csv")),
            CreateMarginFeeInput(GetFixturePath("pipeline_margin_fee.csv")),
            CreateMarketStatInput(GetFixturePath("pipeline_market_stat.csv")),
        };

        var pipeline = new ProductDataLocalCompositionPipeline();
        var result = pipeline.Run(inputs, GetDefaultScenarioSet());

        var tempDir = Path.Combine(Path.GetTempPath(), $"SmallFuturesLab_Test_{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);
        try
        {
            var exportPath = Path.Combine(tempDir, "pipeline_test_output.csv");
            var exporter = new ProductFilterCsvExporter();
            exporter.Export(exportPath, result.Rows);

            Assert.True(File.Exists(exportPath));
            var lines = File.ReadAllLines(exportPath);
            Assert.True(lines.Length > 1);
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [Fact]
    public void Run_Export_DoesNotWriteToOfficialPath()
    {
        var inputs = new[]
        {
            CreateContractSpecInput(GetFixturePath("pipeline_contract_spec.csv")),
            CreateMarginFeeInput(GetFixturePath("pipeline_margin_fee.csv")),
            CreateMarketStatInput(GetFixturePath("pipeline_market_stat.csv")),
        };

        var pipeline = new ProductDataLocalCompositionPipeline();
        var result = pipeline.Run(inputs, GetDefaultScenarioSet());

        var tempDir = Path.Combine(Path.GetTempPath(), $"SmallFuturesLab_Test_{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);
        try
        {
            var exportPath = Path.Combine(tempDir, "pipeline_test_output.csv");
            var exporter = new ProductFilterCsvExporter();
            exporter.Export(exportPath, result.Rows);

            Assert.False(File.Exists("data/product_filter/product_filter_batch1.csv"));
            Assert.False(File.Exists("reports/product_filter_batch1_summary.md"));
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    #endregion

    #region null input 校验

    [Fact]
    public void Run_NullInputInCollection_Throws_ArgumentNullException()
    {
        var pipeline = new ProductDataLocalCompositionPipeline();
        var inputs = new ProductDataSourceInput?[] { null! };

        var ex = Assert.Throws<ArgumentNullException>(() => pipeline.Run(inputs!, GetDefaultScenarioSet()));
        Assert.Contains("input", ex.Message);
    }

    #endregion
}

