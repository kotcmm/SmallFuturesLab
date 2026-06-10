# 品种测算公式

文件：`docs/04_Product_Evaluation_Formula.md`

---

## 1. 文档目的

本文档定义 `ProductEvaluation` 中的全部测算公式和阈值判断逻辑。

它不判断行情方向，不判断买卖点，不判断策略是否有优势。

它只回答一个问题：

```text
在当前账户、当前品种和当前止损设想下，一手风险是否可承受？
```

核心原则：

```text
风险没有通过，行情没有资格被讨论。
```

---

## 2. 输入定义

### 2.1 品种输入（Product）

| 字段 | 含义 |
|---|---|
| `Exchange` | 交易所 |
| `Code` | 品种代码 |
| `Contract` | 合约代码 |
| `Name` | 品种名称 |
| `Price` | 当前价格 |
| `Multiplier` | 合约乘数 |
| `TickSize` | 最小变动价位 |
| `MarginRate` | 保证金比例 |
| `RoundTripFee` | 单手开平总手续费估计 |

### 2.2 账户风险配置（AccountRiskConfig）

| 字段 | 含义 | 当前默认值 |
|---|---|---:|
| `AccountEquity` | 当前账户权益 | — |
| `CautionRiskRate` | 单笔风险谨慎阈值 | 0.010 |
| `RejectRiskRate` | 单笔风险拒绝阈值 | 0.020 |
| `CautionMarginRate` | 保证金占用谨慎阈值 | 0.40 |
| `RejectMarginRate` | 保证金占用拒绝阈值 | 0.50 |
| `CautionCostRatio` | 成本占比谨慎阈值 | 0.20 |
| `RejectCostRatio` | 成本占比拒绝阈值 | 0.30 |

### 2.3 测算条件（FilterCondition）

| 字段 | 含义 | 当前默认值 |
|---|---|---:|
| `StopTicks` | 止损跳数 | — |
| `SlippageTicks` | 预估滑点跳数 | — |
| `Lots` | 手数 | 1 |

---

## 3. 公式节点

### 3.1 一跳金额

```text
TickValue = TickSize × Multiplier
```

### 3.2 一手保证金

```text
MarginPerLot = Price × Multiplier × MarginRate
```

### 3.3 保证金占权益比例

```text
MarginRateOfEquity = MarginPerLot × Lots / AccountEquity
```

### 3.4 止损风险金额

```text
StopRiskMoney = StopTicks × TickValue × Lots
```

### 3.5 滑点金额

```text
SlippageMoney = SlippageTicks × TickValue × Lots
```

### 3.6 成本金额

```text
CostMoney = RoundTripFee × Lots + SlippageMoney
```

### 3.7 总风险金额（1R）

```text
TotalRiskMoney = StopRiskMoney + CostMoney
```

### 3.8 风险占权益比例

```text
RiskRate = TotalRiskMoney / AccountEquity
```

### 3.9 成本占止损风险比例

```text
CostRatio = CostMoney / StopRiskMoney
```

---

## 4. 阈值判断

### 4.1 直接拒绝（Rejected）

满足任意一条，输出 `Rejected`：

```text
RiskRate > RejectRiskRate
MarginRateOfEquity > RejectMarginRate
CostRatio > RejectCostRatio
```

### 4.2 谨慎（Caution）

未触发拒绝，但满足任意一条，输出 `Caution`：

```text
RiskRate > CautionRiskRate
MarginRateOfEquity > CautionMarginRate
CostRatio > CautionCostRatio
```

### 4.3 允许（Allowed）

未触发拒绝或谨慎，输出 `Allowed`：

```text
RiskRate <= CautionRiskRate
MarginRateOfEquity <= CautionMarginRate
CostRatio <= CautionCostRatio
```

---

## 5. 完整数值推演

### 5.1 输入

品种：

```text
Price = 2500
Multiplier = 10
TickSize = 1
MarginRate = 0.10
RoundTripFee = 6
```

账户配置：

```text
AccountEquity = 20,000
CautionRiskRate = 0.01
RejectRiskRate = 0.02
CautionMarginRate = 0.40
RejectMarginRate = 0.50
CautionCostRatio = 0.20
RejectCostRatio = 0.30
```

测算条件：

```text
StopTicks = 12
SlippageTicks = 2
Lots = 1
```

### 5.2 计算过程

一跳金额：

```text
TickValue = 1 × 10 = 10 元
```

一手保证金：

```text
MarginPerLot = 2500 × 10 × 0.10 = 2,500 元
```

保证金占权益：

```text
MarginRateOfEquity = 2,500 × 1 / 20,000 = 0.125 = 12.5%
```

止损风险：

```text
StopRiskMoney = 12 × 10 × 1 = 120 元
```

滑点：

```text
SlippageMoney = 2 × 10 × 1 = 20 元
```

成本：

```text
CostMoney = 6 × 1 + 20 = 26 元
```

总风险：

```text
TotalRiskMoney = 120 + 26 = 146 元
```

风险占比：

```text
RiskRate = 146 / 20,000 = 0.0073 = 0.73%
```

成本占比：

```text
CostRatio = 26 / 120 = 0.2167 = 21.67%
```

### 5.3 阈值判断

```text
RiskRate = 0.73% <= CautionRiskRate (1.0%)     → 通过
MarginRateOfEquity = 12.5% <= CautionMarginRate (40%) → 通过
CostRatio = 21.67% > CautionCostRatio (20%)    → 触发谨慎
```

### 5.4 最终输出

```text
Status = Caution
```

原因：

```text
单笔风险 = 0.73%，账户可承受；
保证金占用 = 12.5%，账户可承受；
但手续费 + 滑点占止损风险 21.67%，超过 0.2R，属于谨慎研究。
```

---

## 6. 拒绝案例推演

假设止损更远：

```text
StopTicks = 40
SlippageTicks = 2
Lots = 1
```

其他条件不变。

止损风险：

```text
StopRiskMoney = 40 × 10 × 1 = 400 元
```

滑点：

```text
SlippageMoney = 2 × 10 × 1 = 20 元
```

成本：

```text
CostMoney = 6 + 20 = 26 元
```

总风险：

```text
TotalRiskMoney = 400 + 26 = 426 元
```

风险占比：

```text
RiskRate = 426 / 20,000 = 0.0213 = 2.13%
```

判断：

```text
RiskRate = 2.13% > RejectRiskRate (2.0%) → 拒绝
```

结论：

```text
Status = Rejected
原因：单笔风险超过账户 2%，不适合当前账户规模继续研究。
```

---

## 7. 与代码的对应关系

本文档中的公式和判断逻辑，由以下代码实现：

```text
src/SmallFuturesLab.Core/ProductEvaluation.cs
test/SmallFuturesLab.Core.Tests/ProductEvaluationTests.cs
```

如果文档和代码出现冲突，以当前代码实现为准，并先修正文档后再调整代码。

---

## 8. 当前不做什么

当前阶段不实现：

```text
连续亏损压力测试；
每日亏损限制检查；
交易次数限制；
硬性禁止条件检查（隔夜、加仓、无止损等）；
ATR 止损测算；
多止损场景批量展开；
CSV 批量输入输出。
```
