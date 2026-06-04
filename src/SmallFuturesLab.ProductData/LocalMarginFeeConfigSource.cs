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
    /// <returns>品种数据记录列表。</returns>
    public IReadOnlyList<ProductDataRecord> Read(string filePath)
    {
        var records = new List<ProductDataRecord>();
        var lines = File.ReadAllLines(filePath);
        if (lines.Length < 2)
            return records;

        var headers = lines[0].Split(',');
        var headerIndex = headers.Select((h, i) => (h.Trim(), i)).ToDictionary(x => x.Item1, x => x.Item2);

        for (int i = 1; i < lines.Length; i++)
        {
            var values = lines[i].Split(',');
            if (values.Length < headers.Length)
                continue;

            var record = new ProductDataRecord
            {
                Exchange = GetValue(values, headerIndex, "Exchange"),
                ProductCode = GetValue(values, headerIndex, "ProductCode"),
                MarginRate = ParseDouble(GetValue(values, headerIndex, "MarginRate")),
                RoundTripFeePerLot = ParseDouble(GetValue(values, headerIndex, "RoundTripFeePerLot")),
                DataDate = GetValue(values, headerIndex, "DataDate"),
                DataSource = GetValue(values, headerIndex, "DataSource"),
                NeedsReview = bool.TryParse(GetValue(values, headerIndex, "NeedsReview"), out var nr) && nr,
                DataSourceType = ProductDataSourceType.ManualConfig,
            };

            records.Add(record);
        }

        return records;
    }

    private static string GetValue(string[] values, Dictionary<string, int> index, string field)
    {
        if (index.TryGetValue(field, out var idx) && idx < values.Length)
            return values[idx].Trim();
        return string.Empty;
    }

    private static double ParseDouble(string value)
    {
        if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
            return result;
        return 0;
    }
}
