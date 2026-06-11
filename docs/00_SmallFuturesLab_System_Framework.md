# SmallFuturesLab 系统框架 v0.1

## 1. 项目目标

SmallFuturesLab 的目标是研究：

> 小资金账户如何在期货市场中，建立一套风险可控、规则清楚、长期有机会获得正期望的交易系统。

核心不是预测行情，而是建立可计算、可验证、可执行的交易流程。

---

## 2. 总体思路

系统分三步：

1. 建立正期望与风险约束；
2. 开盘前筛选日内候选品种；
3. 在观察池中寻找交易行情结构。

第三步生成的交易结构，必须再交给风险约束阶段验算。

---

## 3. 第一步：建立正期望与风险约束

第一步回答：

```text
这套规则在数学上怎样才可能长期期望为正？
小资金账户怎样控制亏损、成本、保证金、交易次数和回撤？
```

核心公式：

```text
E = p × b - (1 - p) × a - c
```

其中：

| 符号 | 含义 |
|---|---|
| E | 单笔交易期望 |
| p | 胜率 |
| b | 平均盈利倍数，来自统计结果 |
| a | 平均亏损倍数，来自统计结果 |
| c | 单笔交易成本，按 R 计算 |

正期望条件：

```text
E > 0
```

风险约束主要包括：

1. `AccountR`：账户单笔风险上限；
2. `TradeR`：某一笔交易的实际计划风险；
3. `MinPlannedRewardR`：单笔最低计划盈利倍数；
4. `PerTradeCostMaxR`：单笔成本上限；
5. `MaxMarginUsageRatio`：账户最大保证金占用比例；
6. `DailyLossLimit`：每日亏损上限；
7. `DailyProfitLockR`：每日盈利保护线；
8. `MaxDailyTrades`：每日最多交易次数；
9. 最大回撤警戒与停止线。

风险约束阶段负责把 `TradeSetup` 转换成：

```text
TargetPrice
AllowedLots
TradeR
是否允许交易
```

---

## 4. 第二步：日内候选品种筛选

日内候选品种筛选在开盘前完成。

目标是排除小资金账户无法承受的品种。

本步骤只做两个基础过滤：

1. 保证金过滤；
2. 最小交易颗粒度过滤。

保证金过滤：

```text
OneLotMargin = PreviousSettlementPrice × Multiplier × MarginRate
MaxAllowedMargin = AccountEquity × MaxMarginUsageRatio
OneLotMargin <= MaxAllowedMargin
```

最小交易颗粒度过滤：

```text
TickValue = TickSize × Multiplier
MinimumOneLotTradeR = TickValue + RoundTripFeePerLot
MinimumOneLotTradeR <= AccountR
```

输出结果：

```text
观察池
```

候选品种筛选不是交易信号。

---

## 5. 第三步：寻找交易行情结构

品种进入观察池后，盘中寻找具体交易行情结构。

交易行情结构只回答：

1. 做多还是做空；
2. 在哪里进场；
3. 在哪里证明自己错了；
4. 结构失效条件是什么。

第一版只使用：

```text
开盘区间突破
```

开盘区间突破输出：

```text
TradeSetup
```

`TradeSetup` 至少包含：

```text
Symbol
Direction
EntryPrice
StopPrice
InvalidReason
```

行情结构阶段不决定：

```text
TargetPrice
AllowedLots
TradeR
是否允许下单
```

这些由风险约束阶段返回。

单品种日内规则：

```text
同一品种每天最多执行一次开盘区间突破交易。
```

无论止盈还是止损，退出后该品种当天不再生成新的开盘区间突破 `TradeSetup`。

---

## 6. 系统定义

SmallFuturesLab 定义为：

> 正期望模型 + 风险约束 + 日内候选品种筛选 + 交易行情结构。

更简单地说：

> 先算清楚怎样才可能赚钱，再控制风险，再筛选当天能观察的品种，最后寻找可验算的入场结构。

---

## 7. 总体执行流程

SmallFuturesLab 的完整流程是：

1. 建立正期望模型；
2. 定义账户风险约束；
3. 开盘前筛选日内候选品种；
4. 在观察池里寻找交易行情结构；
5. 生成 `TradeSetup`；
6. 风险约束阶段生成 `TargetPrice`、`AllowedLots`、`TradeR`；
7. 判断是否允许交易；
8. 执行入场、止损、止盈或收盘退出；
9. 记录结果；
10. 用回测和实盘记录验证系统是否仍然满足正期望条件。

这套流程的核心顺序是：

> 先算账，再控风险，再选品种，再找结构，最后执行。
