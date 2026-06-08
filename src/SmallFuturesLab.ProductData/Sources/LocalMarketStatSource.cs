using SmallFuturesLab.ProductData.Models;
using SmallFuturesLab.ProductData.Abstractions;
using SmallFuturesLab.ProductData.Reading;
using System.Globalization;
using SmallFuturesLab.ProductFilter;

namespace SmallFuturesLab.ProductData.Sources;

/// <summary>
/// 本地行情统计数据源适配器。只读取本地 CSV 文件，不联网，不读取实时行情。
/// </summary>
public class LocalMarketStatSource : IProductDataSource
{
    private static readonly string[] RequiredHeaders = new[]
    {
        "Exchange", "ProductName", "ProductCode", "ContractCode", "Price", "TypicalAtr",
        "Volume", "OpenInterest", "LiquidityLevel", "BookContinuityLevel", "RolloverClarity",
        "DataDate", "DataSource", "NeedsReview",
    };

    /// <summary>
    /// 从本地 CSV 文件中读取行情统计数据。
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

        var headers = lines[0].Split(',').Select(h => h.Trim()).ToArray();
        var headerIndex = headers.Select((h, i) => (h, i)).ToDictionary(x => x.Item1, x => x.Item2);

        foreach (var required in RequiredHeaders)
        {
            if (!headerIndex.ContainsKey(required))
            {
                errors.Add(new ProductDataReadError
                {
                    RowNumber = 1,
                    FieldName = "表头校验",
                    Reason = $"缺失必填表头: '{required}'",
                });
            }
        }

        if (errors.Count > 0)
        {
            return new ProductDataReadResult { Records = records, Errors = errors };
        }

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

            var exchange = ValidateRequiredString(GetValue(values, headerIndex, "Exchange"), "Exchange", rowNumber, rowErrors);
            var productName = ValidateRequiredString(GetValue(values, headerIndex, "ProductName"), "ProductName", rowNumber, rowErrors);
            var productCode = ValidateRequiredString(GetValue(values, headerIndex, "ProductCode"), "ProductCode", rowNumber, rowErrors);
            var contractCode = ValidateRequiredString(GetValue(values, headerIndex, "ContractCode"), "ContractCode", rowNumber, rowErrors);
            var price = TryParseDouble(GetValue(values, headerIndex, "Price"), "Price", rowNumber, rowErrors);
            var typicalAtr = TryParseDouble(GetValue(values, headerIndex, "TypicalAtr"), "TypicalAtr", rowNumber, rowErrors);
            var volume = TryParseDouble(GetValue(values, headerIndex, "Volume"), "Volume", rowNumber, rowErrors);
            var openInterest = TryParseDouble(GetValue(values, headerIndex, "OpenInterest"), "OpenInterest", rowNumber, rowErrors);
            var liquidityLevel = TryParseEnum<LiquidityLevel>(GetValue(values, headerIndex, "LiquidityLevel"), "LiquidityLevel", rowNumber, rowErrors);
            var bookContinuityLevel = TryParseEnum<BookContinuityLevel>(GetValue(values, headerIndex, "BookContinuityLevel"), "BookContinuityLevel", rowNumber, rowErrors);
            var rolloverClarity = TryParseEnum<RolloverClarity>(GetValue(values, headerIndex, "RolloverClarity"), "RolloverClarity", rowNumber, rowErrors);
            var dataDate = ValidateRequiredString(GetValue(values, headerIndex, "DataDate"), "DataDate", rowNumber, rowErrors);
            var dataSource = ValidateRequiredString(GetValue(values, headerIndex, "DataSource"), "DataSource", rowNumber, rowErrors);
            var needsReview = TryParseBool(GetValue(values, headerIndex, "NeedsReview"), "NeedsReview", rowNumber, rowErrors);

            if (price.HasValue) price = ValidatePositiveFinite(price.Value, "Price", rowNumber, rowErrors);
            if (typicalAtr.HasValue) typicalAtr = ValidatePositiveFinite(typicalAtr.Value, "TypicalAtr", rowNumber, rowErrors);
            if (volume.HasValue) volume = ValidateNonNegativeFinite(volume.Value, "Volume", rowNumber, rowErrors);
            if (openInterest.HasValue) openInterest = ValidateNonNegativeFinite(openInterest.Value, "OpenInterest", rowNumber, rowErrors);

            if (rowErrors.Count > 0
                || exchange is null
                || productName is null
                || productCode is null
                || contractCode is null
                || !price.HasValue
                || !typicalAtr.HasValue
                || !volume.HasValue
                || !openInterest.HasValue
                || !liquidityLevel.HasValue
                || !bookContinuityLevel.HasValue
                || !rolloverClarity.HasValue
                || dataDate is null
                || dataSource is null
                || !needsReview.HasValue)
            {
                errors.AddRange(rowErrors);
                continue;
            }

            var record = new ProductDataRecord
            {
                Exchange = exchange,
                ProductName = productName,
                ProductCode = productCode,
                ContractCode = contractCode,
                Price = price.Value,
                TypicalAtr = typicalAtr.Value,
                Volume = volume.Value,
                OpenInterest = openInterest.Value,
                LiquidityLevel = liquidityLevel.Value,
                BookContinuityLevel = bookContinuityLevel.Value,
                RolloverClarity = rolloverClarity.Value,
                DataDate = dataDate,
                DataSource = dataSource,
                NeedsReview = needsReview.Value,
                DataSourceType = ProductDataSourceType.MarketDataApi,
            };

            records.Add(record);
        }

        return new ProductDataReadResult { Records = records, Errors = errors };
    }

    private static string? ValidateRequiredString(string value, string fieldName, int rowNumber, List<ProductDataReadError> errors)
    {
        if (!string.IsNullOrWhiteSpace(value))
            return value;

        errors.Add(new ProductDataReadError
        {
            RowNumber = rowNumber,
            FieldName = fieldName,
            Reason = $"字段不能为空: '{value}'",
        });
        return null;
    }

    private static double? ValidatePositiveFinite(double value, string fieldName, int rowNumber, List<ProductDataReadError> errors)
    {
        if (!double.IsNaN(value) && !double.IsInfinity(value) && value > 0)
            return value;

        errors.Add(new ProductDataReadError
        {
            RowNumber = rowNumber,
            FieldName = fieldName,
            Reason = $"必须是有限数字且大于 0: {value}",
        });
        return null;
    }

    private static double? ValidateNonNegativeFinite(double value, string fieldName, int rowNumber, List<ProductDataReadError> errors)
    {
        if (!double.IsNaN(value) && !double.IsInfinity(value) && value >= 0)
            return value;

        errors.Add(new ProductDataReadError
        {
            RowNumber = rowNumber,
            FieldName = fieldName,
            Reason = $"必须是有限数字且不能为负数: {value}",
        });
        return null;
    }

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

    private static T? TryParseEnum<T>(string value, string fieldName, int rowNumber, List<ProductDataReadError> errors) where T : struct, Enum
    {
        if (Enum.TryParse<T>(value, out var result) && Enum.IsDefined(result))
            return result;

        errors.Add(new ProductDataReadError
        {
            RowNumber = rowNumber,
            FieldName = fieldName,
            Reason = $"不可解析为有效枚举值: '{value}'",
        });
        return null;
    }
}
