using SmallFuturesLab.Core.Products;

namespace SmallFuturesLab.Core.Data;

/// <summary>
/// 品种信息提供者。
/// 具体数据可以来自本地文件、网络、数据库、CTP 或测试内存对象。
/// Core 不关心数据来源。
/// </summary>
public interface IProductProvider
{
    /// <summary>
    /// 获取品种信息。
    /// </summary>
    IReadOnlyList<ProductInfo> GetProducts();
}
