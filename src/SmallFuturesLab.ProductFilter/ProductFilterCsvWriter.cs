using System.Globalization;
using System.Text;

namespace SmallFuturesLab.ProductFilter;

/// <summary>
/// 品种筛选 CSV 写出器，将计算后的数据写出为与模板格式一致的 CSV。
/// </summary>
public class ProductFilterCsvWriter
{
    /// <summary>
    /// 写出 CSV 文件。
    /// </summary>
    /// <param name="filePath">输出文件路径。</param>
    /// <param name="rows">数据行列表。</param>
    public void Write(string filePath, IReadOnlyList<ProductFilterRow> rows)
    {
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var sb = new StringBuilder();
        sb.AppendLine(string.Join(",", ProductFilterCsvHeader.ExpectedHeaders));

        foreach (var row in rows)
        {
            var values = new List<string>
            {
                Escape(row.Exchange),
                Escape(row.ProductName),
                Escape(row.ProductCode),
                Escape(row.ContractCode),
                FormatDouble(row.Price),
                FormatDouble(row.Multiplier),
                FormatDouble(row.TickSize),
                FormatDouble(row.TickValue),
                FormatDouble(row.MarginRate),
                FormatDouble(row.MarginPerLot),
                FormatDouble(row.RoundTripFeePerLot),
                row.SlippageTicks.ToString(CultureInfo.InvariantCulture),
                FormatDouble(row.TypicalAtr),
                FormatDouble(row.AtrMoneyPerLot),
                FormatDouble(row.StopDistance),
                FormatDouble(row.StopRiskMoney),
                FormatDouble(row.SlippageMoney),
                FormatDouble(row.CostMoney),
                FormatDouble(row.TotalRiskMoney),
                FormatDouble(row.AccountEquity),
                FormatDouble(row.RiskRate),
                FormatDouble(row.MarginRateOfEquity),
                FormatDouble(row.CostRatio),
                Escape(row.LiquidityLevel.ToString()),
                Escape(row.BookContinuityLevel.ToString()),
                Escape(row.RolloverClarity.ToString()),
                Escape(row.Result.ToString()),
                Escape(row.Reasons),
                Escape(row.DataDate),
                Escape(row.DataSource),
            };

            sb.AppendLine(string.Join(",", values));
        }

        File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
    }

    private static string FormatDouble(double value)
    {
        return value.ToString(CultureInfo.InvariantCulture);
    }

    private static string Escape(string value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        if (value.Contains('"'))
        {
            value = value.Replace("\"", "\"\"");
        }

        if (value.Contains(',') || value.Contains('\n') || value.Contains('\r') || value.Contains('"'))
        {
            value = $"\"{value}\"";
        }

        return value;
    }
}
