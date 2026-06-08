namespace SmallFuturesLab.ProductData;

/// <summary>
/// 品种数据合并主键，由 ProductCode 和 ContractCode 组成。
/// </summary>
public record ProductDataMergeKey
{
    private string _productCode = string.Empty;
    private string _contractCode = string.Empty;

    /// <summary>
    /// 品种代码，不能为空。
    /// </summary>
    public string ProductCode
    {
        get => _productCode;
        init
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("ProductCode 不能为空", nameof(value));
            }

            _productCode = value;
        }
    }

    /// <summary>
    /// 合约代码，不能为空。
    /// </summary>
    public string ContractCode
    {
        get => _contractCode;
        init
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("ContractCode 不能为空", nameof(value));
            }

            _contractCode = value;
        }
    }

    /// <summary>
    /// 从品种数据记录创建合并主键。
    /// </summary>
    /// <param name="record">品种数据记录。</param>
    /// <returns>合并主键。</returns>
    /// <exception cref="ArgumentNullException">当 record 为 null 时抛出。</exception>
    /// <exception cref="ArgumentException">当 ProductCode 或 ContractCode 为空时抛出。</exception>
    public static ProductDataMergeKey From(ProductDataRecord record)
    {
        ArgumentNullException.ThrowIfNull(record);

        return new ProductDataMergeKey
        {
            ProductCode = record.ProductCode,
            ContractCode = record.ContractCode,
        };
    }
}
