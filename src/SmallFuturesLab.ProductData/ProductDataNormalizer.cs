using SmallFuturesLab.ProductFilter;

namespace SmallFuturesLab.ProductData;

/// <summary>
/// 品种数据标准化器，负责把 ProductDataRecord 转换成 ProductFilterRow。
/// 不计算公式字段，不判断 Allowed / Caution / Rejected，不生成交易建议。
/// </summary>
public class ProductDataNormalizer
{
    /// <summary>
    /// 将品种数据记录标准化为品种筛选行。
    /// </summary>
    /// <param name="record">品种数据记录。</param>
    /// <param name="accountEquity">账户权益，测算维度。</param>
    /// <param name="stopDistance">测算止损距离。</param>
    /// <param name="slippageTicks">预估滑点 tick 数。</param>
    /// <param name="typicalAtr">典型 ATR。</param>
    /// <returns>标准化结果。</returns>
    public ProductDataNormalizeResult Normalize(
        ProductDataRecord record,
        double accountEquity,
        double stopDistance,
        int slippageTicks,
        double typicalAtr)
    {
        if (!record.Multiplier.HasValue || record.Multiplier.Value <= 0)
        {
            return new ProductDataNormalizeResult
            {
                IsSuccess = false,
                Error = $"品种 {record.ProductCode} 缺少有效的 Multiplier（合约乘数）",
            };
        }

        if (!record.TickSize.HasValue || record.TickSize.Value <= 0)
        {
            return new ProductDataNormalizeResult
            {
                IsSuccess = false,
                Error = $"品种 {record.ProductCode} 缺少有效的 TickSize（最小变动价位）",
            };
        }

        var reasons = new List<string>();
        if (record.NeedsReview)
        {
            reasons.Add("数据需复核");
        }

        var row = new ProductFilterRow
        {
            Exchange = record.Exchange,
            ProductName = record.ProductName,
            ProductCode = record.ProductCode,
            ContractCode = record.ContractCode,
            Price = record.Price,
            Multiplier = record.Multiplier.Value,
            TickSize = record.TickSize.Value,
            MarginRate = record.MarginRate,
            RoundTripFeePerLot = record.RoundTripFeePerLot,
            SlippageTicks = slippageTicks,
            TypicalAtr = typicalAtr,
            StopDistance = stopDistance,
            AccountEquity = accountEquity,
            LiquidityLevel = LiquidityLevel.Unknown,
            BookContinuityLevel = BookContinuityLevel.Unknown,
            RolloverClarity = RolloverClarity.Unknown,
            DataDate = record.DataDate,
            DataSource = record.DataSource,
            Reasons = string.Join("；", reasons),
        };

        return new ProductDataNormalizeResult
        {
            IsSuccess = true,
            Row = row,
        };
    }
}
