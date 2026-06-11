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

1. 一手保证金；
2. 一跳价值；
3. 单手开平手续费。

通过基础条件的品种全部进入观察池。

排序只用于展示优先级，不用于强制截断。

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
```

这些内容放到后续交易结构和风险控制阶段处理。

---

## 4. 输入参数

### 4.1 筛选阈值

这些阈值来自小资金账户的承受能力。

| 参数 | 含义 | 示例 |
|---|---|---:|
| MaxOneLotMargin | 一手保证金上限 | 12,000 |
| MaxTickValue | 一跳价值上限 | 20 |
| MaxRoundTripFeePerLot | 单手开平手续费上限 | 30 |

---

### 4.2 品种参数

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

### 5.1 一手保证金

```text
OneLotMargin = PreviousSettlementPrice × Multiplier × MarginRate
```

判断：

```text
OneLotMargin <= MaxOneLotMargin
```

含义：

> 过滤掉一手资金门槛太高的品种。

---

### 5.2 一跳价值

```text
TickValue = TickSize × Multiplier
```

判断：

```text
TickValue <= MaxTickValue
```

含义：

> 过滤掉价格颗粒度太粗的品种。

---

### 5.3 单手开平手续费

```text
RoundTripFeePerLot <= MaxRoundTripFeePerLot
```

含义：

> 过滤掉交易摩擦太高的品种。

---

## 6. 筛选规则

一个品种必须同时满足：

```text
OneLotMargin <= MaxOneLotMargin
TickValue <= MaxTickValue
RoundTripFeePerLot <= MaxRoundTripFeePerLot
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
| MarginTooHigh | 一手保证金过高 |
| TickValueTooLarge | 一跳价值过大 |
| FeeTooHigh | 单手开平手续费过高 |

如果多个条件同时不满足，记录第一个拒绝原因即可。

拒绝顺序：

```text
MarginTooHigh → TickValueTooLarge → FeeTooHigh
```

---

## 8. 排序

排序只用于展示观察优先级。

排序不用于排除品种。

排序原则：

1. 一手保证金越低越靠前；
2. 一跳价值越小越靠前；
3. 单手开平手续费越低越靠前。

排序公式：

```text
Score =
  OneLotMargin / MaxOneLotMargin
+ TickValue / MaxTickValue
+ RoundTripFeePerLot / MaxRoundTripFeePerLot
```

`Score` 越小，观察优先级越高。

---

## 9. 输出字段

| 字段 | 含义 |
|---|---|
| Symbol | 品种代码 |
| Status | 筛选状态 |
| RejectReason | 拒绝原因 |
| OneLotMargin | 一手保证金 |
| TickValue | 一跳价值 |
| RoundTripFeePerLot | 单手开平合计手续费 |
| Score | 排序分数 |

筛选状态：

| 状态 | 含义 |
|---|---|
| Rejected | 不进入观察池 |
| Candidate | 进入观察池 |

---

## 10. 完整算例

筛选阈值：

| 参数 | 数值 |
|---|---:|
| MaxOneLotMargin | 12,000 |
| MaxTickValue | 20 |
| MaxRoundTripFeePerLot | 30 |

---

### 10.1 品种 A

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
RoundTripFeePerLot = 10
```

判断：

```text
3000 <= 12000，通过
10 <= 20，通过
10 <= 30，通过
```

结果：

```text
Status = Candidate
RejectReason = None
```

---

### 10.2 品种 B

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
RoundTripFeePerLot = 20
```

判断：

```text
6000 <= 12000，通过
50 <= 20，不通过
20 <= 30，通过
```

结果：

```text
Status = Rejected
RejectReason = TickValueTooLarge
```

---

### 10.3 品种 C

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
RoundTripFeePerLot = 25
```

判断：

```text
18000 <= 12000，不通过
10 <= 20，通过
25 <= 30，通过
```

结果：

```text
Status = Rejected
RejectReason = MarginTooHigh
```

---

### 10.4 品种 D

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
RoundTripFeePerLot = 18
```

判断：

```text
2500 <= 12000，通过
10 <= 20，通过
18 <= 30，通过
```

结果：

```text
Status = Candidate
RejectReason = None
```

---

## 11. 最终结果示例

| Symbol | Status | RejectReason | OneLotMargin | TickValue | RoundTripFeePerLot | Score |
|---|---|---|---:|---:|---:|---:|
| D | Candidate | None | 2500 | 10 | 18 | 1.31 |
| A | Candidate | None | 3000 | 10 | 10 | 1.08 |
| B | Rejected | TickValueTooLarge | 6000 | 50 | 20 | - |
| C | Rejected | MarginTooHigh | 18000 | 10 | 25 | - |

最终观察池：

```text
A
D
```

说明：

```text
所有通过基础条件的品种都进入观察池。
排序只表示观察优先级，不会因为数量多而截断。
```

---

## 12. 结论

日内候选品种筛选的目标是：

> 开盘前用一手保证金、一跳价值和单手开平手续费，排除不适合小资金账户观察的品种。

核心输出：

```text
观察池
```
