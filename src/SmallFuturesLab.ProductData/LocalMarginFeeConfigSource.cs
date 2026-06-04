using System.Globalization;

namespace SmallFuturesLab.ProductData;

/// <summary>
/// 本地保证金手续费配置 CSV 数据源适配器。
/// </summary>
public class LocalMarginFeeConfigSource : IProductDataSource
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
            var needsReview = TryParseBool(GetValue(values, headerIndex, "NeedsReview"), "NeedsReview", rowNumber, rowErrors);

            if (rowErrors.Count > 0 || !marginRate.HasValue || !roundTripFee.HasValue || !needsReview.HasValue)
            {
                errors.AddRange(rowErrors);
                continue;
            }

            var record = new ProductDataRecord
            {
                Exchange = GetValue(values, headerIndex, "Exchange"),
                ProductCode = GetValue(values, headerIndex, "ProductCode"),
                MarginRate = marginRate.Value,
                RoundTripFeePerLot = roundTripFee.Value,
                DataDate = GetValue(values, headerIndex, "DataDate"),
                DataSource = GetValue(values, headerIndex, "DataSource"),
                NeedsReview = needsReview.Value,
                DataSourceType = ProductDataSourceType.ManualConfig,
            };

            records.Add(record);
        }

        return new ProductDataReadResult { Records = records, Errors = errors };
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
}
