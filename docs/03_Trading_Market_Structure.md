# 寻找交易行情结构

## 1. 目的

交易行情结构，是把观察池里的品种转换成可计算的交易计划。

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
错了以后亏多少
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

## 8. 交易计划风险计算

生成 `TradeSetup` 后，才能计算单笔交易风险。

一手价格风险：

```text
OneLotPriceRisk = |EntryPrice - StopPrice| × Multiplier
```

一手计划风险：

```text
OneLotTradeR = OneLotPriceRisk + RoundTripFeePerLot
```

允许手数：

```text
AllowedLots = floor(AccountR / OneLotTradeR)
```

实际计划风险：

```text
TradeR = OneLotTradeR × AllowedLots
```

这些结果交给 `01_Positive_Expectancy_and_Risk_Constraints.md` 的风险约束继续验算。

---

## 9. 通过条件

一个交易结构至少要满足：

```text
AllowedLots >= 1
TradeR <= AccountR
```

如果后续设置最低盈亏比要求，还需要满足：

```text
PotentialReward >= RequiredReward
```

第一版先不在本文件定义止盈规则。

---

## 10. 拒绝原因

| 原因 | 含义 |
|---|---|
| OpeningRangeNotReady | 开盘区间尚未完成 |
| NoBreakout | 没有突破触发价 |
| InvalidStopPrice | 止损价无效 |
| OneLotRiskTooHigh | 一手计划风险超过账户单笔风险上限 |
| NoAllowedLots | 允许手数小于 1 |

---

## 11. 输出字段

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
| OneLotTradeR | 一手计划风险 |
| AllowedLots | 允许手数 |
| TradeR | 实际计划风险 |
| Status | 结构状态 |
| RejectReason | 拒绝原因 |

---

## 12. 完整算例

账户参数：

| 参数 | 数值 |
|---|---:|
| AccountR | 250 |

品种参数：

| 参数 | 数值 |
|---|---:|
| Multiplier | 10 |
| TickSize | 1 |
| RoundTripFeePerLot | 10 |

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

风险计算：

```text
OneLotPriceRisk = |3001 - 2985| × 10
OneLotPriceRisk = 160
```

```text
OneLotTradeR = 160 + 10
OneLotTradeR = 170
```

```text
AllowedLots = floor(250 / 170)
AllowedLots = 1
```

```text
TradeR = 170 × 1
TradeR = 170
```

结果：

```text
Status = CandidateSetup
RejectReason = None
```

---

## 13. 反例

如果开盘区间过大：

```text
OpeningRangeHigh = 3000
OpeningRangeLow = 2960
EntryPrice = 3001
StopPrice = 2960
Multiplier = 10
RoundTripFeePerLot = 10
AccountR = 250
```

风险计算：

```text
OneLotPriceRisk = |3001 - 2960| × 10
OneLotPriceRisk = 410
```

```text
OneLotTradeR = 410 + 10
OneLotTradeR = 420
```

```text
AllowedLots = floor(250 / 420)
AllowedLots = 0
```

结果：

```text
Status = Rejected
RejectReason = NoAllowedLots
```

---

## 14. 结论

第三步的目标是：

> 在观察池品种里，寻找能够明确给出入场价和止损价的行情结构。

第一版只使用：

```text
开盘区间突破
```

结构生成后，再交给风险约束判断能不能交易。
