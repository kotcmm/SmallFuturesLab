using SmallFuturesLab.ProductData.Models;
using SmallFuturesLab.ProductData.Merging;
using SmallFuturesLab.ProductData.Scenarios;
using SmallFuturesLab.ProductFilter;

namespace SmallFuturesLab.ProductData.Pipeline;

/// <summary>
/// 本地组合管线，负责把多个本地数据源读取、合并、展开成品种筛选行。
/// 不调用 ProductFilterCalculator，不判断 Allowed / Caution / Rejected，不生成交易建议。
/// </summary>
public class ProductDataLocalCompositionPipeline
{
    private readonly ProductDataRecordMerger _merger = new();
    private readonly ProductFilterScenarioExpander _expander = new();

    /// <summary>
    /// 执行本地组合管线。
    /// </summary>
    /// <param name="inputs">本地数据源输入集合。</param>
    /// <param name="scenarioSet">测算场景集。</param>
    /// <returns>管线输出结果。</returns>
    /// <exception cref="ArgumentNullException">当 inputs、scenarioSet 或其中任意 input 为 null 时抛出。</exception>
    public ProductDataPipelineResult Run(IEnumerable<ProductDataSourceInput> inputs, ProductFilterScenarioSet scenarioSet)
    {
        ArgumentNullException.ThrowIfNull(inputs);
        ArgumentNullException.ThrowIfNull(scenarioSet);

        var allRecords = new List<ProductDataRecord>();
        var errors = new List<ProductDataPipelineError>();

        // 1. 逐个读取数据源
        foreach (var input in inputs)
        {
            if (input is null)
            {
                throw new ArgumentNullException(nameof(input), "input 不能为 null");
            }

            var readResult = input.Source.Read(input.FilePath);

            allRecords.AddRange(readResult.Records);

            foreach (var readError in readResult.Errors)
            {
                errors.Add(new ProductDataPipelineError
                {
                    Stage = "Read",
                    SourceName = input.Name,
                    ProductCode = string.Empty,
                    ContractCode = string.Empty,
                    FieldName = readError.FieldName,
                    Reason = readError.Reason,
                });
            }
        }

        // 2. 合并记录
        var mergeResult = _merger.Merge(allRecords);

        foreach (var mergeError in mergeResult.Errors)
        {
            errors.Add(new ProductDataPipelineError
            {
                Stage = "Merge",
                SourceName = string.Empty,
                ProductCode = mergeError.ProductCode,
                ContractCode = mergeError.ContractCode,
                FieldName = mergeError.FieldName,
                Reason = mergeError.Reason,
            });
        }

        // 3. 展开测算场景
        var rows = new List<ProductFilterRow>();

        foreach (var record in mergeResult.Records)
        {
            var expandResult = _expander.Expand(record, scenarioSet);

            rows.AddRange(expandResult.Rows);

            foreach (var expandError in expandResult.Errors)
            {
                errors.Add(new ProductDataPipelineError
                {
                    Stage = "Expand",
                    SourceName = string.Empty,
                    ProductCode = expandError.ProductCode,
                    ContractCode = expandError.ContractCode,
                    FieldName = expandError.ScenarioName,
                    Reason = expandError.Reason,
                });
            }
        }

        return new ProductDataPipelineResult
        {
            MergedRecords = mergeResult.Records,
            Rows = rows,
            Errors = errors,
        };
    }
}
