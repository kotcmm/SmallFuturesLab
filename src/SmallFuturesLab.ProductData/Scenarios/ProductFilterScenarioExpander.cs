using SmallFuturesLab.ProductData.Models;
using SmallFuturesLab.ProductData.Normalization;
using SmallFuturesLab.ProductFilter;

namespace SmallFuturesLab.ProductData.Scenarios;

/// <summary>
/// 品种测算场景展开器，负责把一条完整的 ProductDataRecord 按场景展开成多条 ProductFilterRow。
/// 不计算公式字段，不判断 Allowed / Caution / Rejected，不生成交易建议。
/// </summary>
public class ProductFilterScenarioExpander
{
    private readonly ProductDataNormalizer _normalizer = new();

    /// <summary>
    /// 将品种数据记录按测算场景集展开为多条品种筛选行。
    /// 单个场景失败不影响其他场景继续展开。
    /// </summary>
    /// <param name="record">品种数据记录。</param>
    /// <param name="scenarioSet">测算场景集。</param>
    /// <returns>展开结果，包含成功行和失败错误。</returns>
    /// <exception cref="ArgumentNullException">当 record 或 scenarioSet 为 null 时抛出。</exception>
    public ProductFilterScenarioExpandResult Expand(ProductDataRecord record, ProductFilterScenarioSet scenarioSet)
    {
        ArgumentNullException.ThrowIfNull(record);
        ArgumentNullException.ThrowIfNull(scenarioSet);

        var rows = new List<ProductFilterRow>();
        var errors = new List<ProductFilterScenarioExpandError>();

        foreach (var scenario in scenarioSet.Scenarios)
        {
            var normalizeResult = _normalizer.Normalize(
                record,
                scenario.AccountEquity,
                scenario.StopDistance,
                scenario.SlippageTicks,
                record.TypicalAtr);

            if (normalizeResult.IsSuccess)
            {
                rows.Add(normalizeResult.Row);
            }
            else
            {
                errors.Add(new ProductFilterScenarioExpandError
                {
                    ProductCode = record.ProductCode,
                    ContractCode = record.ContractCode,
                    ScenarioName = scenario.Name,
                    Reason = normalizeResult.Error,
                });
            }
        }

        return new ProductFilterScenarioExpandResult
        {
            Rows = rows,
            Errors = errors,
        };
    }
}
