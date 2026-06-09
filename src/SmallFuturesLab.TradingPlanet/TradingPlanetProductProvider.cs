using SmallFuturesLab.Core.Data;
using SmallFuturesLab.Core.Products;

namespace SmallFuturesLab.TradingPlanet;

/// <summary>
/// 基于交易星球本地文件的品种信息提供者。
/// </summary>
public sealed class TradingPlanetProductProvider : IProductProvider
{
    private readonly string _filePath;
    private readonly TradingPlanetFileReader _reader = new();

    /// <summary>
    /// 创建提供者。
    /// </summary>
    public TradingPlanetProductProvider(string filePath)
    {
        _filePath = filePath;
    }

    /// <inheritdoc />
    public IReadOnlyList<ProductInfo> GetProducts()
    {
        var result = _reader.Read(_filePath);
        return result.Items.Select(x => x.Product).ToArray();
    }
}
