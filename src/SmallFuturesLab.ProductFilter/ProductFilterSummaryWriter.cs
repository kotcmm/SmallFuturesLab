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
        var allowed10k = results.Where(r => r.Result10k == ProductFilterResultStatus.Allowed).ToList();
        var caution10k = results.Where(r => r.Result10k == ProductFilterResultStatus.Caution).ToList();
        var rejected10k = results.Where(r => r.Result10k == ProductFilterResultStatus.Rejected).ToList();

        var allowed20k = results.Where(r => r.Result20k == ProductFilterResultStatus.Allowed).ToList();
        var caution20k = results.Where(r => r.Result20k == ProductFilterResultStatus.Caution).ToList();
        var rejected20k = results.Where(r => r.Result20k == ProductFilterResultStatus.Rejected).ToList();

        var uniqueProducts = results.Select(r => r.Row.ProductCode).Distinct().Count();

        var cautionList = results
            .Where(r => r.Result10k == ProductFilterResultStatus.Caution || r.Result20k == ProductFilterResultStatus.Caution)
            .Select(r => $"{r.Row.ProductName} ({r.Row.ProductCode})")
            .Distinct()
            .ToList();

        var excludedList = results
            .Where(r => r.Result10k == ProductFilterResultStatus.Rejected || r.Result20k == ProductFilterResultStatus.Rejected)
            .Select(r => $"{r.Row.ProductName} ({r.Row.ProductCode})")
            .Distinct()
            .ToList();

        var needsReview = results
            .Where(r =>
                r.Row.LiquidityLevel == LiquidityLevel.Unknown
                || r.Row.BookContinuityLevel == BookContinuityLevel.Unknown
                || r.Row.RolloverClarity == RolloverClarity.Unknown
                || string.IsNullOrWhiteSpace(r.Row.DataSource)
                || r.Row.DataSource.Contains("第三方", StringComparison.Ordinal))
            .Select(r => $"{r.Row.ProductName} ({r.Row.ProductCode}) - {r.Row.DataSource}")
            .Distinct()
            .ToList();

        var rejectionReasons = new Dictionary<string, int>();
        foreach (var r in results.Where(r => r.Result10k == ProductFilterResultStatus.Rejected || r.Result20k == ProductFilterResultStatus.Rejected))
        {
            if (r.Row.RiskRate10k > 0.02)
                Increment(rejectionReasons, "10k 账户单笔风险超过 2%");
            if (r.Row.RiskRate20k > 0.02)
                Increment(rejectionReasons, "20k 账户单笔风险超过 2%");
            if (r.Row.CostRatio > 0.30)
                Increment(rejectionReasons, "成本占比超过 0.3R");
            if (r.Row.MarginRate10k > 0.50)
                Increment(rejectionReasons, "10k 账户保证金占用超过 50%");
            if (r.Row.MarginRate20k > 0.50)
                Increment(rejectionReasons, "20k 账户保证金占用超过 50%");
        }

        return new ProductFilterSummary
        {
            TotalRecords = results.Count,
            UniqueProducts = uniqueProducts,
            AllowedCount10k = allowed10k.Count,
            CautionCount10k = caution10k.Count,
            RejectedCount10k = rejected10k.Count,
            AllowedCount20k = allowed20k.Count,
            CautionCount20k = caution20k.Count,
            RejectedCount20k = rejected20k.Count,
            Candidates10k = allowed10k.Select(r => $"{r.Row.ProductName} ({r.Row.ProductCode})").Distinct().ToList(),
            Candidates20k = allowed20k.Select(r => $"{r.Row.ProductName} ({r.Row.ProductCode})").Distinct().ToList(),
            CautionList = cautionList,
            ExcludedList = excludedList,
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
        sb.AppendLine($"> 生成时间：{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
        sb.AppendLine($"> 总记录数：{summary.TotalRecords}");
        sb.AppendLine($"> 涉及品种数：{summary.UniqueProducts}");
        sb.AppendLine();

        sb.AppendLine("## 统计概览");
        sb.AppendLine();
        sb.AppendLine("| 账户规模 | Allowed | Caution | Rejected |");
        sb.AppendLine("|---|---:|---:|---:|");
        sb.AppendLine($"| 10,000 元 | {summary.AllowedCount10k} | {summary.CautionCount10k} | {summary.RejectedCount10k} |");
        sb.AppendLine($"| 20,000 元 | {summary.AllowedCount20k} | {summary.CautionCount20k} | {summary.RejectedCount20k} |");
        sb.AppendLine();

        sb.AppendLine("## 10,000 元账户候选列表");
        sb.AppendLine();
        if (summary.Candidates10k.Count > 0)
        {
            foreach (var item in summary.Candidates10k)
            {
                sb.AppendLine($"- {item}");
            }
        }
        else
        {
            sb.AppendLine("_无_");
        }
        sb.AppendLine();

        sb.AppendLine("## 20,000 元账户候选列表");
        sb.AppendLine();
        if (summary.Candidates20k.Count > 0)
        {
            foreach (var item in summary.Candidates20k)
            {
                sb.AppendLine($"- {item}");
            }
        }
        else
        {
            sb.AppendLine("_无_");
        }
        sb.AppendLine();

        sb.AppendLine("## 谨慎观察列表");
        sb.AppendLine();
        if (summary.CautionList.Count > 0)
        {
            foreach (var item in summary.CautionList)
            {
                sb.AppendLine($"- {item}");
            }
        }
        else
        {
            sb.AppendLine("_无_");
        }
        sb.AppendLine();

        sb.AppendLine("## 排除列表");
        sb.AppendLine();
        if (summary.ExcludedList.Count > 0)
        {
            foreach (var item in summary.ExcludedList)
            {
                sb.AppendLine($"- {item}");
            }
        }
        else
        {
            sb.AppendLine("_无_");
        }
        sb.AppendLine();

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
