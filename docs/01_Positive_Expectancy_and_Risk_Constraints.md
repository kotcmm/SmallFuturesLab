# 建立正期望与风险约束

## 1. 文档目的

本文档定义 SmallFuturesLab 的第一层约束：

1. 一套交易规则在数学上怎样才可能长期正期望；
2. 小资金账户如何从正期望条件反推风险边界；
3. 交易结构生成后，风险约束如何反推出目标价、手数和是否允许交易。

---

## 2. 核心思想

SmallFuturesLab 的第一步是：

> 先定义正期望条件，再定义风险边界。

核心顺序：

```text
正期望公式 → 账户风险边界 → 单笔交易验算 → 执行记录 → 统计修正
```

---

## 3. 可控制与不可控制

### 3.1 不可控制部分

| 项目 | 说明 |
|---|---|
| 单笔交易结果 | 下一笔交易赚钱还是亏钱不可提前确定。 |
| 短期胜率 | 连续几天、几十笔交易的胜率可能大幅波动。 |
| 连续亏损 | 正期望系统也可能出现连续亏损。 |
| 极端波动 | 突发波动可能导致滑点扩大。 |
| 成交价格 | 实际成交价可能偏离计划价。 |
| 市场状态 | 市场可能长时间不提供适合系统的机会。 |

### 3.2 可控制部分

| 项目 | 约束方向 |
|---|---|
| 是否交易 | 不满足条件就不交易。 |
| 单笔风险 | 每笔亏损限制在 `AccountR` 内。 |
| 仓位手数 | 根据账户风险上限和单笔结构风险计算。 |
| 计划目标空间 | 每笔交易必须满足最低计划盈利倍数。 |
| 保证金占用 | 账户总保证金占用不得超过最大比例。 |
| 单笔成本 | 每笔交易成本不得超过单笔成本上限。 |
| 每日亏损 | 达到上限后停止。 |
| 连续亏损 | 达到暂停条件后停止。 |
| 最大回撤 | 达到警戒线后降风险，达到停止线后停用。 |
| 执行记录 | 每笔交易必须记录结果。 |

---

## 4. R 的定义

系统统一使用 `R` 表示风险单位。

| 名称 | 含义 | 来源 |
|---|---|---|
| `AccountR` | 账户允许的单笔风险上限 | 账户权益和单笔风险比例计算出来 |
| `TradeR` | 某一笔交易的实际计划风险 | 入场价、止损价、合约乘数、手数和成本计算出来 |

账户层风险上限：

```text
AccountR = AccountEquity × RiskPercentPerTrade
```

单笔交易计划风险：

```text
TradeR = OneLotTradeR × AllowedLots
```

硬约束：

```text
TradeR <= AccountR
```

---

## 5. 正期望模型

基础公式：

```text
期望 = 胜率 × 平均盈利 - 失败率 × 平均亏损 - 交易成本
```

用 R 倍数表示：

```text
E = p × b - (1 - p) × a - c
```

其中：

| 符号 | 含义 | 来源 |
|---|---|---|
| `E` | 单笔交易期望，单位是 R | 模型计算 |
| `p` | 胜率 | 回测或实盘记录统计 |
| `b` | 平均盈利，单位是 R | 回测或实盘记录统计 |
| `a` | 平均亏损倍数 | 回测或实盘记录统计 |
| `c` | 单笔交易成本，单位是 R | 单笔交易计划计算 |

正期望条件：

```text
E > 0
```

说明：

```text
b 是统计结果，不是单笔计划参数。
```

---

## 6. 最低胜率要求

当平均亏损控制在 `1R` 时：

```text
E = p × b - (1 - p) - c
```

要让 `E > 0`，需要满足：

```text
p > (1 + c) / (b + 1)
```

示例：

| 平均盈利 b | 成本 c | 最低胜率 p |
|---:|---:|---:|
| 2.0R | 0.10R | 36.7% |
| 2.5R | 0.10R | 31.4% |
| 3.0R | 0.10R | 27.5% |
| 2.0R | 0.20R | 40.0% |
| 2.5R | 0.20R | 34.3% |
| 3.0R | 0.20R | 30.0% |

---

## 7. 单笔计划盈利约束

单笔计划盈利约束用于反推目标价。

```text
MinPlannedRewardR = 单笔最低计划盈利倍数
```

初始建议：

```text
MinPlannedRewardR = 2.5R
```

说明：

```text
MinPlannedRewardR 是账户风险约束配置。
b 是历史样本统计出来的平均盈利。
二者不能混为一个概念。
```

`03_Trading_Market_Structure.md` 只提供：

```text
Direction
EntryPrice
StopPrice
```

风险约束阶段根据 `MinPlannedRewardR` 反推 `TargetPrice`。

---

## 8. 目标价推导

先计算结构价格风险：

```text
SetupPriceRisk = |EntryPrice - StopPrice|
```

一手价格风险：

```text
OneLotPriceRisk = SetupPriceRisk × Multiplier
```

一手计划风险：

```text
OneLotTradeR = OneLotPriceRisk + EstimatedRoundTripCostPerLot
```

目标盈利金额：

```text
RequiredRewardAmount = OneLotTradeR × MinPlannedRewardR
```

目标价格距离：

```text
TargetPriceDistance = RequiredRewardAmount / Multiplier
```

做多目标价：

```text
TargetPrice = EntryPrice + TargetPriceDistance
```

做空目标价：

```text
TargetPrice = EntryPrice - TargetPriceDistance
```

说明：

```text
TargetPrice 由风险约束阶段生成，不由行情结构阶段生成。
```

---

## 9. 单笔成本约束

成本包括：

1. 手续费；
2. 滑点；
3. 买卖价差；
4. 冲击成本；
5. 无法按计划价格成交的损耗。

成本必须在每一笔交易计划中单独计算。

```text
c = 本笔交易总成本 / TradeR
```

约束规则：

```text
c <= PerTradeCostMaxR
```

初始建议：

```text
PerTradeCostMaxR = 0.20R
```

---

## 10. 保证金占用约束

保证金占用约束是账户层风险边界。

```text
MaxMarginUsageRatio = 账户最大允许保证金占用比例
```

初始建议：

```text
MaxMarginUsageRatio = 30%
```

账户最大允许保证金占用金额：

```text
MaxAllowedMargin = AccountEquity × MaxMarginUsageRatio
```

使用方式：

1. 在日内候选品种筛选阶段，用于判断品种一手保证金相对账户规模是否过重；
2. 在交易计划阶段，用于判断新开仓后账户总保证金占用是否超过上限。

---

## 11. 手数计算

风险约束阶段接收 `TradeSetup` 后，计算一手计划风险：

```text
OneLotTradeR = |EntryPrice - StopPrice| × Multiplier + EstimatedRoundTripCostPerLot
```

允许手数：

```text
AllowedLots = floor(AccountR / OneLotTradeR)
```

如果：

```text
AllowedLots < 1
```

则放弃该笔交易。

实际计划风险：

```text
TradeR = OneLotTradeR × AllowedLots
```

---

## 12. 每日风险约束

初始建议：

```text
DailyLossLimit = 2 × AccountR
```

触发条件：

1. 当天累计亏损达到 `DailyLossLimit`；
2. 当天连续亏损达到暂停条件；
3. 当天滑点明显异常；
4. 当天亏损明显偏离预期。

触发后：

```text
当天停止交易
```

---

## 13. 连续亏损约束

如果胜率为 `p`，失败率为：

```text
q = 1 - p
```

连续亏损 `n` 笔的概率是：

```text
P = q^n
```

例如胜率 35%，失败率 65%：

| 连续亏损笔数 | 概率 |
|---:|---:|
| 3 | 27.46% |
| 5 | 11.60% |
| 8 | 3.19% |
| 10 | 1.35% |

初始约束：

```text
账户必须能承受 10 × AccountR 的连续亏损
```

---

## 14. 最大回撤约束

初始建议：

```text
最大回撤警戒 = 10%
最大回撤停止 = 15%
```

触发后：

1. 停止实盘交易；
2. 复盘交易记录；
3. 检查胜率、盈亏比、成本是否偏离模型；
4. 必要时降低单笔风险；
5. 如果模型失效，停止该系统。

---

## 15. 系统最低可接受条件

SmallFuturesLab 的规则组合至少要满足以下条件：

| 项目 | 初始要求 |
|---|---:|
| 平均亏损 a | ≤ 1R |
| 平均盈利 b | ≥ 2R |
| 胜率 p | 高于最低胜率要求 |
| 单笔最低计划盈利倍数 MinPlannedRewardR | ≥ 2R |
| 单笔成本 c | ≤ PerTradeCostMaxR |
| TradeR | ≤ AccountR |
| 最大保证金占用比例 | ≤ MaxMarginUsageRatio |
| 每日最大亏损 | 2 × AccountR |
| 最大回撤警戒 | 10% |
| 最大回撤停止 | 15% |

---

## 16. 具体算例

账户配置：

| 参数 | 数值 |
|---|---:|
| AccountEquity | 50,000 |
| RiskPercentPerTrade | 0.5% |
| AccountR | 250 |
| MaxMarginUsageRatio | 30% |
| PerTradeCostMaxR | 0.20R |
| MinPlannedRewardR | 2.5R |

交易结构输入：

| 参数 | 数值 |
|---|---:|
| Direction | Long |
| EntryPrice | 3000 |
| StopPrice | 2980 |
| Multiplier | 10 |
| EstimatedRoundTripCostPerLot | 20 |
| OneLotMargin | 3000 |

### 16.1 单笔风险计算

```text
SetupPriceRisk = |3000 - 2980| = 20
OneLotPriceRisk = 20 × 10 = 200
OneLotTradeR = 200 + 20 = 220
AllowedLots = floor(250 / 220) = 1
TradeR = 220 × 1 = 220
```

结果：

```text
TradeR <= AccountR
220 <= 250
```

### 16.2 单笔成本计算

```text
c = 20 / 220
c = 0.09R
```

结果：

```text
0.09R <= 0.20R
```

### 16.3 目标价推导

```text
RequiredRewardAmount = 220 × 2.5
RequiredRewardAmount = 550
```

```text
TargetPriceDistance = 550 / 10
TargetPriceDistance = 55
```

```text
TargetPrice = 3000 + 55
TargetPrice = 3055
```

结果：

```text
目标价 = 3055
```

### 16.4 保证金检查

```text
MaxAllowedMargin = 50,000 × 30%
MaxAllowedMargin = 15,000
```

```text
OneLotMargin = 3,000
3,000 <= 15,000
```

结果：

```text
保证金占用合格
```

### 16.5 最终输出

| 输出项 | 结果 | 意义 |
|---|---:|---|
| AccountR | 250 | 账户单笔风险上限。 |
| OneLotTradeR | 220 | 一手计划风险。 |
| AllowedLots | 1 手 | 本笔允许手数。 |
| TradeR | 220 | 本笔实际计划风险。 |
| c | 0.09R | 本笔成本占比。 |
| MinPlannedRewardR | 2.5R | 本笔最低计划盈利倍数。 |
| TargetPrice | 3055 | 风险约束反推出的目标价。 |
| MaxAllowedMargin | 15,000 | 账户最大允许保证金占用金额。 |
| OneLotMargin | 3,000 | 本笔一手保证金。 |

结论：

```text
这笔交易计划通过风险约束。
```

---

## 17. 规则验收条件

任何交易计划都必须先通过这些问题：

1. `AllowedLots` 是否大于等于 1；
2. `TradeR` 是否小于等于 `AccountR`；
3. 本笔交易成本是否低于 `PerTradeCostMaxR`；
4. 本笔交易后账户保证金占用是否低于 `MaxMarginUsageRatio`；
5. 是否能根据 `MinPlannedRewardR` 推导出目标价；
6. 连续亏损是否在账户承受范围内；
7. 最大回撤是否在允许范围内；
8. 实际结果是否持续偏离模型。

---

## 18. 结论

SmallFuturesLab 的第一层规则是：

> 先定义正期望条件，再定义风险约束。

`03_Trading_Market_Structure.md` 负责生成 `TradeSetup`。

本文档负责把 `TradeSetup` 转换成：

```text
TargetPrice
AllowedLots
TradeR
是否允许交易
```

核心顺序：

```text
TradeSetup → 目标价推导 → 手数计算 → 成本检查 → 保证金检查 → 是否允许交易
```
