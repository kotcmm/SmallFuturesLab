namespace SmallFuturesLab.Core;

/// <summary>
/// 小资金期货品种过滤器。
/// 对一个品种执行过滤，返回 Allowed / Caution / Rejected。
/// 不读取文件、不知道交易星球、不生成交易建议、不输出买卖方向。
/// </summary>
public sealed class ProductFilter
{
    /// <summary>
    /// 对单个品种执行过滤，返回过滤结果。
    /// </summary>
    /// <param name="product">待过滤品种。</param>
    /// <param name="config">账户风险配置。</param>
    /// <param name="condition">测算条件。</param>
    /// <returns>过滤结果。</returns>
    public ProductFilterResult Evaluate(Product product, RiskConfig config, FilterCondition condition)
    {
        ArgumentNullException.ThrowIfNull(product);
        ArgumentNullException.ThrowIfNull(config);
        ArgumentNullException.ThrowIfNull(condition);

        var reasons = new List<string>();
        Validate(product, config, condition, reasons);

        if (reasons.Count > 0)
        {
            return new ProductFilterResult
            {
                Code = product.Code,
                Contract = product.Contract,
                RiskRate = 0,
                MarginRate = 0,
                CostRatio = 0,
                TotalRiskMoney = 0,
                MarginMoney = 0,
                Status = ProductFilterStatus.Rejected,
                Reasons = reasons,
            };
        }

        var risk = new ProductRisk
        {
            Product = product,
            RiskConfig = config,
            Condition = condition,
        };

        var status = risk.Status;
        AppendReasons(reasons, status, risk, config);

        return new ProductFilterResult
        {
            Code = product.Code,
            Contract = product.Contract,
            RiskRate = risk.RiskRate,
            MarginRate = risk.MarginRate,
            CostRatio = risk.CostRatio,
            TotalRiskMoney = risk.TotalRiskMoney,
            MarginMoney = risk.MarginMoney,
            Status = status,
            Reasons = reasons,
        };
    }

    private static void Validate(Product product, RiskConfig config, FilterCondition condition, List<string> reasons)
    {
        if (config.AccountEquity <= 0 || !double.IsFinite(config.AccountEquity))
        {
            reasons.Add("账户权益必须是有限正数");
        }

        if (config.RejectRiskRate <= 0 || !double.IsFinite(config.RejectRiskRate))
        {
            reasons.Add("拒绝风险率阈值必须是有限正数");
        }

        if (config.RejectMarginRate <= 0 || !double.IsFinite(config.RejectMarginRate))
        {
            reasons.Add("拒绝保证金率阈值必须是有限正数");
        }

        if (config.RejectCostRatio <= 0 || !double.IsFinite(config.RejectCostRatio))
        {
            reasons.Add("拒绝成本比例阈值必须是有限正数");
        }

        if (string.IsNullOrWhiteSpace(product.Code))
        {
            reasons.Add("品种代码不能为空");
        }

        if (string.IsNullOrWhiteSpace(product.Contract))
        {
            reasons.Add("合约代码不能为空");
        }

        if (product.Price <= 0 || !double.IsFinite(product.Price))
        {
            reasons.Add("价格必须是有限正数");
        }

        if (product.Multiplier <= 0 || !double.IsFinite(product.Multiplier))
        {
            reasons.Add("合约乘数必须是有限正数");
        }

        if (product.TickSize <= 0 || !double.IsFinite(product.TickSize))
        {
            reasons.Add("最小变动价位必须是有限正数");
        }

        if (product.MarginRate <= 0 || !double.IsFinite(product.MarginRate))
        {
            reasons.Add("保证金比例必须是有限正数");
        }

        if (product.RoundTripFee < 0 || !double.IsFinite(product.RoundTripFee))
        {
            reasons.Add("单手开平总手续费不能为负数");
        }

        if (condition.StopTicks <= 0)
        {
            reasons.Add("止损 tick 数必须大于 0");
        }

        if (condition.SlippageTicks < 0)
        {
            reasons.Add("滑点 tick 数不能为负数");
        }

        if (condition.Lots <= 0)
        {
            reasons.Add("测算手数必须大于 0");
        }
    }

    private static void AppendReasons(List<string> reasons, ProductFilterStatus status, ProductRisk risk, RiskConfig config)
    {
        if (risk.RiskRate > config.RejectRiskRate)
        {
            reasons.Add($"单笔风险 {risk.RiskRate:P2} 超过拒绝阈值 {config.RejectRiskRate:P2}");
        }
        else if (risk.RiskRate > config.CautionRiskRate)
        {
            reasons.Add($"单笔风险 {risk.RiskRate:P2} 进入谨慎区");
        }

        if (risk.MarginRate > config.RejectMarginRate)
        {
            reasons.Add($"保证金占用 {risk.MarginRate:P2} 超过拒绝阈值 {config.RejectMarginRate:P2}");
        }
        else if (risk.MarginRate > config.CautionMarginRate)
        {
            reasons.Add($"保证金占用 {risk.MarginRate:P2} 进入谨慎区");
        }

        if (risk.CostRatio > config.RejectCostRatio)
        {
            reasons.Add($"成本占止损风险 {risk.CostRatio:P2} 超过拒绝阈值 {config.RejectCostRatio:P2}");
        }
        else if (risk.CostRatio > config.CautionCostRatio)
        {
            reasons.Add($"成本占止损风险 {risk.CostRatio:P2} 进入谨慎区");
        }

        if (status == ProductFilterStatus.Allowed)
        {
            reasons.Add("当前账户规模下，一手合约压力可接受");
        }
    }
}
