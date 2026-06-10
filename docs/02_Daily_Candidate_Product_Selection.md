# 日内候选品种筛选

## 1. 目的

日内候选品种筛选，是开盘前用合约资料、费用资料、保证金资料和历史行情统计，筛出当天值得观察的少数候选品种。

输出结果：

```text
候选品种列表
```

每个候选品种只回答一个问题：

```text
今天是否值得进入盘中观察池
```

---

## 2. 核心边界

品种筛选只判断“品种是否值得观察”。

单笔交易风险由具体入场价和止损价决定，属于后续交易结构阶段。

因此，本文件不计算：

```text
TradeR
入场价
止损价
允许手数
单笔期望
```

本文件只计算品种层面的可观察性。

---

## 3. 筛选时间

筛选时间：

```text
开盘前
```

可使用的数据：

1. 合约静态参数；
2. 手续费；
3. 保证金；
4. 昨日结算价；
5. 昨日成交量；
6. 昨日持仓量；
7. 最近 N 日波动统计；
8. 最近 N 日成交量统计。

开盘前没有盘口、买一卖一、开盘成交量和当日波动数据。

---

## 4. 输入参数

输入参数分三类。

### 4.1 上游风险约束

这些参数来自 `01_Positive_Expectancy_and_Risk_Constraints.md` 的结果。

| 参数 | 含义 |
|---|---|
| AccountR | 账户单笔风险上限 |
| MaxMarginUsagePercent | 单品种最大保证金占用比例 |
| MaxFeePressureR | 最大费用压力 |
| MaxCandidates | 最多候选品种数 |

这里使用它们作为筛选约束，不在本文件重新定义账户配置。

---

### 4.2 合约与费用资料

| 参数 | 含义 |
|---|---|
| Symbol | 品种代码 |
| ContractName | 合约名称 |
| Multiplier | 合约乘数 |
| TickSize | 最小变动价位 |
| MarginRate | 保证金比例 |
| RoundTripFeePerLot | 单手开平合计手续费 |
| PreviousSettlementPrice | 昨日结算价 |

一跳价值：

```text
TickValue = TickSize × Multiplier
```

一手名义价值：

```text
OneLotNotional = PreviousSettlementPrice × Multiplier
```

一手保证金：

```text
OneLotMargin = OneLotNotional × MarginRate
```

---

### 4.3 历史统计资料

| 参数 | 含义 |
|---|---|
| AverageDailyVolumeN | 最近 N 日平均成交量 |
| AverageOpenInterestN | 最近 N 日平均持仓量 |
| AverageTrueRangeN | 最近 N 日平均真实波幅 |
| MedianIntradayRangeN | 最近 N 日日内振幅中位数 |
| LimitUpDownRatio | 近期涨跌停或接近涨跌停的比例 |

这些数据来自历史行情统计，不来自当天开盘后的盘口。

---

## 5. 推算过程

### 5.1 保证金压力

```text
MarginUsagePercent = OneLotMargin / AccountEquity
```

筛选规则：

```text
MarginUsagePercent <= MaxMarginUsagePercent
```

说明：

> 保证金过高的品种会挤压容错空间。

---

### 5.2 最小价格颗粒度

```text
TickValue = TickSize × Multiplier
```

筛选含义：

> TickValue 越大，最小价格跳动对小资金越不友好。

本步骤不把 TickValue 等同于单笔风险。

---

### 5.3 费用压力

开盘前没有具体入场止损，所以不能计算真实成本占 `TradeR` 的比例。

这里用 `AccountR` 做保守压力测算：

```text
FeePressureR = RoundTripFeePerLot / AccountR
```

筛选规则：

```text
FeePressureR <= MaxFeePressureR
```

说明：

> 如果一手开平手续费相对 AccountR 已经过高，后续交易结构很难形成正期望。

---

### 5.4 历史流动性

成交量过滤：

```text
AverageDailyVolumeN >= MinAverageDailyVolumeN
```

持仓量过滤：

```text
AverageOpenInterestN >= MinAverageOpenInterestN
```

说明：

> 流动性不足会放大滑点，降低执行稳定性。

---

### 5.5 历史波动空间

用历史波动衡量该品种是否有足够日内空间。

```text
IntradaySpaceValue = MedianIntradayRangeN × Multiplier
```

波动空间压力：

```text
SpaceToAccountR = IntradaySpaceValue / AccountR
```

筛选规则：

```text
SpaceToAccountR >= MinSpaceToAccountR
```

说明：

> 如果历史日内波动空间太小，即使能下单，也可能无法覆盖风险和成本。

---

### 5.6 异常波动排除

近期涨跌停或接近涨跌停比例过高时，品种稳定性较差。

```text
LimitUpDownRatio <= MaxLimitUpDownRatio
```

说明：

> 小资金不适合优先观察容易被极端波动影响的品种。

---

## 6. 筛选状态

每个品种输出一个状态。

| 状态 | 含义 |
|---|---|
| Rejected | 不满足硬约束 |
| Candidate | 通过硬约束，可以进入观察池 |
| Preferred | 通过硬约束，并且排序靠前 |

拒绝原因使用明确字段记录。

| 原因 | 含义 |
|---|---|
| MarginTooHigh | 保证金压力过高 |
| TickValueTooLarge | 一跳价值过大 |
| FeePressureTooHigh | 费用压力过高 |
| LiquidityTooLow | 历史流动性不足 |
| SpaceTooSmall | 历史波动空间不足 |
| ExtremeMoveTooFrequent | 极端波动过多 |

---

## 7. 候选排序

通过硬约束后，再排序。

排序优先级：

1. `SpaceToAccountR` 越高越靠前；
2. `FeePressureR` 越低越靠前；
3. `MarginUsagePercent` 越低越靠前；
4. `AverageDailyVolumeN` 越高越靠前；
5. `AverageOpenInterestN` 越高越靠前。

最终只保留：

```text
MaxCandidates
```

默认：

```text
MaxCandidates = 3
```

---

## 8. 输出字段

日内候选品种筛选结果至少包含以下字段。

| 字段 | 含义 |
|---|---|
| Symbol | 品种代码 |
| Status | 筛选状态 |
| RejectReason | 拒绝原因 |
| AccountR | 账户单笔风险上限 |
| PreviousSettlementPrice | 昨日结算价 |
| TickValue | 一跳价值 |
| OneLotMargin | 一手保证金 |
| MarginUsagePercent | 保证金压力 |
| RoundTripFeePerLot | 单手开平合计手续费 |
| FeePressureR | 费用压力 |
| AverageDailyVolumeN | 最近 N 日平均成交量 |
| AverageOpenInterestN | 最近 N 日平均持仓量 |
| MedianIntradayRangeN | 最近 N 日日内振幅中位数 |
| SpaceToAccountR | 历史波动空间相对 AccountR 的倍数 |
| LimitUpDownRatio | 近期极端波动比例 |

---

## 9. 完整算例

上游风险约束：

| 参数 | 数值 |
|---|---:|
| AccountR | 250 |
| MaxMarginUsagePercent | 25% |
| MaxFeePressureR | 0.20R |
| MinSpaceToAccountR | 2.5 |
| MaxLimitUpDownRatio | 10% |
| MaxCandidates | 3 |

历史过滤阈值：

| 参数 | 数值 |
|---|---:|
| MinAverageDailyVolumeN | 100,000 |
| MinAverageOpenInterestN | 50,000 |

候选品种 A：

| 参数 | 数值 |
|---|---:|
| PreviousSettlementPrice | 3000 |
| Multiplier | 10 |
| TickSize | 1 |
| MarginRate | 10% |
| RoundTripFeePerLot | 10 |
| AverageDailyVolumeN | 300,000 |
| AverageOpenInterestN | 180,000 |
| MedianIntradayRangeN | 80 |
| LimitUpDownRatio | 2% |

推算：

```text
TickValue = 1 × 10 = 10
OneLotMargin = 3000 × 10 × 10% = 3000
MarginUsagePercent = 3000 / 50000 = 6%
FeePressureR = 10 / 250 = 0.04R
IntradaySpaceValue = 80 × 10 = 800
SpaceToAccountR = 800 / 250 = 3.2
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
| PreviousSettlementPrice | 5000 |
| Multiplier | 10 |
| TickSize | 5 |
| MarginRate | 12% |
| RoundTripFeePerLot | 20 |
| AverageDailyVolumeN | 260,000 |
| AverageOpenInterestN | 120,000 |
| MedianIntradayRangeN | 70 |
| LimitUpDownRatio | 1% |

推算：

```text
TickValue = 5 × 10 = 50
OneLotMargin = 5000 × 10 × 12% = 6000
MarginUsagePercent = 6000 / 50000 = 12%
FeePressureR = 20 / 250 = 0.08R
IntradaySpaceValue = 70 × 10 = 700
SpaceToAccountR = 700 / 250 = 2.8
```

输出：

```text
Status = Candidate
RejectReason = None
```

---

候选品种 C：

| 参数 | 数值 |
|---|---:|
| PreviousSettlementPrice | 12000 |
| Multiplier | 10 |
| TickSize | 5 |
| MarginRate | 15% |
| RoundTripFeePerLot | 60 |
| AverageDailyVolumeN | 80,000 |
| AverageOpenInterestN | 40,000 |
| MedianIntradayRangeN | 40 |
| LimitUpDownRatio | 4% |

推算：

```text
TickValue = 5 × 10 = 50
OneLotMargin = 12000 × 10 × 15% = 18000
MarginUsagePercent = 18000 / 50000 = 36%
```

输出：

```text
Status = Rejected
RejectReason = MarginTooHigh
```

---

## 10. 最终结果示例

| Symbol | Status | RejectReason | MarginUsagePercent | FeePressureR | SpaceToAccountR |
|---|---|---|---:|---:|---:|
| A | Preferred | None | 6% | 0.04R | 3.2 |
| B | Candidate | None | 12% | 0.08R | 2.8 |
| C | Rejected | MarginTooHigh | 36% | 0.24R | 1.6 |

最终候选列表：

```text
A
B
```

---

## 11. 和后续交易结构的关系

日内候选品种筛选只产生观察池。

进入观察池后，具体交易结构再计算：

```text
入场价
止损价
TradeR
允许手数
成本占 TradeR 的比例
是否满足正期望约束
```

因此，候选品种不是交易信号。

---

## 12. 结论

日内候选品种筛选的目标是：

> 开盘前筛出账户承受能力内、费用压力低、历史流动性足够、历史波动空间足够的少数品种。

核心输出：

```text
观察池
```
