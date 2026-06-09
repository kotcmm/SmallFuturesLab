using SmallFuturesLab.Core.Accounts;
using SmallFuturesLab.Core.Products;
using SmallFuturesLab.Core.Risk;

namespace SmallFuturesLab.Core.Filtering;

/// <summary>
/// 小资金品种过滤计算器。
/// </summary>
public sealed class ProductFilterCalculator
{
    /// <summary>
    /// 计算单个品种在指定账户和测算场景下的过滤决定。
    /// </summary>
    public ProductFilterDecision Calculate(
        ProductInfo product,
        AccountProfile account,
        FilterScenario scenario,
        RiskPolicy policy)
    {
        ArgumentNullException.ThrowIfNull(product);
        ArgumentNullException.ThrowIfNull(account);
        ArgumentNullException.ThrowIfNull(scenario);
        ArgumentNullException.ThrowIfNull(policy);

        var rejected = Validate(product, account, scenario);
        if (rejected.Count > 0)
        {
            return new ProductFilterDecision
            {
                Product = product,
                Account = account,
                Scenario = scenario,
                Status = ProductFilterStatus.Rejected,
                Reasons = rejected,
            };
        }

        var economics = product.Economics;
        var marginPerLot = economics.MarginPerLot;
        var tickValue = economics.TickValue;
        var marginMoney = marginPerLot * scenario.Lots;
        var marginRateOfEquity = marginMoney / account.Equity;
        var stopRiskMoney = scenario.StopTicks * tickValue * scenario.Lots;
        var slippageMoney = scenario.SlippageTicks * tickValue * scenario.Lots;
        var costMoney = economics.RoundTripFeePerLot * scenario.Lots + slippageMoney;
        var totalRiskMoney = stopRiskMoney + costMoney;
        var riskRate = totalRiskMoney / account.Equity;
        var costRatio = stopRiskMoney > 0
            ? costMoney / stopRiskMoney
            : double.PositiveInfinity;

        var reasons = new List<string>();
        var status = ProductFilterStatus.Allowed;

        if (riskRate > policy.MaxRiskRate)
        {
            status = ProductFilterStatus.Rejected;
            reasons.Add($"单笔风险 {riskRate:P2} 超过拒绝阈值 {policy.MaxRiskRate:P2}");
        }
        else if (riskRate > policy.CautionRiskRate)
        {
            status = ProductFilterStatus.Caution;
            reasons.Add($"单笔风险 {riskRate:P2} 进入谨慎区");
        }

        if (marginRateOfEquity > policy.MaxMarginRateOfEquity)
        {
            status = ProductFilterStatus.Rejected;
            reasons.Add($"保证金占用 {marginRateOfEquity:P2} 超过拒绝阈值 {policy.MaxMarginRateOfEquity:P2}");
        }
        else if (marginRateOfEquity > policy.CautionMarginRateOfEquity && status != ProductFilterStatus.Rejected)
        {
            status = ProductFilterStatus.Caution;
            reasons.Add($"保证金占用 {marginRateOfEquity:P2} 进入谨慎区");
        }

        if (costRatio > policy.MaxCostRatio)
        {
            status = ProductFilterStatus.Rejected;
            reasons.Add($"成本占止损风险比例 {costRatio:P2} 超过拒绝阈值 {policy.MaxCostRatio:P2}");
        }
        else if (costRatio > policy.CautionCostRatio && status != ProductFilterStatus.Rejected)
        {
            status = ProductFilterStatus.Caution;
            reasons.Add($"成本占止损风险比例 {costRatio:P2} 进入谨慎区");
        }

        if (reasons.Count == 0)
        {
            reasons.Add("一手合约压力在当前账户和测算场景下可承受，可进入后续研究");
        }

        return new ProductFilterDecision
        {
            Product = product,
            Account = account,
            Scenario = scenario,
            MarginPerLot = marginPerLot,
            MarginRateOfEquity = marginRateOfEquity,
            TickValue = tickValue,
            StopRiskMoney = stopRiskMoney,
            SlippageMoney = slippageMoney,
            CostMoney = costMoney,
            TotalRiskMoney = totalRiskMoney,
            RiskRate = riskRate,
            CostRatio = costRatio,
            Status = status,
            Reasons = reasons,
        };
    }

    private static List<string> Validate(ProductInfo product, AccountProfile account, FilterScenario scenario)
    {
        var errors = new List<string>();
        var economics = product.Economics;

        if (account.Equity <= 0 || !double.IsFinite(account.Equity))
        {
            errors.Add("账户权益必须是有限正数");
        }

        if (string.IsNullOrWhiteSpace(product.Identity.ProductCode))
        {
            errors.Add("品种代码不能为空");
        }

        if (string.IsNullOrWhiteSpace(product.Identity.ContractCode))
        {
            errors.Add("合约代码不能为空");
        }

        if (economics.Price <= 0 || !double.IsFinite(economics.Price))
        {
            errors.Add("价格必须是有限正数");
        }

        if (economics.MarginPerLot <= 0 || !double.IsFinite(economics.MarginPerLot))
        {
            errors.Add("一手保证金必须是有限正数");
        }

        if (economics.TickValue <= 0 || !double.IsFinite(economics.TickValue))
        {
            errors.Add("一跳金额必须是有限正数");
        }

        if (economics.RoundTripFeePerLot < 0 || !double.IsFinite(economics.RoundTripFeePerLot))
        {
            errors.Add("单手开平总手续费不能为负数");
        }

        if (scenario.Lots <= 0)
        {
            errors.Add("测算手数必须大于 0");
        }

        if (scenario.StopTicks <= 0)
        {
            errors.Add("止损 tick 数必须大于 0");
        }

        if (scenario.SlippageTicks < 0)
        {
            errors.Add("滑点 tick 数不能为负数");
        }

        return errors;
    }
}
