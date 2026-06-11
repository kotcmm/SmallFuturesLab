# SmallFuturesLab

SmallFuturesLab 研究小资金账户如何在期货市场中建立一套风险可控、规则清楚、可验证的交易系统。

核心顺序：

```text
先算账 → 再控风险 → 再选品种 → 再找结构 → 最后执行
```

---

## 1. 项目目标

项目目标是把小资金期货交易拆成一套可计算、可验证的流程：

1. 建立正期望与风险约束；
2. 开盘前筛选日内候选品种；
3. 在观察池中寻找交易行情结构；
4. 由风险约束生成交易计划；
5. 执行交易记录；
6. 用回测和实盘记录验证模型。

---

## 2. 当前核心文档

```text
docs/00_SmallFuturesLab_System_Framework.md
docs/01_Positive_Expectancy_and_Risk_Constraints.md
docs/01_Positive_Expectancy_and_Risk_Constraints/Actual_Average_Loss.md
docs/02_Daily_Candidate_Product_Selection.md
docs/03_Trading_Market_Structure.md
```

### 00_SmallFuturesLab_System_Framework.md

项目总纲，定义 SmallFuturesLab 的整体流程：

```text
正期望模型 + 风险约束 + 日内候选品种筛选 + 交易行情结构
```

### 01_Positive_Expectancy_and_Risk_Constraints.md

第一步详细文档，定义：

1. 正期望公式；
2. `AccountR` 和 `TradeR`；
3. `MinPlannedRewardR` 和目标价推导；
4. `PerTradeCostMaxR` 单笔成本上限；
5. `MaxMarginUsageRatio` 保证金占用约束；
6. 手数计算；
7. 每日亏损、盈利保护和交易次数约束；
8. 连续亏损和最大回撤约束；
9. 从 `TradeSetup` 到交易计划的完整算例。

### Actual_Average_Loss.md

解释“实际平均亏损”和 `a` 的含义。

### 02_Daily_Candidate_Product_Selection.md

第二步详细文档，定义开盘前如何筛选日内候选品种。

只做两个基础过滤：

1. 保证金过滤；
2. 最小交易颗粒度过滤。

输出结果：

```text
观察池
```

### 03_Trading_Market_Structure.md

第三步详细文档，定义如何在观察池中寻找交易行情结构。

第一版只使用：

```text
开盘区间突破
```

输出结果：

```text
TradeSetup
```

`TradeSetup` 只提供方向、入场价、止损价和结构失效条件。目标价、手数、`TradeR` 和是否允许交易，由 `01` 的风险约束阶段返回。

---

## 3. 正期望公式

核心公式：

```text
E = p × b - (1 - p) × a - c
```

其中：

| 符号 | 含义 |
|---|---|
| `E` | 单笔交易期望，单位是 R |
| `p` | 胜率，由回测或实盘记录统计出来 |
| `b` | 平均盈利，单位是 R，由回测或实盘记录统计出来 |
| `a` | 平均亏损倍数 |
| `c` | 单笔交易成本，单位是 R |

正期望条件：

```text
E > 0
```

---

## 4. 风险单位

SmallFuturesLab 区分两个风险单位：

| 名称 | 含义 |
|---|---|
| `AccountR` | 账户允许的单笔风险上限 |
| `TradeR` | 某一笔交易的实际计划风险 |

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

## 5. 日内候选品种筛选

筛选时间：

```text
开盘前
```

筛选内容：

```text
保证金过滤
最小交易颗粒度过滤
```

输出结果：

```text
观察池
```

候选品种筛选不是交易信号，只决定哪些品种有资格进入盘中观察。

---

## 6. 交易行情结构

观察池中的品种，盘中只寻找能够明确给出以下内容的结构：

```text
Direction
EntryPrice
StopPrice
InvalidReason
```

第一版结构：

```text
开盘区间突破
```

单品种日内规则：

```text
同一品种每天最多执行一次开盘区间突破交易。
```

---

## 7. 代码目录

```text
src/SmallFuturesLab.Core
src/SmallFuturesLab.TradingPlanet
src/SmallFuturesLab.Cli
```

当前代码实现应按 `docs/00`、`docs/01`、`docs/02` 和 `docs/03` 继续收敛。

---

## 8. 测试

默认测试命令：

```bash
dotnet test
```

新增或修改业务逻辑时，先补测试，再写实现。
