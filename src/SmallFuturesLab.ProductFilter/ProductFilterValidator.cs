namespace SmallFuturesLab.ProductFilter;

/// <summary>
/// 品种筛选数据校验器。
/// </summary>
public class ProductFilterValidator
{
    /// <summary>
    /// 校验单行数据。
    /// </summary>
    /// <param name="row">数据行。</param>
    /// <param name="rowNumber">行号。</param>
    /// <returns>校验结果。</returns>
    public ProductFilterValidationResult Validate(ProductFilterRow row, int rowNumber)
    {
        var errors = new List<ProductFilterValidationError>();

        if (double.IsNaN(row.Price) || double.IsInfinity(row.Price) || row.Price <= 0)
            errors.Add(new ProductFilterValidationError { RowNumber = rowNumber, FieldName = "Price", Reason = "Price 必须大于 0" });

        if (row.Multiplier <= 0)
            errors.Add(new ProductFilterValidationError { RowNumber = rowNumber, FieldName = "Multiplier", Reason = "Multiplier 必须大于 0" });

        if (row.TickSize <= 0)
            errors.Add(new ProductFilterValidationError { RowNumber = rowNumber, FieldName = "TickSize", Reason = "TickSize 必须大于 0" });

        if (row.MarginRate < 0)
            errors.Add(new ProductFilterValidationError { RowNumber = rowNumber, FieldName = "MarginRate", Reason = "MarginRate 不能为负数" });

        if (row.RoundTripFeePerLot < 0)
            errors.Add(new ProductFilterValidationError { RowNumber = rowNumber, FieldName = "RoundTripFeePerLot", Reason = "RoundTripFeePerLot 不能为负数" });

        if (row.SlippageTicks < 0)
            errors.Add(new ProductFilterValidationError { RowNumber = rowNumber, FieldName = "SlippageTicks", Reason = "SlippageTicks 不能为负数" });

        if (row.TypicalAtr < 0)
            errors.Add(new ProductFilterValidationError { RowNumber = rowNumber, FieldName = "TypicalAtr", Reason = "TypicalAtr 不能为负数" });

        if (row.StopDistance <= 0)
            errors.Add(new ProductFilterValidationError { RowNumber = rowNumber, FieldName = "StopDistance", Reason = "StopDistance 必须大于 0" });

        if (double.IsNaN(row.AccountEquity) || double.IsInfinity(row.AccountEquity) || row.AccountEquity <= 0)
            errors.Add(new ProductFilterValidationError { RowNumber = rowNumber, FieldName = "AccountEquity", Reason = "AccountEquity 必须大于 0" });

        if (string.IsNullOrWhiteSpace(row.DataDate))
            errors.Add(new ProductFilterValidationError { RowNumber = rowNumber, FieldName = "DataDate", Reason = "DataDate 不能为空" });

        if (string.IsNullOrWhiteSpace(row.DataSource))
            errors.Add(new ProductFilterValidationError { RowNumber = rowNumber, FieldName = "DataSource", Reason = "DataSource 不能为空" });

        if (!IsValidEnum(row.LiquidityLevel))
            errors.Add(new ProductFilterValidationError { RowNumber = rowNumber, FieldName = "LiquidityLevel", Reason = "LiquidityLevel 只能是 Good / Medium / Poor / Unknown" });

        if (!IsValidEnum(row.BookContinuityLevel))
            errors.Add(new ProductFilterValidationError { RowNumber = rowNumber, FieldName = "BookContinuityLevel", Reason = "BookContinuityLevel 只能是 Good / Medium / Poor / Unknown" });

        if (!IsValidEnum(row.RolloverClarity))
            errors.Add(new ProductFilterValidationError { RowNumber = rowNumber, FieldName = "RolloverClarity", Reason = "RolloverClarity 只能是 Good / Medium / Poor / Unknown" });

        return new ProductFilterValidationResult
        {
            IsValid = errors.Count == 0,
            Errors = errors.AsReadOnly(),
        };
    }

    private static bool IsValidEnum<T>(T value) where T : struct, Enum
    {
        return Enum.IsDefined(value);
    }
}
