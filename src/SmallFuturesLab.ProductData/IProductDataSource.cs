namespace SmallFuturesLab.ProductData;

/// <summary>
/// 品种数据源接口。所有数据源适配器必须实现此接口。
/// </summary>
public interface IProductDataSource
{
    /// <summary>
    /// 从指定路径读取数据并返回读取结果。
    /// </summary>
    /// <param name="filePath">本地文件路径。</param>
    /// <returns>读取结果，包含记录和错误。</returns>
    ProductDataReadResult Read(string filePath);
}
