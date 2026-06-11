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

1. 剩余允许保证金是否够开一手；
2. 最小价格跳动压力是否过高；
3. 手续费压力是否过高。

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

输入参数分为三类：

```text
账户配置
账户状态
品种属性
```

---

### 4.1 账户配置

账户配置描述账户愿意承受的边界。

| 参数 | 含义 | 示例 |
|---|---|---:|
| MaxMarginUsageRatio | 账户最大允许保证金占用比例 | 30% |
| MaxTickValuePressureR | 最小价格跳动盈亏允许占 AccountR 的最大比例 | 0.10R |
| MaxFeePressureR | 单手开平手续费允许占 AccountR 的最大比例 | 0.20R |

---

### 4.2 账户状态

账户状态描述账户当前实际情况。

| 参数 | 含义 | 示例 |
|---|---|---:|
| AccountEquity | 账户权益 | 50,000 |
| UsedMargin | 当前已占用保证金 | 5,000 |
| AccountR | 账户单笔风险上限 | 250 |

账户状态不是配置项。

---

### 4.3 品种属性

品种属性描述合约本身的客观参数。

| 参数 | 含义 |
|---|---|
| Symbol | 品种代码 |
| ContractName | 合约名称 |
| PreviousSettlementPrice | 昨日结算价 |
| Multiplier | 合约乘数 |
| TickSize | 最小变动价位 |
| MarginRate | 品种保证金比例 |
| RoundTripFeePerLot | 单手开平合计手续费 |

---

## 5. 计算公式

### 5.1 剩余允许保证金预算

账户最大允许保证金占用金额：

```text
MaxAllowedMargin = AccountEquity × MaxMarginUsageRatio
```

账户剩余允许保证金预算：

```text
RemainingAllowedMargin = MaxAllowedMargin - UsedMargin
```

如果：

```text
RemainingAllowedMargin <= 0
```

说明账户已经没有新的保证金预算。

---

### 5.2 品种一手保证金

```text
OneLotMargin = PreviousSettlementPrice × Multiplier × MarginRate
```

判断：

```text
OneLotMargin <= RemainingAllowedMargin
```

含义：

> 检查账户剩余允许保证金预算是否足够开该品种一手。

---

### 5.3 最小价格跳动压力

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

### 5.4 手续费压力

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
OneLotMargin <= RemainingAllowedMargin
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
| InsufficientRemainingAllowedMargin | 剩余允许保证金预算不足以开一手 |
| TickValuePressureTooHigh | 最小价格跳动压力过高 |
| FeePressureTooHigh | 手续费压力过高 |

如果多个条件同时不满足，记录第一个拒绝原因即可。

拒绝顺序：

```text
InsufficientRemainingAllowedMargin → TickValuePressureTooHigh → FeePressureTooHigh
```

---

## 8. 输出字段

| 字段 | 含义 |
|---|---|
| Symbol | 品种代码 |
| Status | 筛选状态 |
| RejectReason | 拒绝原因 |
| MaxAllowedMargin | 账户最大允许保证金占用金额 |
| RemainingAllowedMargin | 账户剩余允许保证金预算 |
| OneLotMargin | 品种一手保证金 |
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

账户配置：

| 参数 | 数值 |
|---|---:|
| MaxMarginUsageRatio | 30% |
| MaxTickValuePressureR | 0.10R |
| MaxFeePressureR | 0.20R |

账户状态：

| 参数 | 数值 |
|---|---:|
| AccountEquity | 50,000 |
| UsedMargin | 5,000 |
| AccountR | 250 |

账户保证金预算：

```text
MaxAllowedMargin = 50,000 × 30% = 15,000
RemainingAllowedMargin = 15,000 - 5,000 = 10,000
```

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
TickValue = 1 × 10 = 10
TickValuePressureR = 10 / 250 = 0.04R
FeePressureR = 10 / 250 = 0.04R
```

判断：

```text
3000 <= 10000，通过
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
TickValue = 5 × 10 = 50
TickValuePressureR = 50 / 250 = 0.20R
FeePressureR = 20 / 250 = 0.08R
```

判断：

```text
6000 <= 10000，通过
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
TickValue = 1 × 10 = 10
TickValuePressureR = 10 / 250 = 0.04R
FeePressureR = 25 / 250 = 0.10R
```

判断：

```text
18000 <= 10000，不通过
0.04R <= 0.10R，通过
0.10R <= 0.20R，通过
```

结果：

```text
Status = Rejected
RejectReason = InsufficientRemainingAllowedMargin
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
TickValue = 1 × 10 = 10
TickValuePressureR = 10 / 250 = 0.04R
FeePressureR = 18 / 250 = 0.072R
```

判断：

```text
2500 <= 10000，通过
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

| Symbol | Status | RejectReason | RemainingAllowedMargin | OneLotMargin | TickValuePressureR | FeePressureR |
|---|---|---|---:|---:|---:|---:|
| A | Candidate | None | 10000 | 3000 | 0.04R | 0.04R |
| B | Rejected | TickValuePressureTooHigh | 10000 | 6000 | 0.20R | 0.08R |
| C | Rejected | InsufficientRemainingAllowedMargin | 10000 | 18000 | 0.04R | 0.10R |
| D | Candidate | None | 10000 | 2500 | 0.04R | 0.072R |

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

> 开盘前用账户配置和账户状态计算剩余允许保证金预算，再检查品种一手保证金、最小价格跳动压力和手续费压力是否适合小资金账户。

核心输出：

```text
观察池
```
