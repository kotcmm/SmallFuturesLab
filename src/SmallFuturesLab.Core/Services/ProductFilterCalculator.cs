namespace SmallFuturesLab.Core.Services;

using SmallFuturesLab.Core.Models;

/// <summary>
/// 小资金品种过滤计算器。
/// 只判断一手合约对账户的保证金、止损、手续费和滑点压力是否可承受。
/// </summary>
public sealed class ProductFilterCalculator
{
    /// <summary>
    /// 计算单个合约在当前账户设置下的过滤结果。
    /// </summary>
    /// <param name="product">品种信息。</param>
    /// <param name="setting">账户风险设置。</param>
    /// <returns>过滤结果。</returns>
    public ProductFilterResult Calculate(ProductInfo product, AccountRiskSetting setting)
    {
        ArgumentNullException.ThrowIfNull(product);
        ArgumentNullException.ThrowIfNull(setting);

        var reasons = new List<string>();
        ValidateInputs(product, setting, reasons);

        var tickValue = ResolveTickValue(product);
        var marginPerLot = ResolveMarginPerLot(product);
        var stopRiskMoney = setting.StopTicks * tickValue * setting.Lots;
        var slippageMoney = setting.SlippageTicks * tickValue * setting.Lots;
        var costMoney = product.RoundTripFeePerLot * setting.Lots + slippageMoney;
        var totalRiskMoney = stopRiskMoney + costMoney;
        var riskRate = setting.AccountEquity > 0 ? totalRiskMoney / setting.AccountEquity : double.PositiveInfinity;
        var marginRateOfEquity = setting.AccountEquity > 0 ? marginPerLot / setting.AccountEquity : double.PositiveInfinity;
        var costRatio = stopRiskMoney > 0 ? costMoney / stopRiskMoney : double.PositiveInfinity;

        ProductFilterStatus status;
        if (reasons.Count > 0
            || riskRate > setting.ExtremeRiskRate
            || marginRateOfEquity > setting.ExtremeMarginRateOfEquity
            || costRatio > setting.ExtremeCostRatio)
        {
            status = ProductFilterStatus.Rejected;
        }
        else if (riskRate > setting.NormalRiskRate
                 || marginRateOfEquity > setting.PreferredMarginRateOfEquity
                 || costRatio > setting.PreferredCostRatio)
        {
            status = ProductFilterStatus.Caution;
        }
        else
        {
            status = ProductFilterStatus.Allowed;
        }

        AppendThresholdReasons(reasons, status, riskRate, marginRateOfEquity, costRatio, setting);

        return new ProductFilterResult
        {
            Exchange = product.Exchange,
            ProductName = product.ProductName,
            ProductCode = product.ProductCode,
            ContractCode = product.ContractCode,
            AccountEquity = setting.AccountEquity,
            TickValue = tickValue,
            MarginPerLot = marginPerLot,
            MarginRateOfEquity = marginRateOfEquity,
            StopRiskMoney = stopRiskMoney,
            SlippageMoney = slippageMoney,
            CostMoney = costMoney,
            TotalRiskMoney = totalRiskMoney,
            RiskRate = riskRate,
            CostRatio = costRatio,
            Status = status,
            Reasons = string.Join("；", reasons),
        };
    }

    private static void ValidateInputs(ProductInfo product, AccountRiskSetting setting, List<string> reasons)
    {
        if (string.IsNullOrWhiteSpace(product.ProductCode))
        {
            reasons.Add("品种代码为空");
        }

        if (string.IsNullOrWhiteSpace(product.ContractCode))
        {
            reasons.Add("合约代码为空");
        }

        if (setting.AccountEquity <= 0 || !double.IsFinite(setting.AccountEquity))
        {
            reasons.Add("账户权益必须是有限正数");
        }

        if (setting.Lots != 1)
        {
            reasons.Add("当前阶段只测算 1 手");
        }

        if (setting.StopTicks <= 0)
        {
            reasons.Add("止损 tick 必须大于 0");
        }

        if (setting.SlippageTicks < 0)
        {
            reasons.Add("滑点 tick 不能为负数");
        }

        if (ResolveTickValue(product) <= 0)
        {
            reasons.Add("缺少有效一跳金额，无法测算止损和滑点");
        }

        if (ResolveMarginPerLot(product) <= 0)
        {
            reasons.Add("缺少有效一手保证金，无法测算保证金占比");
        }

        if (product.RoundTripFeePerLot < 0 || !double.IsFinite(product.RoundTripFeePerLot))
        {
            reasons.Add("单手开平总手续费不能为负数或非有限数字");
        }
    }

    private static double ResolveTickValue(ProductInfo product)
    {
        if (product.TickSize > 0 && product.Multiplier > 0)
        {
            return product.TickSize * product.Multiplier;
        }

        return product.TickValue > 0 && double.IsFinite(product.TickValue) ? product.TickValue : 0;
    }

    private static double ResolveMarginPerLot(ProductInfo product)
    {
        if (product.MarginPerLot > 0 && double.IsFinite(product.MarginPerLot))
        {
            return product.MarginPerLot;
        }

        if (product.Price > 0 && product.Multiplier > 0 && product.MarginRate > 0)
        {
            return product.Price * product.Multiplier * product.MarginRate;
        }

        return 0;
    }

    private static void AppendThresholdReasons(
        List<string> reasons,
        ProductFilterStatus status,
        double riskRate,
        double marginRateOfEquity,
        double costRatio,
        AccountRiskSetting setting)
    {
        if (riskRate > setting.ExtremeRiskRate)
        {
            reasons.Add($"单笔风险 {riskRate:P2} 超过极限上限 {setting.ExtremeRiskRate:P2}");
        }
        else if (riskRate > setting.NormalRiskRate)
        {
            reasons.Add($"单笔风险 {riskRate:P2} 超过常规上限 {setting.NormalRiskRate:P2}");
        }
        else
        {
            reasons.Add($"单笔风险 {riskRate:P2} 未超过常规上限 {setting.NormalRiskRate:P2}");
        }

        if (marginRateOfEquity > setting.ExtremeMarginRateOfEquity)
        {
            reasons.Add($"保证金占用 {marginRateOfEquity:P2} 超过极限上限 {setting.ExtremeMarginRateOfEquity:P2}");
        }
        else if (marginRateOfEquity > setting.PreferredMarginRateOfEquity)
        {
            reasons.Add($"保证金占用 {marginRateOfEquity:P2} 超过推荐上限 {setting.PreferredMarginRateOfEquity:P2}");
        }
        else
        {
            reasons.Add($"保证金占用 {marginRateOfEquity:P2} 未超过推荐上限 {setting.PreferredMarginRateOfEquity:P2}");
        }

        if (costRatio > setting.ExtremeCostRatio)
        {
            reasons.Add($"成本占比 {costRatio:P2} 超过极限上限 {setting.ExtremeCostRatio:P2}");
        }
        else if (costRatio > setting.PreferredCostRatio)
        {
            reasons.Add($"成本占比 {costRatio:P2} 超过推荐上限 {setting.PreferredCostRatio:P2}");
        }
        else
        {
            reasons.Add($"成本占比 {costRatio:P2} 未超过推荐上限 {setting.PreferredCostRatio:P2}");
        }

        reasons.Add(status switch
        {
            ProductFilterStatus.Allowed => "可以进入后续研究，不是交易建议",
            ProductFilterStatus.Caution => "谨慎观察，不是交易建议",
            ProductFilterStatus.Rejected => "当前账户规模下排除，不是交易建议",
            _ => "不是交易建议",
        });
    }
}
