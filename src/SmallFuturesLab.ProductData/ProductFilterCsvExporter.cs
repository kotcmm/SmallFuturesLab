using SmallFuturesLab.ProductFilter;

namespace SmallFuturesLab.ProductData;

/// <summary>
/// 品种筛选 CSV 导出器，将标准化后的 ProductFilterRow 写成模板兼容的 CSV 文件。
/// 复用 ProductFilterCsvWriter，不做重复实现。
/// </summary>
public class ProductFilterCsvExporter
{
    private readonly ProductFilterCsvWriter _writer = new();

    /// <summary>
    /// 导出 CSV 文件。
    /// </summary>
    /// <param name="filePath">输出文件路径。</param>
    /// <param name="rows">品种筛选行数据列表。</param>
    public void Export(string filePath, IReadOnlyList<ProductFilterRow> rows)
    {
        _writer.Write(filePath, rows);
    }
}
