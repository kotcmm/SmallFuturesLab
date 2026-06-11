# 日内候选品种筛选

## 1. 目的

日内候选品种筛选，是开盘前排除不适合小资金账户观察的品种。

输出结果：

```text
观察池
```

本步骤只回答一个问题：

```text
这个品种今天是否有资格进入观察池
```

---

## 2. 核心原则

品种筛选不是判断行情，也不是寻找入场机会。

它只判断品种本身是否适合小资金账户日内观察。

筛选依据只有三类：

1. 保证金占用压力；
2. 最小价格跳动压力；
3. 手续费压力。

通过基础条件的品种全部进入观察池。

---

## 3. 核心边界

品种本身没有入场价和止损价。

所以本步骤不计算：

```text
TradeR
允许手数
单笔期望
入场价
止损价
盘中结构
最多交易品种数
排序分数
```

这些内容放到后续交易结构和风险控制阶段处理。

---

## 4. 输入参数

输入参数分为账户侧参数和品种侧参数。

### 4.1 账户侧参数

账户侧参数描述账户当前能承受的边界。

| 参数 | 含义 | 示例 |
|---|---|---:|
| AccountR | 账户单笔风险上限 | 250 |
| AvailableMargin | 账户当前可用保证金 | 40,000 |
| MaxMarginUsageRatio | 单个品种最小交易单位允许占用可用保证金的最大比例 | 25% |
| MaxTickValuePressureR | 最小价格跳动盈亏允许占 AccountR 的最大比例 | 0.10R |
| MaxFeePressureR | 单手开平手续费允许占 AccountR 的最大比例 | 0.20R |

这些参数是账户约束，不是品种配置。

---

### 4.2 品种侧参数

品种侧参数描述合约本身的客观属性。

| 参数 | 含义 |
|---|---|
| Symbol | 品种代码 |
| ContractName | 合约名称 |
| PreviousSettlementPrice | 昨日结算价 |
| Multiplier | 合约乘数 |
| TickSize | 最小变动价位 |
| MarginRate | 保证金比例 |
| RoundTripFeePerLot | 单手开平合计手续费 |

---

## 5. 计算公式

### 5.1 保证金占用压力

先计算品种一手保证金：

```text
OneLotMargin = PreviousSettlementPrice × Multiplier × MarginRate
```

再计算这“一手”会占用账户当前可用保证金的比例：

```text
MarginUsageRatio = OneLotMargin / AvailableMargin
```

判断：

```text
MarginUsageRatio <= MaxMarginUsageRatio
```

含义：

> 检查品种最小交易单位是否会占用过多账户可用保证金。

---

### 5.2 最小价格跳动压力

先计算一跳价值：

```text
TickValue = TickSize × Multiplier
```

再计算一跳盈亏相对账户单笔风险上限的压力：

```text
TickValuePressureR = TickValue / AccountR
```

判断：

```text
TickValuePressureR <= MaxTickValuePressureR
```

含义：

> 检查价格最小跳动带来的盈亏变化，是否相对账户风险过粗。

---

### 5.3 手续费压力

```text
FeePressureR = RoundTripFeePerLot / AccountR
```

判断：

```text
FeePressureR <= MaxFeePressureR
```

含义：

> 检查单手开平手续费是否会过度侵蚀账户单笔风险预算。

---

## 6. 筛选规则

一个品种必须同时满足：

```text
MarginUsageRatio <= MaxMarginUsageRatio
TickValuePressureR <= MaxTickValuePressureR
FeePressureR <= MaxFeePressureR
```

全部满足：

```text
进入观察池
```

任意一项不满足：

```text
排除
```

---

## 7. 拒绝原因

| 原因 | 含义 |
|---|---|
| MarginUsageTooHigh | 保证金占用压力过高 |
| TickValuePressureTooHigh | 最小价格跳动压力过高 |
| FeePressureTooHigh | 手续费压力过高 |

如果多个条件同时不满足，记录第一个拒绝原因即可。

拒绝顺序：

```text
MarginUsageTooHigh → TickValuePressureTooHigh → FeePressureTooHigh
```

---

## 8. 输出字段

| 字段 | 含义 |
|---|---|
| Symbol | 品种代码 |
| Status | 筛选状态 |
| RejectReason | 拒绝原因 |
| OneLotMargin | 品种一手保证金 |
| MarginUsageRatio | 一手保证金占账户可用保证金的比例 |
| TickValue | 一跳价值 |
| TickValuePressureR | 一跳价值占 AccountR 的比例 |
| RoundTripFeePerLot | 单手开平合计手续费 |
| FeePressureR | 手续费占 AccountR 的比例 |

筛选状态：

| 状态 | 含义 |
|---|---|
| Rejected | 不进入观察池 |
| Candidate | 进入观察池 |

---

## 9. 完整算例

账户侧参数：

| 参数 | 数值 |
|---|---:|
| AccountR | 250 |
| AvailableMargin | 40,000 |
| MaxMarginUsageRatio | 25% |
| MaxTickValuePressureR | 0.10R |
| MaxFeePressureR | 0.20R |

---

### 9.1 品种 A

输入：

| 参数 | 数值 |
|---|---:|
| PreviousSettlementPrice | 3000 |
| Multiplier | 10 |
| MarginRate | 10% |
| TickSize | 1 |
| RoundTripFeePerLot | 10 |

计算：

```text
OneLotMargin = 3000 × 10 × 10% = 3000
MarginUsageRatio = 3000 / 40000 = 7.5%
TickValue = 1 × 10 = 10
TickValuePressureR = 10 / 250 = 0.04R
FeePressureR = 10 / 250 = 0.04R
```

判断：

```text
7.5% <= 25%，通过
0.04R <= 0.10R，通过
0.04R <= 0.20R，通过
```

结果：

```text
Status = Candidate
RejectReason = None
```

---

### 9.2 品种 B

输入：

| 参数 | 数值 |
|---|---:|
| PreviousSettlementPrice | 5000 |
| Multiplier | 10 |
| MarginRate | 12% |
| TickSize | 5 |
| RoundTripFeePerLot | 20 |

计算：

```text
OneLotMargin = 5000 × 10 × 12% = 6000
MarginUsageRatio = 6000 / 40000 = 15%
TickValue = 5 × 10 = 50
TickValuePressureR = 50 / 250 = 0.20R
FeePressureR = 20 / 250 = 0.08R
```

判断：

```text
15% <= 25%，通过
0.20R <= 0.10R，不通过
0.08R <= 0.20R，通过
```

结果：

```text
Status = Rejected
RejectReason = TickValuePressureTooHigh
```

---

### 9.3 品种 C

输入：

| 参数 | 数值 |
|---|---:|
| PreviousSettlementPrice | 12000 |
| Multiplier | 10 |
| MarginRate | 15% |
| TickSize | 1 |
| RoundTripFeePerLot | 25 |

计算：

```text
OneLotMargin = 12000 × 10 × 15% = 18000
MarginUsageRatio = 18000 / 40000 = 45%
TickValue = 1 × 10 = 10
TickValuePressureR = 10 / 250 = 0.04R
FeePressureR = 25 / 250 = 0.10R
```

判断：

```text
45% <= 25%，不通过
0.04R <= 0.10R，通过
0.10R <= 0.20R，通过
```

结果：

```text
Status = Rejected
RejectReason = MarginUsageTooHigh
```

---

### 9.4 品种 D

输入：

| 参数 | 数值 |
|---|---:|
| PreviousSettlementPrice | 2500 |
| Multiplier | 10 |
| MarginRate | 10% |
| TickSize | 1 |
| RoundTripFeePerLot | 18 |

计算：

```text
OneLotMargin = 2500 × 10 × 10% = 2500
MarginUsageRatio = 2500 / 40000 = 6.25%
TickValue = 1 × 10 = 10
TickValuePressureR = 10 / 250 = 0.04R
FeePressureR = 18 / 250 = 0.072R
```

判断：

```text
6.25% <= 25%，通过
0.04R <= 0.10R，通过
0.072R <= 0.20R，通过
```

结果：

```text
Status = Candidate
RejectReason = None
```

---

## 10. 最终结果示例

| Symbol | Status | RejectReason | OneLotMargin | MarginUsageRatio | TickValuePressureR | FeePressureR |
|---|---|---|---:|---:|---:|---:|
| A | Candidate | None | 3000 | 7.5% | 0.04R | 0.04R |
| B | Rejected | TickValuePressureTooHigh | 6000 | 15% | 0.20R | 0.08R |
| C | Rejected | MarginUsageTooHigh | 18000 | 45% | 0.04R | 0.10R |
| D | Candidate | None | 2500 | 6.25% | 0.04R | 0.072R |

最终观察池：

```text
A
D
```

说明：

```text
所有通过基础条件的品种都进入观察池。
```

---

## 11. 结论

日内候选品种筛选的目标是：

> 开盘前用账户侧约束检查品种侧属性，排除保证金占用压力、最小价格跳动压力和手续费压力不适合小资金账户的品种。

核心输出：

```text
观察池
```
