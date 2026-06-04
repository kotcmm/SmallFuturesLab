namespace SmallFuturesLab.ProductData;

/// <summary>
/// 品种数据源接口。所有数据源适配器必须实现此接口。
/// </summary>
public interface IProductDataSource
{
    /// <summary>
    /// 从指定路径读取数据并返回品种数据记录列表。
    /// </summary>
    /// <param name="filePath">本地文件路径。</param>
    /// <returns>品种数据记录列表。</returns>
    IReadOnlyList<ProductDataRecord> Read(string filePath);
}
