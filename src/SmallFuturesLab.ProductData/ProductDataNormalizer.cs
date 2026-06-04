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
        if (string.IsNullOrWhiteSpace(record.ProductCode))
            return Fail("ProductCode 不能为空");

        if (string.IsNullOrWhiteSpace(record.ContractCode))
            return Fail("ContractCode 不能为空");

        if (!IsValidPositiveFinite(record.Price))
            return Fail("Price 必须是有限数字且大于 0");

        if (!record.Multiplier.HasValue || !IsValidPositiveFinite(record.Multiplier.Value))
            return Fail("Multiplier 必须是有限数字且大于 0");

        if (!record.TickSize.HasValue || !IsValidPositiveFinite(record.TickSize.Value))
            return Fail("TickSize 必须是有限数字且大于 0");

        if (!IsValidPositiveFinite(record.MarginRate))
            return Fail("MarginRate 必须是有限数字且大于 0");

        if (!IsValidPositiveFinite(record.RoundTripFeePerLot))
            return Fail("RoundTripFeePerLot 必须是有限数字且大于 0");

        if (!IsValidPositiveFinite(accountEquity))
            return Fail("AccountEquity 必须是有限数字且大于 0");

        if (!IsValidPositiveFinite(stopDistance))
            return Fail("StopDistance 必须是有限数字且大于 0");

        if (slippageTicks < 0)
            return Fail("SlippageTicks 不能为负数");

        if (!IsValidNonNegativeFinite(typicalAtr))
            return Fail("TypicalAtr 必须是有限数字且不能为负数");

        if (string.IsNullOrWhiteSpace(record.DataDate))
            return Fail("DataDate 不能为空");

        if (string.IsNullOrWhiteSpace(record.DataSource))
            return Fail("DataSource 不能为空");

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

    private static ProductDataNormalizeResult Fail(string error)
    {
        return new ProductDataNormalizeResult
        {
            IsSuccess = false,
            Error = error,
        };
    }

    private static bool IsValidPositiveFinite(double value)
    {
        return !double.IsNaN(value) && !double.IsInfinity(value) && value > 0;
    }

    private static bool IsValidNonNegativeFinite(double value)
    {
        return !double.IsNaN(value) && !double.IsInfinity(value) && value >= 0;
    }
}
