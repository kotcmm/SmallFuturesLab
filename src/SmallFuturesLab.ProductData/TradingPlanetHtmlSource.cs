using System.Globalization;
using System.Text.RegularExpressions;

namespace SmallFuturesLab.ProductData;

/// <summary>
/// 交易星球 HTML 数据源适配器。只解析本地 HTML 文件，不联网。
/// </summary>
public class TradingPlanetHtmlSource : IProductDataSource
{
    /// <summary>
    /// 从本地 HTML 文件中解析品种数据记录。
    /// </summary>
    /// <param name="filePath">本地 HTML 文件路径。</param>
    /// <returns>读取结果，包含记录和错误。</returns>
    public ProductDataReadResult Read(string filePath)
    {
        var html = File.ReadAllText(filePath);
        var records = new List<ProductDataRecord>();
        var errors = new List<ProductDataReadError>();

        // 简化解析：匹配 tbody 中的每一行 tr
        var tbodyMatch = Regex.Match(html, @"<tbody>(.*?)</tbody>", RegexOptions.Singleline);
        if (!tbodyMatch.Success)
        {
            return new ProductDataReadResult { Records = records, Errors = errors };
        }

        var rowMatches = Regex.Matches(tbodyMatch.Groups[1].Value, @"<tr>(.*?)</tr>", RegexOptions.Singleline);
        for (int rowIndex = 0; rowIndex < rowMatches.Count; rowIndex++)
        {
            var rowMatch = rowMatches[rowIndex];
            var rowNumber = rowIndex + 2; // HTML 表格数据行从第 2 行开始（第 1 行为 thead）
            var cells = ExtractCells(rowMatch.Groups[1].Value);
            if (cells.Count < 12)
            {
                errors.Add(new ProductDataReadError
                {
                    RowNumber = rowNumber,
                    FieldName = "行数据",
                    Reason = $"字段数量不足，期望 12 个，实际 {cells.Count} 个",
                });
                continue;
            }

            var rowErrors = new List<ProductDataReadError>();
            var price = TryParseDouble(cells[3], "Price", rowNumber, rowErrors);
            var volume = TryParseDouble(cells[4], "Volume", rowNumber, rowErrors);
            var marginRate = TryParseDouble(cells[5], "MarginRate", rowNumber, rowErrors);
            var marginPerLot = TryParseDouble(cells[6], "MarginPerLot", rowNumber, rowErrors);
            var openFee = TryParseDouble(cells[7], "OpenFeePerLot", rowNumber, rowErrors);
            var closeYesterdayFee = TryParseDouble(cells[8], "CloseYesterdayFeePerLot", rowNumber, rowErrors);
            var closeTodayFee = TryParseDouble(cells[9], "CloseTodayFeePerLot", rowNumber, rowErrors);
            var roundTripFee = TryParseDouble(cells[10], "RoundTripFeePerLot", rowNumber, rowErrors);

            if (rowErrors.Count > 0 || !price.HasValue || !volume.HasValue || !marginRate.HasValue || !marginPerLot.HasValue || !openFee.HasValue || !closeYesterdayFee.HasValue || !closeTodayFee.HasValue || !roundTripFee.HasValue)
            {
                errors.AddRange(rowErrors);
                continue;
            }

            var record = new ProductDataRecord
            {
                ProductName = cells[0],
                ProductCode = cells[1],
                ContractCode = cells[2],
                Price = price.Value,
                Volume = volume.Value,
                MarginRate = marginRate.Value,
                MarginPerLot = marginPerLot.Value,
                OpenFeePerLot = openFee.Value,
                CloseYesterdayFeePerLot = closeYesterdayFee.Value,
                CloseTodayFeePerLot = closeTodayFee.Value,
                RoundTripFeePerLot = roundTripFee.Value,
                IsMainContract = cells[11].Contains("是"),
                DataSource = "交易星球手续费页面",
                DataSourceType = ProductDataSourceType.ThirdPartyResearch,
                NeedsReview = true,
            };

            records.Add(record);
        }

        return new ProductDataReadResult { Records = records, Errors = errors };
    }

    private static List<string> ExtractCells(string rowHtml)
    {
        var cells = new List<string>();
        var matches = Regex.Matches(rowHtml, @"<td>(.*?)</td>", RegexOptions.Singleline);
        foreach (Match match in matches)
        {
            cells.Add(match.Groups[1].Value.Trim());
        }
        return cells;
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
}
