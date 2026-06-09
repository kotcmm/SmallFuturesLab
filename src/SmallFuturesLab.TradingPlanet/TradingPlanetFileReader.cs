using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using SmallFuturesLab.Core.Products;

namespace SmallFuturesLab.TradingPlanet;

/// <summary>
/// 交易星球下载文件读取器。
/// 支持 Excel 可打开的 HTML 表格文件。
/// </summary>
public sealed partial class TradingPlanetFileReader
{
    /// <summary>
    /// 读取交易星球本地下载文件。
    /// </summary>
    public TradingPlanetReadResult Read(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("文件路径不能为空", nameof(filePath));
        }

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("文件不存在", filePath);
        }

        var text = File.ReadAllText(filePath);
        var rows = ExtractRows(text);
        var items = new List<TradingPlanetReadItem>();
        var errors = new List<TradingPlanetReadError>();
        var currentExchange = string.Empty;

        for (var i = 0; i < rows.Count; i++)
        {
            var rowNumber = i + 1;
            var cells = rows[i];

            if (cells.Count == 0)
            {
                continue;
            }

            if (cells.Count == 1 && cells[0].Contains("交易所", StringComparison.Ordinal))
            {
                currentExchange = cells[0];
                continue;
            }

            if (cells.Count < 14)
            {
                continue;
            }

            if (cells[0].Contains("合约名称", StringComparison.Ordinal)
                || cells[1].Contains("合约代码", StringComparison.Ordinal))
            {
                continue;
            }

            var item = TryParseDataRow(rowNumber, currentExchange, cells, errors);
            if (item is not null)
            {
                items.Add(item);
            }
        }

        return new TradingPlanetReadResult
        {
            Items = items,
            Errors = errors,
        };
    }

    private static TradingPlanetReadItem? TryParseDataRow(
        int rowNumber,
        string exchange,
        IReadOnlyList<string> cells,
        List<TradingPlanetReadError> errors)
    {
        var productName = cells[0].Trim();
        var contractCode = cells[1].Trim();
        var productCode = ExtractProductCode(contractCode);
        var rawRemark = cells[13].Trim();

        if (string.IsNullOrWhiteSpace(contractCode))
        {
            AddError(errors, rowNumber, "ContractCode", "合约代码不能为空");
            return null;
        }

        if (string.IsNullOrWhiteSpace(productCode))
        {
            AddError(errors, rowNumber, "ProductCode", "无法从合约代码中解析品种代码");
            return null;
        }

        var price = TryParsePositiveDouble(cells[2], rowNumber, "Price", errors);
        var marginPerLot = TryParsePositiveDouble(cells[6], rowNumber, "MarginPerLot", errors);
        var roundTripFee = TryParseNonNegativeDouble(cells[10], rowNumber, "RoundTripFeePerLot", errors);
        var tickValue = TryParsePositiveDouble(cells[11], rowNumber, "TickValue", errors);

        if (!price.HasValue || !marginPerLot.HasValue || !roundTripFee.HasValue || !tickValue.HasValue)
        {
            return null;
        }

        return new TradingPlanetReadItem
        {
            Product = new ProductInfo
            {
                Identity = new ProductIdentity
                {
                    Exchange = exchange,
                    ProductCode = productCode,
                    ContractCode = contractCode,
                    ProductName = NormalizeProductName(productName),
                },
                Economics = new PerLotEconomics
                {
                    Price = price.Value,
                    MarginPerLot = marginPerLot.Value,
                    TickValue = tickValue.Value,
                    RoundTripFeePerLot = roundTripFee.Value,
                },
            },
            RawRemark = rawRemark,
            NeedsReview = true,
        };
    }

    private static string NormalizeProductName(string value)
    {
        var text = value.Trim();
        var index = text.IndexOf('(', StringComparison.Ordinal);
        return index > 0 ? text[..index] : text;
    }

    private static string ExtractProductCode(string contractCode)
    {
        var match = ProductCodeRegex().Match(contractCode);
        return match.Success ? match.Value.ToUpperInvariant() : string.Empty;
    }

    private static double? TryParsePositiveDouble(
        string value,
        int rowNumber,
        string fieldName,
        List<TradingPlanetReadError> errors)
    {
        var parsed = TryParseDouble(value);
        if (!parsed.HasValue || parsed.Value <= 0)
        {
            AddError(errors, rowNumber, fieldName, "必须是有限正数");
            return null;
        }

        return parsed.Value;
    }

    private static double? TryParseNonNegativeDouble(
        string value,
        int rowNumber,
        string fieldName,
        List<TradingPlanetReadError> errors)
    {
        var parsed = TryParseDouble(value);
        if (!parsed.HasValue || parsed.Value < 0)
        {
            AddError(errors, rowNumber, fieldName, "必须是有限非负数");
            return null;
        }

        return parsed.Value;
    }

    private static double? TryParseDouble(string value)
    {
        var cleaned = value
            .Replace(",", string.Empty, StringComparison.Ordinal)
            .Replace("元", string.Empty, StringComparison.Ordinal)
            .Trim();

        if (cleaned.EndsWith('%'))
        {
            cleaned = cleaned.TrimEnd('%');
        }

        if (!double.TryParse(cleaned, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed))
        {
            return null;
        }

        return double.IsFinite(parsed) ? parsed : null;
    }

    private static void AddError(
        List<TradingPlanetReadError> errors,
        int rowNumber,
        string fieldName,
        string reason)
    {
        errors.Add(new TradingPlanetReadError
        {
            RowNumber = rowNumber,
            FieldName = fieldName,
            Reason = reason,
        });
    }

    private static List<IReadOnlyList<string>> ExtractRows(string text)
    {
        var rows = new List<IReadOnlyList<string>>();

        foreach (Match rowMatch in RowRegex().Matches(text))
        {
            var rowHtml = rowMatch.Groups[1].Value;
            var cells = new List<string>();

            foreach (Match cellMatch in CellRegex().Matches(rowHtml))
            {
                var cellHtml = cellMatch.Groups[1].Value;
                var withoutTags = TagRegex().Replace(cellHtml, string.Empty);
                var decoded = WebUtility.HtmlDecode(withoutTags).Trim();
                cells.Add(decoded);
            }

            rows.Add(cells);
        }

        return rows;
    }

    [GeneratedRegex("<tr[^>]*>(.*?)</tr>", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex RowRegex();

    [GeneratedRegex("<t[dh][^>]*>(.*?)</t[dh]>", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex CellRegex();

    [GeneratedRegex("<.*?>", RegexOptions.Singleline)]
    private static partial Regex TagRegex();

    [GeneratedRegex("^[A-Za-z]+")]
    private static partial Regex ProductCodeRegex();
}
