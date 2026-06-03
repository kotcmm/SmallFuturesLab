using System.Text;

namespace SmallFuturesLab.ProductFilter;

/// <summary>
/// 品种筛选汇总 Markdown 输出器。
/// </summary>
public class ProductFilterSummaryWriter
{
    /// <summary>
    /// 生成汇总统计。
    /// </summary>
    /// <param name="results">计算结果列表。</param>
    /// <returns>汇总统计。</returns>
    public ProductFilterSummary GenerateSummary(IReadOnlyList<ProductFilterCalculationResult> results)
    {
        var uniqueProducts = results.Select(r => r.Row.ProductCode).Distinct().Count();

        var byEquity = results
            .GroupBy(r => r.Row.AccountEquity)
            .OrderBy(g => g.Key)
            .ToDictionary(
                g => g.Key,
                g => new AccountEquitySummary
                {
                    AllowedCount = g.Count(r => r.Result == ProductFilterResultStatus.Allowed),
                    CautionCount = g.Count(r => r.Result == ProductFilterResultStatus.Caution),
                    RejectedCount = g.Count(r => r.Result == ProductFilterResultStatus.Rejected),
                    Candidates = g
                        .Where(r => r.Result == ProductFilterResultStatus.Allowed)
                        .Select(r => $"{r.Row.ProductName} ({r.Row.ProductCode})")
                        .Distinct()
                        .ToList(),
                    CautionList = g
                        .Where(r => r.Result == ProductFilterResultStatus.Caution)
                        .Select(r => $"{r.Row.ProductName} ({r.Row.ProductCode})")
                        .Distinct()
                        .ToList(),
                    ExcludedList = g
                        .Where(r => r.Result == ProductFilterResultStatus.Rejected)
                        .Select(r => $"{r.Row.ProductName} ({r.Row.ProductCode})")
                        .Distinct()
                        .ToList(),
                });

        var needsReview = results
            .Where(r =>
                r.Row.LiquidityLevel == LiquidityLevel.Unknown
                || r.Row.BookContinuityLevel == BookContinuityLevel.Unknown
                || r.Row.RolloverClarity == RolloverClarity.Unknown
                || string.IsNullOrWhiteSpace(r.Row.DataSource)
                || r.Row.DataSource.Contains("第三方", StringComparison.Ordinal))
            .Select(r => $"{r.Row.ProductName} ({r.Row.ProductCode}) - AccountEquity={r.Row.AccountEquity} - {r.Row.DataSource}")
            .Distinct()
            .ToList();

        var rejectionReasons = new Dictionary<string, int>();
        foreach (var r in results.Where(r => r.Result == ProductFilterResultStatus.Rejected))
        {
            if (r.Row.RiskRate > 0.02)
                Increment(rejectionReasons, $"AccountEquity={r.Row.AccountEquity} 单笔风险超过 2%");
            if (r.Row.CostRatio > 0.30)
                Increment(rejectionReasons, "成本占比超过 0.3R");
            if (r.Row.MarginRateOfEquity > 0.50)
                Increment(rejectionReasons, $"AccountEquity={r.Row.AccountEquity} 保证金占用超过 50%");
        }

        return new ProductFilterSummary
        {
            TotalRecords = results.Count,
            UniqueProducts = uniqueProducts,
            ByAccountEquity = byEquity,
            NeedsReview = needsReview,
            RejectionReasonStats = rejectionReasons,
        };
    }

    /// <summary>
    /// 生成 Markdown 汇总文本。
    /// </summary>
    /// <param name="results">计算结果列表。</param>
    /// <returns>Markdown 文本。</returns>
    public string WriteMarkdown(IReadOnlyList<ProductFilterCalculationResult> results)
    {
        var summary = GenerateSummary(results);
        var sb = new StringBuilder();

        sb.AppendLine("# 品种筛选汇总");
        sb.AppendLine();
        sb.AppendLine($"> 总记录数：{summary.TotalRecords}");
        sb.AppendLine($"> 涉及品种数：{summary.UniqueProducts}");
        sb.AppendLine();

        sb.AppendLine("## 统计概览");
        sb.AppendLine();
        sb.AppendLine("| 账户规模 | Allowed | Caution | Rejected |");
        sb.AppendLine("|---|---:|---:|---:|");
        foreach (var kv in summary.ByAccountEquity.OrderBy(kv => kv.Key))
        {
            var equity = kv.Key;
            var s = kv.Value;
            sb.AppendLine($"| {equity:F0} 元 | {s.AllowedCount} | {s.CautionCount} | {s.RejectedCount} |");
        }
        sb.AppendLine();

        foreach (var kv in summary.ByAccountEquity.OrderBy(kv => kv.Key))
        {
            var equity = kv.Key;
            var s = kv.Value;

            sb.AppendLine($"## {equity:F0} 元账户");
            sb.AppendLine();

            sb.AppendLine($"### 进入后续周期研究列表");
            sb.AppendLine();
            if (s.Candidates.Count > 0)
            {
                foreach (var item in s.Candidates)
                {
                    sb.AppendLine($"- {item}");
                }
            }
            else
            {
                sb.AppendLine("_无_");
            }
            sb.AppendLine();

            sb.AppendLine($"### 谨慎观察列表");
            sb.AppendLine();
            if (s.CautionList.Count > 0)
            {
                foreach (var item in s.CautionList)
                {
                    sb.AppendLine($"- {item}");
                }
            }
            else
            {
                sb.AppendLine("_无_");
            }
            sb.AppendLine();

            sb.AppendLine($"### 当前账户规模排除列表");
            sb.AppendLine();
            if (s.ExcludedList.Count > 0)
            {
                foreach (var item in s.ExcludedList)
                {
                    sb.AppendLine($"- {item}");
                }
            }
            else
            {
                sb.AppendLine("_无_");
            }
            sb.AppendLine();
        }

        sb.AppendLine("## 需要复核的数据");
        sb.AppendLine();
        if (summary.NeedsReview.Count > 0)
        {
            foreach (var item in summary.NeedsReview)
            {
                sb.AppendLine($"- {item}");
            }
        }
        else
        {
            sb.AppendLine("_无_");
        }
        sb.AppendLine();

        sb.AppendLine("## 主要排除原因统计");
        sb.AppendLine();
        if (summary.RejectionReasonStats.Count > 0)
        {
            foreach (var kv in summary.RejectionReasonStats.OrderByDescending(kv => kv.Value))
            {
                sb.AppendLine($"- {kv.Key}：{kv.Value} 次");
            }
        }
        else
        {
            sb.AppendLine("_无_");
        }
        sb.AppendLine();

        sb.AppendLine("---");
        sb.AppendLine();
        sb.AppendLine("**注意**：本汇总仅用于品种筛选研究，不代表任何交易建议。所有结论基于输入数据和风险测算公式，不涉及行情判断或策略信号。");

        return sb.ToString();
    }

    private static void Increment(Dictionary<string, int> dict, string key)
    {
        if (!dict.TryAdd(key, 1))
        {
            dict[key]++;
        }
    }
}
