# 寻找交易行情结构

## 1. 目的

交易行情结构，是把观察池里的品种转换成可被风险约束验算的交易设想。

本步骤输出：

```text
TradeSetup
```

一个合格的 `TradeSetup` 至少要给出：

1. 交易方向；
2. 入场价；
3. 止损价；
4. 结构失效条件。

---

## 2. 核心原则

交易结构不是预测行情。

交易结构只回答三个问题：

```text
在哪里进场
在哪里证明自己错了
价格距离是多少
```

如果一个结构不能给出明确止损价，就不是合格结构。

---

## 3. 本阶段先使用的结构

第一版只使用一个结构：

```text
开盘区间突破
```

原因：

1. 边界清楚；
2. 入场价清楚；
3. 止损价清楚；
4. 适合日内；
5. 容易回测。

---

## 4. 输入参数

### 4.1 来自观察池的品种参数

| 参数 | 含义 |
|---|---|
| Symbol | 品种代码 |
| ContractName | 合约名称 |
| Multiplier | 合约乘数 |
| TickSize | 最小变动价位 |
| RoundTripFeePerLot | 单手开平合计手续费 |

### 4.2 结构参数

| 参数 | 含义 | 示例 |
|---|---|---:|
| OpeningRangeMinutes | 开盘观察窗口分钟数 | 15 |
| BreakoutOffsetTicks | 突破触发偏移 tick 数 | 1 |

### 4.3 盘中行情数据

| 参数 | 含义 |
|---|---|
| High | K 线最高价 |
| Low | K 线最低价 |
| Close | K 线收盘价或最新价 |
| Time | K 线时间 |

---

## 5. 开盘区间

开盘后先等待观察窗口完成。

例如：

```text
OpeningRangeMinutes = 15
```

计算开盘区间：

```text
OpeningRangeHigh = 开盘观察窗口内最高价
OpeningRangeLow = 开盘观察窗口内最低价
OpeningRangeSize = OpeningRangeHigh - OpeningRangeLow
```

开盘区间完成前，不生成交易结构。

---

## 6. 做多结构

做多触发价：

```text
LongTriggerPrice = OpeningRangeHigh + BreakoutOffsetTicks × TickSize
```

当价格向上突破 `LongTriggerPrice`，生成做多结构。

做多计划：

```text
Direction = Long
EntryPrice = LongTriggerPrice
StopPrice = OpeningRangeLow
InvalidReason = 价格跌回 OpeningRangeLow
```

---

## 7. 做空结构

做空触发价：

```text
ShortTriggerPrice = OpeningRangeLow - BreakoutOffsetTicks × TickSize
```

当价格向下跌破 `ShortTriggerPrice`，生成做空结构。

做空计划：

```text
Direction = Short
EntryPrice = ShortTriggerPrice
StopPrice = OpeningRangeHigh
InvalidReason = 价格涨回 OpeningRangeHigh
```

---

## 8. 结构风险距离

生成 `TradeSetup` 后，本步骤只计算结构本身的价格距离。

```text
SetupPriceRisk = |EntryPrice - StopPrice|
```

如果需要换算成一手价格风险，可以计算：

```text
OneLotPriceRisk = SetupPriceRisk × Multiplier
```

说明：

```text
OneLotPriceRisk 只表示价格从入场价到止损价的距离金额。
它不包含手续费、滑点，也不决定下多少手。
```

---

## 9. 和风险约束的关系

行情结构阶段不决定：

```text
AllowedLots
TradeR
PerTradeCostMaxR 是否满足
MaxMarginUsageRatio 是否满足
是否允许下单
```

这些结果由风险约束阶段返回。

行情结构阶段只把以下内容交给风险约束阶段：

```text
Symbol
Direction
EntryPrice
StopPrice
Multiplier
RoundTripFeePerLot
```

风险约束阶段再计算：

```text
OneLotTradeR
AllowedLots
TradeR
成本占比 c
保证金占用是否合格
最终是否允许交易
```

---

## 10. 结构通过条件

一个行情结构在本阶段只需要满足：

```text
EntryPrice 有效
StopPrice 有效
EntryPrice != StopPrice
```

如果结构价格无效，则拒绝。

风险是否合格，不在本步骤判断。

---

## 11. 拒绝原因

| 原因 | 含义 |
|---|---|
| OpeningRangeNotReady | 开盘区间尚未完成 |
| NoBreakout | 没有突破触发价 |
| InvalidEntryPrice | 入场价无效 |
| InvalidStopPrice | 止损价无效 |
| ZeroPriceRisk | 入场价和止损价相同 |

---

## 12. 输出字段

| 字段 | 含义 |
|---|---|
| Symbol | 品种代码 |
| StructureType | 行情结构类型 |
| Direction | 方向 |
| OpeningRangeHigh | 开盘区间高点 |
| OpeningRangeLow | 开盘区间低点 |
| EntryPrice | 入场价 |
| StopPrice | 止损价 |
| InvalidReason | 结构失效条件 |
| SetupPriceRisk | 入场价到止损价的价格距离 |
| OneLotPriceRisk | 一手价格风险，不含成本 |
| Status | 结构状态 |
| RejectReason | 拒绝原因 |

---

## 13. 完整算例

品种参数：

| 参数 | 数值 |
|---|---:|
| Multiplier | 10 |
| TickSize | 1 |

结构参数：

| 参数 | 数值 |
|---|---:|
| OpeningRangeMinutes | 15 |
| BreakoutOffsetTicks | 1 |

开盘区间：

| 项目 | 数值 |
|---|---:|
| OpeningRangeHigh | 3000 |
| OpeningRangeLow | 2985 |
| OpeningRangeSize | 15 |

做多触发价：

```text
LongTriggerPrice = 3000 + 1 × 1
LongTriggerPrice = 3001
```

如果价格突破 `3001`，生成做多结构：

```text
Direction = Long
EntryPrice = 3001
StopPrice = 2985
```

结构风险距离：

```text
SetupPriceRisk = |3001 - 2985|
SetupPriceRisk = 16
```

一手价格风险：

```text
OneLotPriceRisk = 16 × 10
OneLotPriceRisk = 160
```

结果：

```text
Status = CandidateSetup
RejectReason = None
```

说明：

```text
AllowedLots、TradeR 和是否允许交易，由风险约束阶段返回。
```

---

## 14. 反例

如果开盘区间尚未完成：

```text
OpeningRangeMinutes = 15
当前只收到 10 分钟数据
```

结果：

```text
Status = Rejected
RejectReason = OpeningRangeNotReady
```

如果突破后无法形成有效止损价：

```text
EntryPrice = 3001
StopPrice = null
```

结果：

```text
Status = Rejected
RejectReason = InvalidStopPrice
```

---

## 15. 结论

第三步的目标是：

> 在观察池品种里，寻找能够明确给出入场价和止损价的行情结构。

第一版只使用：

```text
开盘区间突破
```

行情结构只生成 `TradeSetup`。

是否能交易，由后续风险约束阶段判断。
