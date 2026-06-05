using SmallFuturesLab.ProductFilter;

namespace SmallFuturesLab.ProductData;

/// <summary>
/// 品种数据记录合并器，负责把多个来源的 ProductDataRecord 按 ProductCode + ContractCode 合并成完整记录。
/// 不计算公式字段，不判断 Allowed / Caution / Rejected，不生成交易建议，不执行 CTP 优先规则。
/// </summary>
public class ProductDataRecordMerger
{
    /// <summary>
    /// 将多条品种数据记录按 ProductCode + ContractCode 合并。
    /// 同一 key 的多条记录合并成一条；字段冲突时该 key 进入 Errors。
    /// </summary>
    /// <param name="records">品种数据记录集合。</param>
    /// <returns>合并结果。</returns>
    /// <exception cref="ArgumentNullException">当 records 为 null 时抛出。</exception>
    public ProductDataMergeResult Merge(IEnumerable<ProductDataRecord> records)
    {
        ArgumentNullException.ThrowIfNull(records);

        var mergedRecords = new List<ProductDataRecord>();
        var errors = new List<ProductDataMergeError>();

        var groups = records.GroupBy(r => (r.ProductCode, r.ContractCode));

        foreach (var group in groups)
        {
            var productCode = group.Key.ProductCode;
            var contractCode = group.Key.ContractCode;

            if (string.IsNullOrWhiteSpace(productCode) || string.IsNullOrWhiteSpace(contractCode))
            {
                errors.Add(new ProductDataMergeError
                {
                    ProductCode = productCode,
                    ContractCode = contractCode,
                    FieldName = "Key",
                    Reason = "ProductCode 或 ContractCode 不能为空",
                });
                continue;
            }

            var groupRecords = group.ToList();
            var conflict = TryMergeGroup(groupRecords, out var mergedRecord);

            if (conflict != null)
            {
                errors.Add(new ProductDataMergeError
                {
                    ProductCode = productCode,
                    ContractCode = contractCode,
                    FieldName = conflict.Value.FieldName,
                    Reason = conflict.Value.Reason,
                });
            }
            else
            {
                mergedRecords.Add(mergedRecord!);
            }
        }

        return new ProductDataMergeResult
        {
            Records = mergedRecords,
            Errors = errors,
        };
    }

    private static (string FieldName, string Reason)? TryMergeGroup(List<ProductDataRecord> records, out ProductDataRecord? merged)
    {
        merged = null;
        var conflictMessages = new List<string>();
        string? firstConflictField = null;

        // 字符串字段合并
        var exchange = MergeString(records, r => r.Exchange, "Exchange", conflictMessages, ref firstConflictField);
        var productName = MergeString(records, r => r.ProductName, "ProductName", conflictMessages, ref firstConflictField);
        var dataDate = MergeString(records, r => r.DataDate, "DataDate", conflictMessages, ref firstConflictField);

        // 可空 double 字段合并
        var multiplier = MergeNullableDouble(records, r => r.Multiplier, "Multiplier", conflictMessages, ref firstConflictField);
        var tickSize = MergeNullableDouble(records, r => r.TickSize, "TickSize", conflictMessages, ref firstConflictField);

        // 正 double 字段合并
        var price = MergePositiveDouble(records, r => r.Price, "Price", conflictMessages, ref firstConflictField);
        var marginRate = MergePositiveDouble(records, r => r.MarginRate, "MarginRate", conflictMessages, ref firstConflictField);
        var marginPerLot = MergePositiveDouble(records, r => r.MarginPerLot, "MarginPerLot", conflictMessages, ref firstConflictField);
        var roundTripFeePerLot = MergePositiveDouble(records, r => r.RoundTripFeePerLot, "RoundTripFeePerLot", conflictMessages, ref firstConflictField);
        var openFeePerLot = MergePositiveDouble(records, r => r.OpenFeePerLot, "OpenFeePerLot", conflictMessages, ref firstConflictField);
        var closeYesterdayFeePerLot = MergePositiveDouble(records, r => r.CloseYesterdayFeePerLot, "CloseYesterdayFeePerLot", conflictMessages, ref firstConflictField);
        var closeTodayFeePerLot = MergePositiveDouble(records, r => r.CloseTodayFeePerLot, "CloseTodayFeePerLot", conflictMessages, ref firstConflictField);
        var typicalAtr = MergePositiveDouble(records, r => r.TypicalAtr, "TypicalAtr", conflictMessages, ref firstConflictField);

        // 非负 double 字段合并（成交量、持仓量）
        var volume = MergePositiveDouble(records, r => r.Volume, "Volume", conflictMessages, ref firstConflictField);
        var openInterest = MergePositiveDouble(records, r => r.OpenInterest, "OpenInterest", conflictMessages, ref firstConflictField);

        // 枚举字段合并
        var liquidityLevel = MergeEnum(records, r => r.LiquidityLevel, LiquidityLevel.Unknown, "LiquidityLevel", conflictMessages, ref firstConflictField);
        var bookContinuityLevel = MergeEnum(records, r => r.BookContinuityLevel, BookContinuityLevel.Unknown, "BookContinuityLevel", conflictMessages, ref firstConflictField);
        var rolloverClarity = MergeEnum(records, r => r.RolloverClarity, RolloverClarity.Unknown, "RolloverClarity", conflictMessages, ref firstConflictField);

        // DataSource 特殊合并：去重后连接
        var dataSources = records.Select(r => r.DataSource).Where(IsValidString).Distinct().ToList();
        var mergedDataSource = string.Join("；", dataSources);

        // DataSourceType 合并：一致则保留，不一致保留第一条
        var sourceTypes = records.Select(r => r.DataSourceType).ToList();
        var mergedSourceType = sourceTypes.First();

        // Bool 字段 OR 合并
        var needsReview = records.Any(r => r.NeedsReview);
        var isMainContract = records.Any(r => r.IsMainContract);

        if (conflictMessages.Count > 0)
        {
            return (firstConflictField ?? "Multiple", string.Join("；", conflictMessages));
        }

        merged = new ProductDataRecord
        {
            Exchange = exchange,
            ProductName = productName,
            ProductCode = records[0].ProductCode,
            ContractCode = records[0].ContractCode,
            Price = price,
            Multiplier = multiplier,
            TickSize = tickSize,
            MarginRate = marginRate,
            MarginPerLot = marginPerLot,
            RoundTripFeePerLot = roundTripFeePerLot,
            OpenFeePerLot = openFeePerLot,
            CloseYesterdayFeePerLot = closeYesterdayFeePerLot,
            CloseTodayFeePerLot = closeTodayFeePerLot,
            Volume = volume,
            OpenInterest = openInterest,
            TypicalAtr = typicalAtr,
            LiquidityLevel = liquidityLevel,
            BookContinuityLevel = bookContinuityLevel,
            RolloverClarity = rolloverClarity,
            IsMainContract = isMainContract,
            DataDate = dataDate,
            DataSource = mergedDataSource,
            DataSourceType = mergedSourceType,
            NeedsReview = needsReview,
        };

        return null;
    }

    private static string MergeString(
        List<ProductDataRecord> records,
        Func<ProductDataRecord, string> selector,
        string fieldName,
        List<string> conflictMessages,
        ref string? firstConflictField)
    {
        var validValues = records.Select(selector).Where(IsValidString).Distinct().ToList();

        if (validValues.Count > 1)
        {
            conflictMessages.Add($"{fieldName} 冲突: {string.Join(" vs ", validValues)}");
            firstConflictField ??= fieldName;
        }

        return validValues.Count >= 1 ? validValues[0] : string.Empty;
    }

    private static double? MergeNullableDouble(
        List<ProductDataRecord> records,
        Func<ProductDataRecord, double?> selector,
        string fieldName,
        List<string> conflictMessages,
        ref string? firstConflictField)
    {
        var validValues = records.Select(selector).Where(v => v.HasValue && IsPositiveFinite(v.Value)).Distinct().ToList();

        if (validValues.Count > 1)
        {
            conflictMessages.Add($"{fieldName} 冲突: {string.Join(" vs ", validValues)}");
            firstConflictField ??= fieldName;
        }

        return validValues.Count >= 1 ? validValues[0] : null;
    }

    private static double MergePositiveDouble(
        List<ProductDataRecord> records,
        Func<ProductDataRecord, double> selector,
        string fieldName,
        List<string> conflictMessages,
        ref string? firstConflictField)
    {
        var validValues = records.Select(selector).Where(IsPositiveFinite).Distinct().ToList();

        if (validValues.Count > 1)
        {
            conflictMessages.Add($"{fieldName} 冲突: {string.Join(" vs ", validValues)}");
            firstConflictField ??= fieldName;
        }

        return validValues.Count >= 1 ? validValues[0] : 0;
    }

    private static T MergeEnum<T>(
        List<ProductDataRecord> records,
        Func<ProductDataRecord, T> selector,
        T invalidValue,
        string fieldName,
        List<string> conflictMessages,
        ref string? firstConflictField) where T : struct, Enum
    {
        var validValues = records.Select(selector).Where(v => !EqualityComparer<T>.Default.Equals(v, invalidValue)).Distinct().ToList();

        if (validValues.Count > 1)
        {
            conflictMessages.Add($"{fieldName} 冲突: {string.Join(" vs ", validValues)}");
            firstConflictField ??= fieldName;
        }

        return validValues.Count >= 1 ? validValues[0] : invalidValue;
    }

    private static bool IsValidString(string value) => !string.IsNullOrWhiteSpace(value);

    private static bool IsPositiveFinite(double value) => !double.IsNaN(value) && !double.IsInfinity(value) && value > 0;
}
