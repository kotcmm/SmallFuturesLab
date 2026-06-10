namespace SmallFuturesLab.Core;

public sealed class ProductEvaluation
{
    public ProductEvaluation(ProductInfo product, double accountEquity, ProductRiskConfig productRiskConfig)
    {
        TickValue = product.TickSize * product.Multiplier;
        MarginPerLot = product.Price * product.Multiplier * product.MarginRate;
        MarginRateOfEquity = MarginPerLot * productRiskConfig.Lots / accountEquity;
        StopRiskMoney = productRiskConfig.StopTicks * TickValue * productRiskConfig.Lots;
        SlippageMoney = productRiskConfig.SlippageTicks * TickValue * productRiskConfig.Lots;
        CostMoney = product.RoundTripFee * productRiskConfig.Lots + SlippageMoney;
        TotalRiskMoney = StopRiskMoney + CostMoney;
        RiskRate = TotalRiskMoney / accountEquity;
        CostRatio = StopRiskMoney > 0 ? CostMoney / StopRiskMoney : double.PositiveInfinity;
    }
    /// <summary>
    /// 一跳金额。
    /// TickValue = TickSize * Multiplier。
    /// </summary>
    public double TickValue { get; private set; }

    /// <summary>
    /// 一手保证金金额。
    /// MarginPerLot = Price * Multiplier * MarginRate。
    /// </summary>
    public double MarginPerLot { get; private set; }

    /// <summary>
    /// 保证金占账户比例。
    /// MarginRateOfEquity = MarginRateOfEquity = MarginPerLot × Lots / AccountEquity。
    /// </summary>
    public double MarginRateOfEquity { get; private set; }

    /// <summary>
    /// 止损风险金额。
    /// StopRiskMoney = ProductRiskConfig.StopTicks * Product.TickValue * ProductRiskConfig.Lots。
    /// </summary>
    public double StopRiskMoney { get; private set; }

    /// <summary>
    /// 滑点金额。
    /// SlippageMoney = ProductRiskConfig.SlippageTicks * Product.TickValue * ProductRiskConfig.Lots。
    /// </summary>
    public double SlippageMoney { get; private set; }

    /// <summary>
    /// 成本金额，包含手续费和滑点。
    /// CostMoney = Product.RoundTripFee * ProductRiskConfig.Lots + SlippageMoney。
    /// </summary>
    public double CostMoney { get; private set; }

    /// <summary>
    /// 总风险金额，包含止损、手续费和滑点。
    /// TotalRiskMoney = StopRiskMoney + CostMoney。
    /// </summary>
    public double TotalRiskMoney { get; private set; }

    /// <summary>
    /// 总风险占账户权益比例。
    /// RiskRate = TotalRiskMoney / RiskConfig.AccountEquity。
    /// </summary>
    public double RiskRate { get; private set; }

    /// <summary>
    /// 成本占止损风险比例。
    /// CostRatio = CostMoney / StopRiskMoney。
    /// 当 StopRiskMoney 为 0 时返回正无穷。
    /// </summary>
    public double CostRatio { get; private set; }

    /// <summary>
    /// 根据风险配置评估产品的过滤状态。
    /// </summary>
    /// <param name="riskConfig"></param>
    /// <returns></returns>
    public ProductEvaluationStatus Evaluate(AccountRiskConfig riskConfig)
    {
        if (RiskRate > riskConfig.RejectRiskRate ||
            MarginRateOfEquity > riskConfig.RejectMarginRate ||
            CostRatio > riskConfig.RejectCostRatio)
        {
            return ProductEvaluationStatus.Rejected;
        }

        if (RiskRate > riskConfig.CautionRiskRate ||
            MarginRateOfEquity > riskConfig.CautionMarginRate ||
            CostRatio > riskConfig.CautionCostRatio)
        {
            return ProductEvaluationStatus.Caution;
        }

        return ProductEvaluationStatus.Allowed;
    }
}
