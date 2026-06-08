using SmallFuturesLab.ProductData.Abstractions;
namespace SmallFuturesLab.ProductData.Pipeline;

/// <summary>
/// 本地数据源输入，表示组合管线中的一个数据来源。
/// </summary>
public record ProductDataSourceInput
{
    private string _name = string.Empty;
    private IProductDataSource _source = null!;
    private string _filePath = string.Empty;

    /// <summary>
    /// 输入名称，不能为空。
    /// </summary>
    /// <exception cref="ArgumentException">设置为空字符串时抛出。</exception>
    public string Name
    {
        get => _name;
        init => _name = !string.IsNullOrWhiteSpace(value)
            ? value
            : throw new ArgumentException("Name 不能为空", nameof(Name));
    }

    /// <summary>
    /// 数据源适配器，不能为 null。
    /// </summary>
    /// <exception cref="ArgumentNullException">设置为 null 时抛出。</exception>
    public IProductDataSource Source
    {
        get => _source;
        init => _source = value ?? throw new ArgumentNullException(nameof(Source));
    }

    /// <summary>
    /// 本地文件路径，不能为空。
    /// </summary>
    /// <exception cref="ArgumentException">设置为空字符串时抛出。</exception>
    public string FilePath
    {
        get => _filePath;
        init => _filePath = !string.IsNullOrWhiteSpace(value)
            ? value
            : throw new ArgumentException("FilePath 不能为空", nameof(FilePath));
    }
}
