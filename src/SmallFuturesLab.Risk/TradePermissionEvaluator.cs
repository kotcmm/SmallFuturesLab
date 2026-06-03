namespace SmallFuturesLab.Risk;

/// <summary>
/// 交易许可评估器。
/// 纯计算模块，不读取行情、不访问网络、不写数据库、不调用交易接口、不依赖当前时间。
/// 判断在当前账户、品种、周期和止损设想下，该交易是否被账户允许继续研究。
/// </summary>
public class TradePermissionEvaluator
{
    /// <summary>
    /// 评估交易许可。
    /// </summary>
    /// <param name="account">账户快照。</param>
    /// <param name="instrument">合约规格。</param>
    /// <param name="trade">交易设想。</param>
    /// <param name="policy">风险政策。</param>
    /// <returns>交易许可评估结果。</returns>
    public TradePermissionResult Evaluate(
        AccountSnapshot account,
        InstrumentSpec instrument,
        TradeIdea trade,
        RiskPolicy policy)
    {
        var metrics = ComputeMetrics(account, instrument, trade, policy);
        var passed = new List<string>();
        var warnings = new List<string>();
        var rejections = new List<string>();

        // 硬性禁止条件
        if (trade.Lots != 1)
        {
            rejections.Add("手数不是 1 手，当前阶段只允许 1 手交易");
        }

        if (!trade.HasStop)
        {
            rejections.Add("无明确止损，无法提前定义 1R");
        }

        if (trade.IsOvernight)
        {
            rejections.Add("隔夜交易，当前阶段不研究隔夜持仓");
        }

        if (trade.IsAddPosition)
        {
            rejections.Add("加仓交易，当前阶段不研究加仓策略");
        }

        if (metrics.RiskRate > policy.ExtremeRiskRate)
        {
            rejections.Add($"单笔风险 {metrics.RiskRate:P2} 超过极限上限 {policy.ExtremeRiskRate:P2}");
        }

        if (metrics.MarginRateOfEquity > policy.ExtremeMarginRate)
        {
            rejections.Add($"保证金占用 {metrics.MarginRateOfEquity:P2} 超过极限上限 {policy.ExtremeMarginRate:P2}");
        }

        if (metrics.CostRatio > policy.ExtremeCostRatio)
        {
            rejections.Add($"成本占比 {metrics.CostRatio:P2} 超过极限上限 {policy.ExtremeCostRatio:P2}");
        }

        if (metrics.ProjectedDailyLoss > metrics.DailyLossLimitCash)
        {
            rejections.Add($"本笔亏损后今日累计亏损 {metrics.ProjectedDailyLoss:F2} 元超过每日亏损上限 {metrics.DailyLossLimitCash:F2} 元");
        }

        if (account.TradeCountToday >= policy.MaxTradesPerDay)
        {
            rejections.Add($"今日交易次数 {account.TradeCountToday} 已达上限 {policy.MaxTradesPerDay}");
        }

        // 连续亏损压力测试：以极限单笔风险上限为基准定义严重失败，
        // 确保 RiskRate 在常规上限（1%）与极限上限（2%）之间时输出 Caution 而非 Rejected。
        if (metrics.LossAfter5Rate > 0.10 || metrics.LossAfter8Rate > 0.16 || metrics.LossAfter10Rate > 0.20)
        {
            rejections.Add("连续亏损压力测试严重失败");
        }

        // 如果没有拒绝，检查通过项和警告项
        if (rejections.Count == 0)
        {
            if (metrics.RiskRate <= policy.RecommendedRiskRate)
            {
                passed.Add($"单笔风险 {metrics.RiskRate:P2} 在推荐范围 {policy.RecommendedRiskRate:P2} 内");
            }
            else if (metrics.RiskRate <= policy.NormalRiskRate)
            {
                passed.Add($"单笔风险 {metrics.RiskRate:P2} 在常规上限 {policy.NormalRiskRate:P2} 内");
            }
            else
            {
                warnings.Add($"单笔风险 {metrics.RiskRate:P2} 超过常规上限 {policy.NormalRiskRate:P2}，处于谨慎区间");
            }

            if (metrics.MarginRateOfEquity <= policy.PreferredMarginRate)
            {
                passed.Add($"保证金占用 {metrics.MarginRateOfEquity:P2} 在优先范围 {policy.PreferredMarginRate:P2} 内");
            }
            else
            {
                warnings.Add($"保证金占用 {metrics.MarginRateOfEquity:P2} 超过推荐上限 {policy.PreferredMarginRate:P2}，处于谨慎区间");
            }

            if (metrics.CostRatio <= policy.PreferredCostRatio)
            {
                passed.Add($"成本占比 {metrics.CostRatio:P2} 在优先范围 {policy.PreferredCostRatio:P2} 内");
            }
            else
            {
                warnings.Add($"成本占比 {metrics.CostRatio:P2} 超过推荐上限 {policy.PreferredCostRatio:P2}，处于谨慎区间");
            }

            passed.Add("连续亏损压力测试通过");
            passed.Add("每日亏损限制通过");
            passed.Add("交易次数限制通过");
            passed.Add("硬性条件通过");

            // 连续亏损压力接近上限（以常规单笔风险上限为基准）
            if (metrics.LossAfter5Rate > 0.05 || metrics.LossAfter8Rate > 0.08 || metrics.LossAfter10Rate > 0.10)
            {
                warnings.Add("连续亏损压力接近上限");
            }

            // 今日已出现亏损但未达上限
            if (account.DailyLossSoFar > 0 && metrics.ProjectedDailyLoss <= metrics.DailyLossLimitCash)
            {
                warnings.Add("今日已出现亏损，但未达到日亏损上限");
            }
        }

        var status = rejections.Count > 0
            ? TradePermissionStatus.Rejected
            : warnings.Count > 0
                ? TradePermissionStatus.Caution
                : TradePermissionStatus.Allowed;

        var conclusion = status switch
        {
            TradePermissionStatus.Allowed => "允许继续研究或模拟交易。",
            TradePermissionStatus.Caution => "可继续模拟观察，但不是优先候选，不应直接进入实盘。",
            TradePermissionStatus.Rejected => "拒绝交易，行情再好也不做。",
            _ => string.Empty,
        };

        return new TradePermissionResult
        {
            Status = status,
            Metrics = metrics,
            PassedItems = passed.AsReadOnly(),
            WarningItems = warnings.AsReadOnly(),
            RejectedItems = rejections.AsReadOnly(),
            Conclusion = conclusion,
        };
    }

    private static RiskMetrics ComputeMetrics(
        AccountSnapshot account,
        InstrumentSpec instrument,
        TradeIdea trade,
        RiskPolicy policy)
    {
        var tickValue = instrument.TickSize * instrument.Multiplier;
        var notionalPerLot = instrument.Price * instrument.Multiplier;
        var marginPerLot = notionalPerLot * instrument.MarginRatio;

        var rawStopDistance = Math.Abs(trade.EntryPrice - trade.StopPrice);
        var stopTicks = (int)Math.Ceiling(rawStopDistance / instrument.TickSize);
        var adjustedStopDistance = stopTicks * instrument.TickSize;

        var stopRiskPerLot = adjustedStopDistance * instrument.Multiplier;
        var stopRisk = stopRiskPerLot * trade.Lots;

        var slippageMoney = trade.SlippageTicks * instrument.TickSize * instrument.Multiplier * trade.Lots;
        var costMoney = instrument.FeePerRoundTrip * trade.Lots + slippageMoney;

        var totalRiskMoney = stopRisk + costMoney;
        var riskRate = totalRiskMoney / account.Equity;

        var costRatio = stopRisk > 0 ? costMoney / stopRisk : double.PositiveInfinity;

        var marginMoney = marginPerLot * trade.Lots;
        var marginRateOfEquity = marginMoney / account.Equity;

        var lossAfter5 = totalRiskMoney * 5;
        var lossAfter8 = totalRiskMoney * 8;
        var lossAfter10 = totalRiskMoney * 10;

        var lossAfter5Rate = lossAfter5 / account.Equity;
        var lossAfter8Rate = lossAfter8 / account.Equity;
        var lossAfter10Rate = lossAfter10 / account.Equity;

        var projectedDailyLoss = account.DailyLossSoFar + totalRiskMoney;
        var dailyLossLimitCash = account.Equity * policy.DailyMaxLossRate;

        return new RiskMetrics
        {
            TickValue = tickValue,
            NotionalPerLot = notionalPerLot,
            MarginPerLot = marginPerLot,
            StopTicks = stopTicks,
            AdjustedStopDistance = adjustedStopDistance,
            StopRiskPerLot = stopRiskPerLot,
            StopRisk = stopRisk,
            SlippageMoney = slippageMoney,
            CostMoney = costMoney,
            TotalRiskMoney = totalRiskMoney,
            RiskRate = riskRate,
            CostRatio = costRatio,
            MarginMoney = marginMoney,
            MarginRateOfEquity = marginRateOfEquity,
            LossAfter5 = lossAfter5,
            LossAfter8 = lossAfter8,
            LossAfter10 = lossAfter10,
            LossAfter5Rate = lossAfter5Rate,
            LossAfter8Rate = lossAfter8Rate,
            LossAfter10Rate = lossAfter10Rate,
            ProjectedDailyLoss = projectedDailyLoss,
            DailyLossLimitCash = dailyLossLimitCash,
            RecommendedRiskCash = account.Equity * policy.RecommendedRiskRate,
            NormalRiskCash = account.Equity * policy.NormalRiskRate,
            ExtremeRiskCash = account.Equity * policy.ExtremeRiskRate,
        };
    }
}
