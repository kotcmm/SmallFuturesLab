# 日内候选品种筛选

## 1. 目的

日内候选品种筛选，是开盘前排除小资金账户无法承受的品种。

输出结果：

```text
观察池
```

---

## 2. 核心判断

本步骤只做两个过滤：

1. 保证金过滤；
2. 最小交易颗粒度过滤。

执行过滤前，需要先确认合约资料有效且合约处于可交易状态；具体有效性检查由代码实现。

含义：

```text
保证金过滤：账户允许的保证金边界，能不能覆盖该品种一手保证金。
最小交易颗粒度过滤：该品种一手最小价格跳动加手续费，会不会已经超过账户单笔风险上限。
```

通过两个过滤的品种，进入观察池。

---

## 3. 输入参数

### 3.1 账户参数

| 参数 | 含义 | 示例 |
|---|---|---:|
| AccountEquity | 账户权益 | 50,000 |
| AccountR | 账户单笔风险上限 | 250 |
| MaxMarginUsageRatio | 账户最大允许保证金占用比例 | 30% |

### 3.2 品种参数

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

## 4. 过滤一：保证金过滤

先计算该品种筛选用一手保证金：

```text
OneLotMargin = PreviousSettlementPrice × Multiplier × MarginRate
```

再计算账户允许的保证金占用金额：

```text
MaxAllowedMargin = AccountEquity × MaxMarginUsageRatio
```

判断：

```text
OneLotMargin <= MaxAllowedMargin
```

不满足则排除。

拒绝原因：

```text
MarginTooHigh
```

---

## 5. 过滤二：最小交易颗粒度过滤

先计算一跳价值：

```text
TickValue = TickSize × Multiplier
```

再计算该品种一手最小可能交易风险：

```text
MinimumOneLotTradeR = TickValue + RoundTripFeePerLot
```

判断：

```text
MinimumOneLotTradeR <= AccountR
```

含义：

> 如果价格只反向跳一跳，加上单手开平手续费，就已经超过账户单笔风险上限，这个品种不适合当前小资金账户。

说明：

```text
这是最小可交易性硬过滤，不是盈利质量过滤。
如果 MinimumOneLotTradeR > AccountR，则该品种在当前账户规模下不存在任何合规的一手交易结构。
```

不满足则排除。

拒绝原因：

```text
MinimumTradeRTooHigh
```

---

## 6. 筛选规则

一个品种必须同时满足：

```text
OneLotMargin <= MaxAllowedMargin
MinimumOneLotTradeR <= AccountR
```

全部满足：

```text
进入观察池
```

任意一项不满足：

```text
排除
```

拒绝顺序：

```text
MarginTooHigh → MinimumTradeRTooHigh
```

---

## 7. 输出字段

| 字段 | 含义 |
|---|---|
| Symbol | 品种代码 |
| Status | 筛选状态 |
| RejectReason | 拒绝原因 |
| OneLotMargin | 品种一手保证金 |
| MaxAllowedMargin | 账户允许的保证金占用金额 |
| TickValue | 一跳价值 |
| RoundTripFeePerLot | 单手开平合计手续费 |
| MinimumOneLotTradeR | 一手最小可能交易风险 |

### 7.1 成本字段传递说明

`RoundTripFeePerLot` 会传递给后续行情结构阶段。

第一版可以在后续风险约束阶段令：

```text
EstimatedRoundTripCostPerLot = RoundTripFeePerLot
```

后续如果需要更保守，可以在风险约束阶段或交易计划阶段额外叠加滑点、价差和冲击成本缓冲。

筛选状态：

| 状态 | 含义 |
|---|---|
| Rejected | 不进入观察池 |
| Candidate | 进入观察池 |

---

## 8. 完整算例

账户参数：

| 参数 | 数值 |
|---|---:|
| AccountEquity | 50,000 |
| AccountR | 250 |
| MaxMarginUsageRatio | 30% |

账户允许保证金占用金额：

```text
MaxAllowedMargin = 50,000 × 30%
MaxAllowedMargin = 15,000
```

---

### 8.1 品种 A

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
MinimumOneLotTradeR = 10 + 10 = 20
```

判断：

```text
3000 <= 15000，通过
20 <= 250，通过
```

结果：

```text
Status = Candidate
RejectReason = None
```

---

### 8.2 品种 B

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
MinimumOneLotTradeR = 50 + 20 = 70
```

判断：

```text
6000 <= 15000，通过
70 <= 250，通过
```

结果：

```text
Status = Candidate
RejectReason = None
```

---

### 8.3 品种 C

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
MinimumOneLotTradeR = 10 + 25 = 35
```

判断：

```text
18000 <= 15000，不通过
35 <= 250，通过
```

结果：

```text
Status = Rejected
RejectReason = MarginTooHigh
```

---

### 8.4 品种 D

输入：

| 参数 | 数值 |
|---|---:|
| PreviousSettlementPrice | 8000 |
| Multiplier | 20 |
| MarginRate | 10% |
| TickSize | 10 |
| RoundTripFeePerLot | 80 |

计算：

```text
OneLotMargin = 8000 × 20 × 10% = 16000
TickValue = 10 × 20 = 200
MinimumOneLotTradeR = 200 + 80 = 280
```

判断：

```text
16000 <= 15000，不通过
280 <= 250，不通过
```

结果：

```text
Status = Rejected
RejectReason = MarginTooHigh
```

说明：

```text
多个条件不满足时，记录第一个拒绝原因。
```

---

## 9. 最终结果示例

| Symbol | Status | RejectReason | OneLotMargin | MaxAllowedMargin | TickValue | RoundTripFeePerLot | MinimumOneLotTradeR |
|---|---|---|---:|---:|---:|---:|---:|
| A | Candidate | None | 3000 | 15000 | 10 | 10 | 20 |
| B | Candidate | None | 6000 | 15000 | 50 | 20 | 70 |
| C | Rejected | MarginTooHigh | 18000 | 15000 | 10 | 25 | 35 |
| D | Rejected | MarginTooHigh | 16000 | 15000 | 200 | 80 | 280 |

最终观察池：

```text
A
B
```

---

## 10. 结论

日内候选品种筛选只做两个基础过滤：

```text
一手保证金是否超过账户允许保证金边界。
一跳价值 + 单手开平手续费是否超过账户单笔风险上限。
```

核心输出：

```text
观察池
```
