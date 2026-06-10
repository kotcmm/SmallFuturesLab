# 品种筛选

文件：`docs/05_Product_Filter.md`

---

## 1. 文档目的

本文档定义 SmallFuturesLab 的品种筛选规则。

品种筛选不判断行情方向，不判断入场信号，不判断策略收益。

它只回答：

```text
某个期货品种，在给定账户规模下，是否有资格进入后续研究？
```

核心原则：

```text
品种先通过账户和风险筛选，才允许讨论行情。
```

---

## 2. 品种筛选的位置

当前研究顺序是：

```text
账户约束
→ 品种测算公式
→ 品种筛选
→ 周期筛选
→ 风控底线
→ 候选方向评价
```

品种筛选必须使用 `docs/04_Product_Evaluation_Formula.md` 中定义的测算公式和阈值逻辑。

如果某个品种的一手风险无法通过测算，则该品种不进入后续行情研究。

---

## 3. 筛选结论

每条测算记录只能输出三类结论：

```text
Allowed   = 允许进入后续研究；
Caution   = 谨慎，只允许继续观察或模拟测算；
Rejected  = 排除，当前账户规模不研究。
```

`Allowed` 不代表可以实盘交易。

`Allowed` 只代表：

```text
该品种在当前账户规模、保证金、成本和一手风险上，暂时具备继续研究资格。
```

---

## 4. 核心公式

品种筛选依赖 `docs/04_Product_Evaluation_Formula.md` 中的公式：

```text
TickValue = TickSize × Multiplier
MarginPerLot = Price × Multiplier × MarginRate
MarginRateOfEquity = MarginPerLot × Lots / AccountEquity
StopRiskMoney = StopTicks × TickValue × Lots
SlippageMoney = SlippageTicks × TickValue × Lots
CostMoney = RoundTripFee × Lots + SlippageMoney
TotalRiskMoney = StopRiskMoney + CostMoney
RiskRate = TotalRiskMoney / AccountEquity
CostRatio = CostMoney / StopRiskMoney
```

公式字段由测算逻辑自动计算，不应人工手填。

---

## 5. 优先研究条件

满足以下条件的记录，优先进入后续研究：

```text
RiskRate <= 1%；
MarginRateOfEquity <= 40%；
CostRatio <= 0.2；
成交活跃；
盘口连续；
买卖价差小；
主力合约换月清晰；
不依赖隔夜持仓；
适合人工观察和复盘。
```

---

## 6. 谨慎研究条件

满足以下任意条件，标记为 `Caution`：

```text
RiskRate > 1% 但 <= 2%；
MarginRateOfEquity > 40% 但 <= 50%；
CostRatio > 0.2 但 <= 0.3；
成交活跃度不稳定；
盘口在部分时段变薄；
换月期间流动性迁移不清晰；
对较小账户不友好，但对较大账户可能可研究。
```

`Caution` 品种不得直接进入实盘假设，只能继续观察或模拟测算。

---

## 7. 排除条件

满足以下任意条件，标记为 `Rejected`：

```text
RiskRate > 2%；
MarginRateOfEquity > 50%；
CostRatio > 0.3；
一跳金额过大，导致止损颗粒度不可接受；
成交不活跃；
盘口过薄；
买卖价差过大；
止损可能无法正常成交；
主力合约不清晰；
需要隔夜才能体现交易逻辑；
容易诱导扛单、摊平或重仓。
```

排除记录必须记录排除原因。

---

## 8. 与测算公式的关系

品种筛选不重新定义风险公式。

品种筛选直接调用 `docs/04_Product_Evaluation_Formula.md` 中定义的测算逻辑：

```text
品种数据 + 账户配置 + 测算条件
→ 公式计算
→ Allowed / Caution / Rejected
```

如果品种筛选文档和测算公式文档出现冲突，以 `docs/04_Product_Evaluation_Formula.md` 为准，并先修正文档后再实现代码。

---

## 9. 当前不做什么

当前品种筛选阶段不做：

```text
不选择具体入场信号；
不判断趋势；
不判断震荡；
不判断胜率；
不做收益回测；
不做参数优化；
不讨论加仓；
不讨论隔夜；
不讨论多品种组合；
不输出 CSV 批量报告；
不生成 Markdown 汇总。
```

---

## 10. 当前结论

SmallFuturesLab 的品种筛选不是寻找"好行情品种"。

它只筛选：

```text
账户是否承受得起；
一手颗粒度是否合适；
保证金是否过重；
交易成本是否过高；
流动性是否支持止损退出。
```

只有通过这些条件的品种，才有资格进入周期研究。
