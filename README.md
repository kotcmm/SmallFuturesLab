# SmallFuturesLab

SmallFuturesLab 研究小资金账户如何在期货市场中建立一套风险可控、规则清楚、可验证的交易系统。

核心顺序：

```text
先算账 → 再控风险 → 再选品种 → 最后交易
```

---

## 1. 项目目标

项目目标是把小资金期货交易拆成一套可计算、可验证的流程：

1. 建立正期望与风险约束；
2. 开盘前筛选日内候选品种；
3. 在候选品种中寻找交易结构；
4. 执行交易记录；
5. 用回测和实盘记录验证模型。

---

## 2. 当前核心文档

```text
docs/00_SmallFuturesLab_System_Framework.md
docs/01_Positive_Expectancy_and_Risk_Constraints.md
docs/01_Positive_Expectancy_and_Risk_Constraints/Actual_Average_Loss.md
docs/02_Daily_Candidate_Product_Selection.md
```

### 00_SmallFuturesLab_System_Framework.md

项目总纲，定义 SmallFuturesLab 的整体流程：

```text
正期望模型 + 小资金风险管理 + 日内候选品种筛选 + 日内交易结构
```

### 01_Positive_Expectancy_and_Risk_Constraints.md

第一步详细文档，定义：

1. 可控制与不可控制；
2. `AccountR` 和 `TradeR`；
3. 正期望公式；
4. 成本约束；
5. 单笔风险约束；
6. 每日风险约束；
7. 连续亏损约束；
8. 最大回撤约束；
9. 从输入参数到输出约束的完整算例。

### Actual_Average_Loss.md

解释“实际平均亏损”和 `a` 的含义。

### 02_Daily_Candidate_Product_Selection.md

第二步详细文档，定义开盘前如何筛选日内候选品种：

1. 上游风险约束；
2. 合约与费用资料；
3. 历史统计资料；
4. 一手保证金测算；
5. 费用压力测算；
6. 历史流动性筛选；
7. 历史波动空间筛选；
8. 候选排序和观察池输出。

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
| `b` | 平均盈利，单位是 R |
| `a` | 平均亏损倍数 |
| `c` | 交易成本，单位是 R |

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
| `TradeR` | 某一笔交易的计划风险 |

账户层风险上限：

```text
AccountR = 账户权益 × 单笔风险比例
```

单笔交易计划风险：

```text
TradeR = |入场价 - 止损价| × 合约乘数 × 手数 + 预估交易成本
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

筛选顺序：

```text
保证金 → 费用压力 → 历史流动性 → 历史波动空间 → 极端波动 → 候选排序
```

输出结果：

```text
观察池
```

进入观察池后，再由交易结构计算入场价、止损价、`TradeR` 和允许手数。

---

## 6. 代码目录

```text
src/SmallFuturesLab.Core
src/SmallFuturesLab.TradingPlanet
src/SmallFuturesLab.Cli
```

当前代码以小资金品种测算为基础，后续实现按 `docs/00`、`docs/01` 和 `docs/02` 继续收敛。

---

## 7. 测试

默认测试命令：

```bash
dotnet test
```

新增或修改业务逻辑时，先补测试，再写实现。
