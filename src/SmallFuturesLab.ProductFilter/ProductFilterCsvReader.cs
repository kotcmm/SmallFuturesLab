using System.Globalization;

namespace SmallFuturesLab.ProductFilter;

/// <summary>
/// 品种筛选 CSV 读取器。
/// </summary>
public class ProductFilterCsvReader
{
    /// <summary>
    /// 读取 CSV 文件。
    /// </summary>
    /// <param name="filePath">文件路径。</param>
    /// <returns>读取结果。</returns>
    public CsvReadResult Read(string filePath)
    {
        var errors = new List<ProductFilterValidationError>();
        var rows = new List<ProductFilterRow>();

        var lines = File.ReadAllLines(filePath);
        if (lines.Length == 0)
        {
            errors.Add(new ProductFilterValidationError
            {
                RowNumber = 0,
                FieldName = "表头",
                Reason = "CSV 文件为空",
            });
            return new CsvReadResult { IsSuccess = false, Errors = errors.AsReadOnly() };
        }

        var headers = ParseLine(lines[0]);
        var headerErrors = ValidateHeaders(headers);
        errors.AddRange(headerErrors);

        if (headerErrors.Count > 0)
        {
            return new CsvReadResult { IsSuccess = false, Errors = errors.AsReadOnly() };
        }

        var headerIndex = headers.Select((h, i) => (h, i)).ToDictionary(x => x.h, x => x.i, StringComparer.Ordinal);

        for (int i = 1; i < lines.Length; i++)
        {
            var line = lines[i];
            if (string.IsNullOrWhiteSpace(line))
                continue;

            var values = ParseLine(line);
            if (values.Length < headers.Length)
            {
                errors.Add(new ProductFilterValidationError
                {
                    RowNumber = i + 1,
                    FieldName = "行数据",
                    Reason = "字段数量少于表头",
                });
                continue;
            }

            var row = ParseRow(values, headerIndex, i + 1, errors);
            if (row != null)
            {
                rows.Add(row);
            }
        }

        if (errors.Count > 0)
        {
            return new CsvReadResult { IsSuccess = false, Errors = errors.AsReadOnly() };
        }

        return new CsvReadResult { IsSuccess = true, Rows = rows.AsReadOnly() };
    }

    private static List<ProductFilterValidationError> ValidateHeaders(string[] headers)
    {
        var errors = new List<ProductFilterValidationError>();
        var expected = ProductFilterCsvHeader.ExpectedHeaders;

        if (headers.Length != expected.Count)
        {
            errors.Add(new ProductFilterValidationError
            {
                RowNumber = 1,
                FieldName = "表头",
                Reason = $"表头字段数量不匹配，期望 {expected.Count} 个，实际 {headers.Length} 个",
            });
        }

        int minLen = Math.Min(headers.Length, expected.Count);
        for (int i = 0; i < minLen; i++)
        {
            if (!string.Equals(headers[i], expected[i], StringComparison.Ordinal))
            {
                errors.Add(new ProductFilterValidationError
                {
                    RowNumber = 1,
                    FieldName = $"第 {i + 1} 列",
                    Reason = $"表头字段顺序错误，期望 '{expected[i]}'，实际 '{headers[i]}'",
                });
            }
        }

        var actualSet = new HashSet<string>(headers, StringComparer.Ordinal);
        var expectedSet = new HashSet<string>(expected, StringComparer.Ordinal);
        var unknownFields = actualSet.Except(expectedSet, StringComparer.Ordinal).ToList();
        foreach (var field in unknownFields)
        {
            errors.Add(new ProductFilterValidationError
            {
                RowNumber = 1,
                FieldName = field,
                Reason = "表头包含未知字段",
            });
        }

        var missingFields = expectedSet.Except(actualSet, StringComparer.Ordinal).ToList();
        foreach (var field in missingFields)
        {
            errors.Add(new ProductFilterValidationError
            {
                RowNumber = 1,
                FieldName = field,
                Reason = "表头缺少字段",
            });
        }

        return errors;
    }

    private static ProductFilterRow? ParseRow(
        string[] values,
        Dictionary<string, int> headerIndex,
        int rowNumber,
        List<ProductFilterValidationError> errors)
    {
        var row = new ProductFilterRow();
        var hasError = false;

        foreach (var field in ProductFilterCsvHeader.RequiredFields)
        {
            if (!headerIndex.TryGetValue(field, out var idx))
                continue;

            var value = idx < values.Length ? values[idx] : string.Empty;
            if (string.IsNullOrWhiteSpace(value))
            {
                errors.Add(new ProductFilterValidationError
                {
                    RowNumber = rowNumber,
                    FieldName = field,
                    Reason = "必填字段为空",
                });
                hasError = true;
            }
        }

        if (hasError)
            return null;

        try
        {
            row = row with
            {
                Exchange = GetString(values, headerIndex, "Exchange"),
                ProductName = GetString(values, headerIndex, "ProductName"),
                ProductCode = GetString(values, headerIndex, "ProductCode"),
                ContractCode = GetString(values, headerIndex, "ContractCode"),
                Price = GetDouble(values, headerIndex, "Price"),
                Multiplier = GetDouble(values, headerIndex, "Multiplier"),
                TickSize = GetDouble(values, headerIndex, "TickSize"),
                MarginRate = GetDouble(values, headerIndex, "MarginRate"),
                RoundTripFeePerLot = GetDouble(values, headerIndex, "RoundTripFeePerLot"),
                SlippageTicks = GetInt(values, headerIndex, "SlippageTicks"),
                TypicalAtr = GetDouble(values, headerIndex, "TypicalAtr"),
                StopDistance = GetDouble(values, headerIndex, "StopDistance"),
                AccountEquity = GetDouble(values, headerIndex, "AccountEquity"),
                LiquidityLevel = GetEnum<LiquidityLevel>(values, headerIndex, "LiquidityLevel"),
                BookContinuityLevel = GetEnum<BookContinuityLevel>(values, headerIndex, "BookContinuityLevel"),
                RolloverClarity = GetEnum<RolloverClarity>(values, headerIndex, "RolloverClarity"),
                DataDate = GetString(values, headerIndex, "DataDate"),
                DataSource = GetString(values, headerIndex, "DataSource"),
            };
        }
        catch (FormatException ex)
        {
            errors.Add(new ProductFilterValidationError
            {
                RowNumber = rowNumber,
                FieldName = ExtractFieldNameFromException(ex),
                Reason = ex.Message,
            });
            return null;
        }

        return row;
    }

    private static string ExtractFieldNameFromException(FormatException ex)
    {
        var message = ex.Message;
        // 异常消息格式为 "{FieldName} 不可解析为..." 或 "{FieldName} 不是有效的枚举值..."
        var spaceIndex = message.IndexOf(' ');
        if (spaceIndex > 0)
        {
            return message.Substring(0, spaceIndex);
        }
        return "字段";
    }

    private static string GetString(string[] values, Dictionary<string, int> index, string field)
    {
        return index.TryGetValue(field, out var idx) && idx < values.Length ? values[idx] : string.Empty;
    }

    private static double GetDouble(string[] values, Dictionary<string, int> index, string field)
    {
        var val = GetString(values, index, field);
        if (string.IsNullOrWhiteSpace(val))
            return 0;
        if (!double.TryParse(val, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
            throw new FormatException($"{field} 不可解析为数值: '{val}'");
        return result;
    }

    private static int GetInt(string[] values, Dictionary<string, int> index, string field)
    {
        var val = GetString(values, index, field);
        if (string.IsNullOrWhiteSpace(val))
            return 0;
        if (!int.TryParse(val, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
            throw new FormatException($"{field} 不可解析为整数: '{val}'");
        return result;
    }

    private static T GetEnum<T>(string[] values, Dictionary<string, int> index, string field) where T : struct, Enum
    {
        var val = GetString(values, index, field);
        if (string.IsNullOrWhiteSpace(val))
            return default;
        if (!Enum.TryParse<T>(val, true, out var result))
            throw new FormatException($"{field} 不是有效的枚举值: '{val}'");
        return result;
    }

    private static string[] ParseLine(string line)
    {
        var result = new List<string>();
        var current = new System.Text.StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            if (c == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    current.Append('"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c == ',' && !inQuotes)
            {
                result.Add(current.ToString());
                current.Clear();
            }
            else
            {
                current.Append(c);
            }
        }

        result.Add(current.ToString());
        return result.ToArray();
    }
}
