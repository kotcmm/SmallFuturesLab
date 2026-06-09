namespace SmallFuturesLab.TradingPlanet;

using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using SmallFuturesLab.Core.Abstractions;
using SmallFuturesLab.Core.Models;

/// <summary>
/// 读取交易星球下载的本地表格文件。
/// 交易星球的 .xls 文件通常是 Excel 可打开的 HTML 表格，本读取器按本地 HTML 表格解析。
/// </summary>
public sealed class TradingPlanetTableReader : IProductDataReader
{
    private static readonly Regex RowRegex = new("<tr[^>]*>(.*?)</tr>", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
    private static readonly Regex CellRegex = new("<t[dh][^>]*>(.*?)</t[dh]>", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
    private static readonly Regex TagRegex = new("<.*?>", RegexOptions.Singleline | RegexOptions.Compiled);
    private static readonly Regex ContractCodeRegex = new("^[A-Za-z]+[0-9]+$", RegexOptions.Compiled);
    private static readonly Regex ProductCodeRegex = new("^[A-Za-z]+", RegexOptions.Compiled);

    /// <summary>
    /// 从交易星球本地下载文件读取品种信息。
    /// </summary>
    /// <param name="filePath">交易星球下载的 .xls 或 HTML 表格路径。</param>
    /// <returns>品种信息列表。</returns>
    public IReadOnlyList<ProductInfo> Read(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("文件路径不能为空。", nameof(filePath));
        }

        var html = File.ReadAllText(filePath);
        return ParseHtml(html, Path.GetFileName(filePath));
    }

    /// <summary>
    /// 解析交易星球 HTML 表格内容。
    /// </summary>
    /// <param name="html">HTML 内容。</param>
    /// <param name="sourceName">来源文件名。</param>
    /// <returns>品种信息列表。</returns>
    public IReadOnlyList<ProductInfo> ParseHtml(string html, string sourceName = "交易星球本地表格")
    {
        if (string.IsNullOrWhiteSpace(html))
        {
            return Array.Empty<ProductInfo>();
        }

        var products = new List<ProductInfo>();
        var exchange = string.Empty;
        var dataDate = ExtractDataDate(sourceName);

        foreach (Match rowMatch in RowRegex.Matches(html))
        {
            var cells = CellRegex.Matches(rowMatch.Groups[1].Value)
                .Select(m => CleanCell(m.Groups[1].Value))
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList();

            if (cells.Count == 0)
            {
                continue;
            }

            if (cells.Count == 1 && cells[0].Contains("交易所", StringComparison.Ordinal))
            {
                exchange = cells[0];
                continue;
            }

            if (cells.Count < 14)
            {
                continue;
            }

            var contractCode = cells[1].Trim();
            if (!ContractCodeRegex.IsMatch(contractCode))
            {
                continue;
            }

            var productCode = ProductCodeRegex.Match(contractCode).Value.ToUpperInvariant();
            var price = ParseNumber(cells[2]);
            var volume = ParseNumber(cells[3]);
            var buyMarginRate = ParsePercent(cells[4]);
            var sellMarginRate = ParsePercent(cells[5]);
            var marginRate = Math.Max(buyMarginRate, sellMarginRate);
            var marginPerLot = ParseNumber(cells[6]);
            var openFee = ParseNumber(cells[7]);
            var closeYesterdayFee = ParseNumber(cells[8]);
            var closeTodayFee = ParseNumber(cells[9]);
            var roundTripFee = ParseNumber(cells[10]);
            var tickValue = ParseNumber(cells[11]);
            var remark = cells.Count > 13 ? cells[13] : string.Empty;

            if (roundTripFee <= 0)
            {
                var closeFee = closeTodayFee > 0 ? closeTodayFee : closeYesterdayFee;
                roundTripFee = openFee + closeFee;
            }

            products.Add(new ProductInfo
            {
                Exchange = exchange,
                ProductName = cells[0],
                ProductCode = productCode,
                ContractCode = contractCode,
                Price = price,
                TickValue = tickValue,
                MarginRate = marginRate,
                MarginPerLot = marginPerLot,
                RoundTripFeePerLot = roundTripFee,
                OpenFeePerLot = openFee,
                CloseYesterdayFeePerLot = closeYesterdayFee,
                CloseTodayFeePerLot = closeTodayFee,
                Volume = volume,
                IsMainContract = remark.Contains("主力", StringComparison.Ordinal),
                DataDate = dataDate,
                DataSource = $"交易星球本地下载表格：{sourceName}",
                Remark = remark,
            });
        }

        return products;
    }

    private static string CleanCell(string value)
    {
        var withoutTags = TagRegex.Replace(value, string.Empty);
        var decoded = WebUtility.HtmlDecode(withoutTags);
        return decoded.Replace("\u00A0", " ").Trim();
    }

    private static double ParseNumber(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return 0;
        }

        var normalized = text
            .Replace(",", string.Empty, StringComparison.Ordinal)
            .Replace("元", string.Empty, StringComparison.Ordinal)
            .Replace("手", string.Empty, StringComparison.Ordinal)
            .Replace("约", string.Empty, StringComparison.Ordinal)
            .Trim();

        var match = Regex.Match(normalized, @"-?\d+(\.\d+)?");
        if (!match.Success)
        {
            return 0;
        }

        return double.TryParse(match.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result)
            ? result
            : 0;
    }

    private static double ParsePercent(string text)
    {
        var value = ParseNumber(text);
        if (value <= 0)
        {
            return 0;
        }

        return text.Contains('%', StringComparison.Ordinal) ? value / 100.0 : value;
    }

    private static string ExtractDataDate(string sourceName)
    {
        var match = Regex.Match(sourceName, @"(20\d{2})年?(\d{2})月?(\d{2})日?");
        if (match.Success)
        {
            return $"{match.Groups[1].Value}-{match.Groups[2].Value}-{match.Groups[3].Value}";
        }

        return string.Empty;
    }
}
