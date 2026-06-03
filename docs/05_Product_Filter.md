# 品种筛选

文件：`docs/05_Product_Filter.md`

---

## 1. 文档目的

本文档定义 SmallFuturesLab 的品种筛选规则。

品种筛选不判断行情方向，不判断入场信号，不判断策略收益。

它只回答：

```text
某个期货品种，在给定账户规模下，是否有资格进入后续周期研究和候选方向研究？
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
→ 交易许可流水线
→ 品种筛选
→ 周期筛选
→ 风控底线
→ 候选方向评价
```

品种筛选必须使用 `docs/04_Trade_Permission_Pipeline.md` 中定义的交易许可逻辑。

如果某个品种的一手风险无法通过交易许可，则该品种不进入后续行情研究。

---

## 3. 筛选结论

每条测算记录只能输出三类结论：

```text
Allowed   = 允许进入后续周期研究；
Caution   = 谨慎，只允许继续观察或模拟测算；
Rejected  = 排除，当前账户规模不研究。
```

`Allowed` 不代表可以实盘交易。

`Allowed` 只代表：

```text
该品种在当前账户规模、账户颗粒度、保证金、成本和一手风险上，暂时具备继续研究资格。
```

---

## 4. 账户规模建模

账户规模是测算维度，不是固定字段名。

模板使用：

```text
AccountEquity
```

表示当前记录使用的账户权益。

不要在模型或 CSV 中固化：

```text
RiskRate10k
RiskRate20k
MarginRate10k
MarginRate20k
Result10k
Result20k
```

原因：

```text
1 万和 2 万只是当前默认测算场景；
未来可能增加 3 万、4 万、5 万或其他账户规模；
账户规模变化不应该导致代码模型、CSV 表头和测试大面积修改。
```

默认第一轮账户规模：

```text
10,000 元；
20,000 元。
```

如果后续要测算 30,000 元、40,000 元、50,000 元，只增加对应 `AccountEquity` 记录行，不修改字段结构。

---

## 5. 必须收集的字段

每条测算记录必须至少收集或生成：

```text
交易所；
品种名称；
品种代码；
合约代码；
合约乘数；
最小变动价位；
一跳金额；
典型价格；
保证金比例；
一手保证金估计；
开平总手续费估计；
预估滑点 tick；
盘口连续性；
成交活跃度；
主力合约换月是否清晰；
典型 ATR；
1 手 1 ATR 金额；
常见止损距离；
常见止损金额；
账户权益；
手续费 + 滑点占止损风险比例；
当前账户规模下的风险占比；
当前账户规模下的保证金占比；
初步结论；
结论原因；
数据日期；
数据来源。
```

没有数据日期和数据来源的记录，不进入正式筛选表。

---

## 6. 基础计算公式

### 6.1 一跳金额

```text
TickValue = TickSize × Multiplier
```

### 6.2 一手名义金额

```text
NotionalPerLot = Price × Multiplier
```

### 6.3 一手保证金估计

```text
MarginPerLot = Price × Multiplier × MarginRate
```

### 6.4 保证金占比

```text
MarginRateOfEquity = MarginPerLot / AccountEquity
```

### 6.5 1 手 1 ATR 金额

```text
AtrMoneyPerLot = ATR × Multiplier
```

### 6.6 常见止损金额

```text
StopRiskMoney = StopDistance × Multiplier
```

### 6.7 滑点金额

```text
SlippageMoney = SlippageTicks × TickSize × Multiplier
```

### 6.8 成本金额

```text
CostMoney = RoundTripFeePerLot + SlippageMoney
```

### 6.9 成本占比

```text
CostRatio = CostMoney / StopRiskMoney
```

### 6.10 实际 1R

```text
TotalRiskMoney = StopRiskMoney + CostMoney
```

### 6.11 风险占账户比例

```text
RiskRate = TotalRiskMoney / AccountEquity
```

---

## 7. 常见止损距离的定义

品种筛选阶段不定义最终策略止损。

但为了判断品种是否适合小资金，必须给出几个用于测算的常见止损距离。

至少测算：

```text
最小止损：3 tick；
短线止损：5 tick；
中短线止损：10 tick；
ATR 止损：0.5 ATR；
ATR 止损：1.0 ATR。
```

如果某个品种连 `5 tick` 或 `0.5 ATR` 的一手风险都明显超出账户承受能力，则该品种不适合当前阶段。

---

## 8. 账户测算要求

每个品种必须至少测算：

```text
AccountEquity = 10000；
AccountEquity = 20000。
```

并分别输出：

```text
单笔风险占比；
保证金占比；
连续 5 次亏损占比；
连续 8 次亏损占比；
连续 10 次亏损占比；
手续费 + 滑点占止损风险比例；
Allowed / Caution / Rejected。
```

如果同一品种在不同账户规模下结论不同，应明确标记账户差异。

账户规模增加时，只增加新的 `AccountEquity` 场景，不新增字段。

---

## 9. 优先研究条件

满足以下条件的记录，优先进入后续周期研究：

```text
1 手常见止损风险 <= 当前账户权益 1%；
保证金占用 <= 当前账户权益 40%；
手续费 + 滑点占止损风险 <= 0.2；
成交活跃；
盘口连续；
买卖价差小；
主力合约换月清晰；
不依赖隔夜持仓；
适合人工观察和复盘。
```

---

## 10. 谨慎研究条件

满足以下任意条件，标记为 `Caution`：

```text
1 手常见止损风险 > 当前账户权益 1% 但 <= 当前账户权益 2%；
保证金占用 > 当前账户权益 40% 但 <= 当前账户权益 50%；
手续费 + 滑点占止损风险 > 0.2 但 <= 0.3；
成交活跃度不稳定；
盘口在部分时段变薄；
换月期间流动性迁移不清晰；
对较小账户不友好，但对较大账户可能可研究。
```

`Caution` 品种不得直接进入实盘假设，只能继续观察或模拟测算。

---

## 11. 排除条件

满足以下任意条件，标记为 `Rejected`：

```text
1 手常见止损风险 > 当前账户权益 2%；
保证金占用 > 当前账户权益 50%；
手续费 + 滑点占止损风险 > 0.3；
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

## 12. 盘口与流动性判断

当前阶段不研究盘口高频交易，但品种筛选必须检查流动性。

至少记录：

```text
成交是否活跃；
盘口是否连续；
买卖价差是否稳定；
止损时是否可能出现明显滑点；
主力合约是否容易识别；
换月时是否容易迁移。
```

这些字段允许先使用人工分级：

```text
Good     = 良好；
Medium   = 一般；
Poor     = 较差；
Unknown  = 暂无数据。
```

`Poor` 或 `Unknown` 不直接等于排除，但不能输出 `Allowed`。

---

## 13. 筛选表模板

品种筛选表模板位于：

```text
templates/product_filter_template.csv
```

该模板只定义字段结构，不代表实际筛选结论。

模板至少包含：

| 字段 | 说明 |
|---|---|
| Exchange | 交易所 |
| ProductName | 品种名称 |
| ProductCode | 品种代码 |
| ContractCode | 合约代码 |
| Price | 典型价格 |
| Multiplier | 合约乘数 |
| TickSize | 最小变动价位 |
| TickValue | 一跳金额 |
| MarginRate | 保证金比例 |
| MarginPerLot | 一手保证金 |
| RoundTripFeePerLot | 开平总手续费 |
| SlippageTicks | 预估滑点 tick |
| TypicalAtr | 典型 ATR |
| AtrMoneyPerLot | 1 手 1 ATR 金额 |
| StopDistance | 测算止损距离 |
| StopRiskMoney | 止损金额 |
| SlippageMoney | 滑点金额 |
| CostMoney | 成本金额 |
| TotalRiskMoney | 含成本 1R |
| AccountEquity | 当前记录使用的账户权益 |
| RiskRate | 当前账户规模下的风险占比 |
| MarginRateOfEquity | 当前账户规模下的保证金占比 |
| CostRatio | 成本占止损风险比例 |
| LiquidityLevel | 流动性等级 |
| BookContinuityLevel | 盘口连续性等级 |
| RolloverClarity | 换月清晰度 |
| Result | 当前账户规模下的结论 |
| Reasons | 结论原因 |
| DataDate | 数据日期 |
| DataSource | 数据来源 |

使用模板时必须遵守：

```text
不填入未经确认的数据；
每条记录必须包含 DataDate；
每条记录必须包含 DataSource；
AccountEquity 必须大于 0；
Result 只能使用 Allowed / Caution / Rejected；
Reasons 必须写明结论原因。
```

---

## 14. 单品种输出格式

每个品种应按账户规模输出结构化结论：

```text
品种：
合约：
数据日期：
数据来源：

核心指标：
一跳金额：
一手保证金：
典型 ATR：
1 手 1 ATR 金额：
常见止损金额：
手续费 + 滑点占比：

AccountEquity：
结论：Allowed / Caution / Rejected
原因：

最终结论：
是否进入后续周期研究：是 / 否 / 谨慎观察
```

---

## 15. 与交易许可模块的关系

品种筛选不重新定义风险公式。

品种筛选应把每个测算场景转换成交易许可输入：

```text
AccountSnapshot
InstrumentSpec
TradeIdea
RiskPolicy
```

其中 `AccountSnapshot.Equity` 来自当前记录的 `AccountEquity`。

然后使用交易许可结果作为品种筛选依据。

如果品种筛选文档和交易许可文档出现冲突，以 `docs/04_Trade_Permission_Pipeline.md` 为准，并先修正文档后再实现代码。

---

## 16. 当前不做什么

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
不讨论多品种组合。
```

---

## 17. 通过标准

品种筛选阶段完成时，必须产出：

```text
按 AccountEquity 分组的候选品种白名单；
按 AccountEquity 分组的谨慎观察品种列表；
按 AccountEquity 分组的排除品种列表；
每条记录的计算依据；
每条记录的结论原因；
数据日期和数据来源。
```

没有明确结论原因的记录，不允许进入后续周期研究。

---

## 18. 当前结论

SmallFuturesLab 的品种筛选不是寻找“好行情品种”。

它只筛选：

```text
账户是否承受得起；
一手颗粒度是否合适；
保证金是否过重；
交易成本是否过高；
流动性是否支持止损退出。
```

只有通过这些条件的品种，才有资格进入周期研究。