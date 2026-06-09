namespace SmallFuturesLab.Core.Abstractions;

using SmallFuturesLab.Core.Models;

/// <summary>
/// 品种数据读取接口。
/// Core 只定义抽象，不关心数据来自交易星球、CSV、CTP 还是其他来源。
/// </summary>
public interface IProductDataReader
{
    /// <summary>
    /// 从本地文件读取品种信息。
    /// </summary>
    /// <param name="filePath">本地文件路径。</param>
    /// <returns>读取出的品种信息列表。</returns>
    IReadOnlyList<ProductInfo> Read(string filePath);
}
