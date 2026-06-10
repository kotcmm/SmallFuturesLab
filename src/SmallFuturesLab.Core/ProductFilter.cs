namespace SmallFuturesLab.Core;

/// <summary>
/// 小资金期货品种过滤器。
/// 执行公式计算和阈值判断，输出 Allowed / Caution / Rejected。
/// 不读取文件、不知道交易星球、不生成交易建议、不输出买卖方向。
/// </summary>
public sealed class ProductFilter
{
    /// <summary>
    /// 对单个合约进行评估，返回过滤结果。
    /// </summary>
    /// <param name="contract">期货合约参数。</param>
    /// <param name="config">账户风险配置。</param>
    /// <returns>过滤结果。</returns>
    public ProductFilterResult Evaluate(FuturesContract contract, RiskConfig config)
    {
        ArgumentNullException.ThrowIfNull(contract);
        ArgumentNullException.ThrowIfNull(config);

        var reasons = new List<string>();
        Validate(contract, config, reasons);

        if (reasons.Count > 0)
        {
            return new ProductFilterResult
            {
                ProductCode = contract.ProductCode,
                ContractCode = contract.ContractCode,
                RiskRate = 0,
                MarginRate = 0,
                CostRatio = 0,
                TotalRiskMoney = 0,
                MarginMoney = 0,
                Status = ProductFilterStatus.Rejected,
                Reasons = reasons,
            };
        }

        var tickValue = contract.TickValue;
        var marginPerLot = contract.MarginPerLot;
        var marginMoney = marginPerLot * contract.Lots;
        var stopRiskMoney = contract.StopTicks * tickValue * contract.Lots;
        var slippageMoney = contract.SlippageTicks * tickValue * contract.Lots;
        var costMoney = contract.RoundTripFee * contract.Lots + slippageMoney;
        var totalRiskMoney = stopRiskMoney + costMoney;
        var riskRate = totalRiskMoney / config.AccountEquity;
        var marginRate = marginMoney / config.AccountEquity;
        var costRatio = stopRiskMoney > 0 ? costMoney / stopRiskMoney : double.PositiveInfinity;

        var status = ProductFilterStatus.Allowed;

        if (riskRate > config.RejectRiskRate)
        {
            status = ProductFilterStatus.Rejected;
            reasons.Add($"单笔风险 {riskRate:P2} 超过拒绝阈值 {config.RejectRiskRate:P2}");
        }
        else if (riskRate > config.CautionRiskRate)
        {
            status = ProductFilterStatus.Caution;
            reasons.Add($"单笔风险 {riskRate:P2} 进入谨慎区");
        }

        if (marginRate > config.RejectMarginRate)
        {
            status = ProductFilterStatus.Rejected;
            reasons.Add($"保证金占用 {marginRate:P2} 超过拒绝阈值 {config.RejectMarginRate:P2}");
        }
        else if (marginRate > config.CautionMarginRate && status != ProductFilterStatus.Rejected)
        {
            status = ProductFilterStatus.Caution;
            reasons.Add($"保证金占用 {marginRate:P2} 进入谨慎区");
        }

        if (costRatio > config.RejectCostRatio)
        {
            status = ProductFilterStatus.Rejected;
            reasons.Add($"成本占止损风险 {costRatio:P2} 超过拒绝阈值 {config.RejectCostRatio:P2}");
        }
        else if (costRatio > config.CautionCostRatio && status != ProductFilterStatus.Rejected)
        {
            status = ProductFilterStatus.Caution;
            reasons.Add($"成本占止损风险 {costRatio:P2} 进入谨慎区");
        }

        if (reasons.Count == 0)
        {
            reasons.Add("当前账户规模下，一手合约压力可接受");
        }

        return new ProductFilterResult
        {
            ProductCode = contract.ProductCode,
            ContractCode = contract.ContractCode,
            RiskRate = riskRate,
            MarginRate = marginRate,
            CostRatio = costRatio,
            TotalRiskMoney = totalRiskMoney,
            MarginMoney = marginMoney,
            Status = status,
            Reasons = reasons,
        };
    }

    private static void Validate(FuturesContract contract, RiskConfig config, List<string> reasons)
    {
        if (config.AccountEquity <= 0 || !double.IsFinite(config.AccountEquity))
        {
            reasons.Add("账户权益必须是有限正数");
        }

        if (string.IsNullOrWhiteSpace(contract.ProductCode))
        {
            reasons.Add("品种代码不能为空");
        }

        if (string.IsNullOrWhiteSpace(contract.ContractCode))
        {
            reasons.Add("合约代码不能为空");
        }

        if (contract.Price <= 0 || !double.IsFinite(contract.Price))
        {
            reasons.Add("价格必须是有限正数");
        }

        if (contract.Multiplier <= 0 || !double.IsFinite(contract.Multiplier))
        {
            reasons.Add("合约乘数必须是有限正数");
        }

        if (contract.TickSize <= 0 || !double.IsFinite(contract.TickSize))
        {
            reasons.Add("最小变动价位必须是有限正数");
        }

        if (contract.MarginRate <= 0 || !double.IsFinite(contract.MarginRate))
        {
            reasons.Add("保证金比例必须是有限正数");
        }

        if (contract.RoundTripFee < 0 || !double.IsFinite(contract.RoundTripFee))
        {
            reasons.Add("单手开平总手续费不能为负数");
        }

        if (contract.StopTicks <= 0)
        {
            reasons.Add("止损 tick 数必须大于 0");
        }

        if (contract.SlippageTicks < 0)
        {
            reasons.Add("滑点 tick 数不能为负数");
        }

        if (contract.Lots <= 0)
        {
            reasons.Add("测算手数必须大于 0");
        }
    }
}
