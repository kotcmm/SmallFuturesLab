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
    /// <returns>品种数据记录列表。</returns>
    public IReadOnlyList<ProductDataRecord> Read(string filePath)
    {
        var html = File.ReadAllText(filePath);
        var records = new List<ProductDataRecord>();

        // 简化解析：匹配 tbody 中的每一行 tr
        var tbodyMatch = Regex.Match(html, @"<tbody>(.*?)</tbody>", RegexOptions.Singleline);
        if (!tbodyMatch.Success)
            return records;

        var rowMatches = Regex.Matches(tbodyMatch.Groups[1].Value, @"<tr>(.*?)</tr>", RegexOptions.Singleline);
        foreach (Match rowMatch in rowMatches)
        {
            var cells = ExtractCells(rowMatch.Groups[1].Value);
            if (cells.Count < 12)
                continue;

            var record = new ProductDataRecord
            {
                ProductName = cells[0],
                ProductCode = cells[1],
                ContractCode = cells[2],
                Price = ParseDouble(cells[3]),
                Volume = ParseDouble(cells[4]),
                MarginRate = ParseDouble(cells[5]),
                MarginPerLot = ParseDouble(cells[6]),
                OpenFeePerLot = ParseDouble(cells[7]),
                CloseYesterdayFeePerLot = ParseDouble(cells[8]),
                CloseTodayFeePerLot = ParseDouble(cells[9]),
                RoundTripFeePerLot = ParseDouble(cells[10]),
                IsMainContract = cells[11].Contains("是"),
                DataSource = "交易星球手续费页面",
                DataSourceType = ProductDataSourceType.ThirdPartyResearch,
                NeedsReview = true,
            };

            records.Add(record);
        }

        return records;
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

    private static double ParseDouble(string value)
    {
        if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
            return result;
        return 0;
    }
}
