# 日内候选品种筛选

## 1. 目的

日内候选品种筛选，是在每天开盘后，用账户风险、合约参数和开盘市场数据，筛出当天值得关注的少数候选品种。

输出结果：

```text
候选品种列表
```

每个候选品种必须能回答三个问题：

1. 一手风险是否能被账户承受；
2. 成本是否不会吞噬 R；
3. 当天波动空间是否值得继续观察。

---

## 2. 核心原则

先看能不能做，再看值不值得做。

判断顺序：

```text
账户风险 → 保证金 → 一手风险 → 成本占比 → 流动性 → 波动空间 → 候选排序
```

核心约束：

```text
TradeR <= AccountR
c <= c_max
```

---

## 3. 输入参数

输入参数分四类。

### 3.1 账户配置

| 参数 | 含义 | 示例 |
|---|---|---:|
| AccountEquity | 账户权益 | 50,000 |
| RiskPercentPerTrade | 单笔风险比例 | 0.5% |
| MaxMarginUsagePercent | 单品种最大保证金占用比例 | 25% |
| CostMaxR | 成本上限 | 0.20R |
| MinExpectedRewardR | 最低期望盈利空间 | 2.5R |
| MaxCandidates | 最多候选品种数 | 3 |

账户风险上限：

```text
AccountR = AccountEquity × RiskPercentPerTrade
```

---

### 3.2 合约静态参数

| 参数 | 含义 |
|---|---|
| Symbol | 品种代码 |
| ContractName | 合约名称 |
| Multiplier | 合约乘数 |
| TickSize | 最小变动价位 |
| MarginRate | 保证金比例 |
| RoundTripFeePerLot | 单手开平合计手续费 |

一跳价值：

```text
TickValue = TickSize × Multiplier
```

---

### 3.3 开盘市场数据

| 参数 | 含义 |
|---|---|
| OpenPrice | 开盘价或筛选时最新价 |
| BidPrice1 | 买一价 |
| AskPrice1 | 卖一价 |
| OpeningVolume | 开盘观察窗口成交量 |
| ExpectedIntradayMoveTicks | 预估日内可用波动空间，单位是 tick |

买卖价差：

```text
SpreadTicks = (AskPrice1 - BidPrice1) / TickSize
```

---

### 3.4 风险测算模板

| 参数 | 含义 | 示例 |
|---|---|---:|
| TestStopTicks | 测算用止损距离，单位是 tick | 20 |
| ExpectedSlippageTicksRoundTrip | 预估开平合计滑点，单位是 tick | 2 |
| MaxSpreadTicks | 最大允许买卖价差，单位是 tick | 2 |
| MinOpeningVolume | 最低开盘观察窗口成交量 | 1000 |

风险测算模板用于日内候选品种初筛。

后续每一笔具体交易仍然使用自己的入场价和止损价重新计算 `TradeR`。

---

## 4. 推算过程

### 4.1 账户风险上限

```text
AccountR = AccountEquity × RiskPercentPerTrade
```

示例：

```text
AccountR = 50,000 × 0.5%
AccountR = 250
```

---

### 4.2 一手保证金

```text
OneLotMargin = OpenPrice × Multiplier × MarginRate
```

保证金占比：

```text
MarginUsagePercent = OneLotMargin / AccountEquity
```

筛选规则：

```text
MarginUsagePercent <= MaxMarginUsagePercent
```

---

### 4.3 一手价格止损风险

```text
OneLotPriceRisk = TestStopTicks × TickValue
```

---

### 4.4 一手预估成本

```text
OneLotEstimatedCost = RoundTripFeePerLot + ExpectedSlippageTicksRoundTrip × TickValue
```

---

### 4.5 一手计划风险

```text
OneLotTradeR = OneLotPriceRisk + OneLotEstimatedCost
```

允许手数：

```text
AllowedLots = floor(AccountR / OneLotTradeR)
```

筛选规则：

```text
AllowedLots >= 1
```

---

### 4.6 成本占比

```text
CostR = OneLotEstimatedCost / OneLotTradeR
```

筛选规则：

```text
CostR <= CostMaxR
```

---

### 4.7 流动性筛选

买卖价差筛选：

```text
SpreadTicks <= MaxSpreadTicks
```

成交量筛选：

```text
OpeningVolume >= MinOpeningVolume
```

---

### 4.8 波动空间筛选

最低目标盈利金额：

```text
RequiredProfitAmount = MinExpectedRewardR × OneLotTradeR
```

折算成需要移动的 tick 数：

```text
RequiredMoveTicks = ceil(RequiredProfitAmount / TickValue)
```

波动空间比例：

```text
SpaceRatio = ExpectedIntradayMoveTicks / RequiredMoveTicks
```

筛选规则：

```text
SpaceRatio >= 1
```

含义：

> 预估日内波动空间至少要覆盖最低期望盈利空间。

---

## 5. 筛选状态

每个品种输出一个状态。

| 状态 | 含义 |
|---|---|
| Rejected | 不满足硬约束 |
| Candidate | 通过硬约束，可以进入候选列表 |
| Preferred | 通过硬约束，并且排序靠前 |

拒绝原因使用明确字段记录。

| 原因 | 含义 |
|---|---|
| MarginTooHigh | 保证金占比过高 |
| RiskTooHigh | 一手计划风险超过 AccountR |
| CostTooHigh | 成本占比超过上限 |
| SpreadTooWide | 买卖价差过大 |
| LiquidityTooLow | 开盘流动性不足 |
| SpaceTooSmall | 预估波动空间不足 |

---

## 6. 候选排序

通过硬约束后，再排序。

排序优先级：

1. `SpaceRatio` 越高越靠前；
2. `CostR` 越低越靠前；
3. `MarginUsagePercent` 越低越靠前；
4. `OpeningVolume` 越高越靠前。

最终只保留：

```text
MaxCandidates
```

默认：

```text
MaxCandidates = 3
```

---

## 7. 输出字段

日内候选品种筛选结果至少包含以下字段。

| 字段 | 含义 |
|---|---|
| Symbol | 品种代码 |
| Status | 筛选状态 |
| RejectReason | 拒绝原因 |
| AccountR | 账户单笔风险上限 |
| OneLotMargin | 一手保证金 |
| MarginUsagePercent | 保证金占比 |
| TickValue | 一跳价值 |
| OneLotTradeR | 一手计划风险 |
| AllowedLots | 允许手数 |
| CostR | 成本占比 |
| SpreadTicks | 买卖价差 |
| OpeningVolume | 开盘成交量 |
| RequiredMoveTicks | 达到目标盈利需要移动的 tick 数 |
| ExpectedIntradayMoveTicks | 预估日内可用波动空间 |
| SpaceRatio | 波动空间比例 |

---

## 8. 完整算例

账户配置：

| 参数 | 数值 |
|---|---:|
| AccountEquity | 50,000 |
| RiskPercentPerTrade | 0.5% |
| MaxMarginUsagePercent | 25% |
| CostMaxR | 0.20R |
| MinExpectedRewardR | 2.5R |
| MaxCandidates | 3 |

风险测算模板：

| 参数 | 数值 |
|---|---:|
| TestStopTicks | 20 |
| ExpectedSlippageTicksRoundTrip | 2 |
| MaxSpreadTicks | 2 |
| MinOpeningVolume | 1000 |

账户风险上限：

```text
AccountR = 50,000 × 0.5%
AccountR = 250
```

候选品种 A：

| 参数 | 数值 |
|---|---:|
| OpenPrice | 3000 |
| Multiplier | 10 |
| TickSize | 1 |
| MarginRate | 10% |
| RoundTripFeePerLot | 10 |
| BidPrice1 | 2999 |
| AskPrice1 | 3000 |
| OpeningVolume | 5000 |
| ExpectedIntradayMoveTicks | 80 |

推算：

```text
TickValue = 1 × 10 = 10
OneLotMargin = 3000 × 10 × 10% = 3000
MarginUsagePercent = 3000 / 50000 = 6%
OneLotPriceRisk = 20 × 10 = 200
OneLotEstimatedCost = 10 + 2 × 10 = 30
OneLotTradeR = 200 + 30 = 230
AllowedLots = floor(250 / 230) = 1
CostR = 30 / 230 = 0.13R
SpreadTicks = (3000 - 2999) / 1 = 1
RequiredProfitAmount = 2.5 × 230 = 575
RequiredMoveTicks = ceil(575 / 10) = 58
SpaceRatio = 80 / 58 = 1.38
```

输出：

```text
Status = Candidate
RejectReason = None
```

---

候选品种 B：

| 参数 | 数值 |
|---|---:|
| OpenPrice | 5000 |
| Multiplier | 10 |
| TickSize | 5 |
| MarginRate | 12% |
| RoundTripFeePerLot | 20 |
| BidPrice1 | 4995 |
| AskPrice1 | 5000 |
| OpeningVolume | 3000 |
| ExpectedIntradayMoveTicks | 70 |

推算：

```text
TickValue = 5 × 10 = 50
OneLotMargin = 5000 × 10 × 12% = 6000
MarginUsagePercent = 6000 / 50000 = 12%
OneLotPriceRisk = 20 × 50 = 1000
OneLotEstimatedCost = 20 + 2 × 50 = 120
OneLotTradeR = 1000 + 120 = 1120
AllowedLots = floor(250 / 1120) = 0
```

输出：

```text
Status = Rejected
RejectReason = RiskTooHigh
```

---

## 9. 最终结果示例

| Symbol | Status | RejectReason | OneLotTradeR | AllowedLots | CostR | SpaceRatio |
|---|---|---|---:|---:|---:|---:|
| A | Candidate | None | 230 | 1 | 0.13R | 1.38 |
| B | Rejected | RiskTooHigh | 1120 | 0 | 0.11R | - |

最终候选列表：

```text
A
```

---

## 10. 结论

日内候选品种筛选的目标是：

> 找出账户当天能承受、成本合理、波动空间足够的少数品种。

核心输出不是交易信号，而是候选品种列表。
