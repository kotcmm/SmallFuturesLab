# 冲击成本

## 1. 定义

冲击成本是因为自己的订单消耗盘口流动性，导致实际成交均价比下单前可见价格更差的成本。

```text
冲击成本 = 实际成交均价相对参考价格的不利偏移 × 合约乘数 × 手数
```

它来自成交记录和盘口深度。

---

## 2. 示例

假设某品种盘口如下：

| 档位 | 卖价 | 卖量 |
|---|---:|---:|
| 卖一 | 3000 | 1 手 |
| 卖二 | 3001 | 3 手 |
| 卖三 | 3002 | 5 手 |

如果计划买入 1 手，通常可以在 3000 成交。

如果一次买入 5 手，卖一只有 1 手，剩余订单需要继续吃到卖二和卖三。

假设最终成交为：

| 成交价 | 成交手数 |
|---:|---:|
| 3000 | 1 手 |
| 3001 | 3 手 |
| 3002 | 1 手 |

则成交均价：

```text
ActualAverageFillPrice = (3000 × 1 + 3001 × 3 + 3002 × 1) / 5
ActualAverageFillPrice = 3001
```

如果参考价格是 3000，则买入方向的冲击价格偏移为：

```text
ImpactPriceDistance = 3001 - 3000
ImpactPriceDistance = 1
```

假设合约乘数为 10，成交 5 手，则冲击成本为：

```text
MarketImpactCost = 1 × 10 × 5
MarketImpactCost = 50
```

---

## 3. 和手续费、滑点、买卖价差的区别

| 成本 | 含义 |
|---|---|
| 手续费 | 交易所或期货公司收取的费用。 |
| 买卖价差 | 买一和卖一之间天然存在的价格差。 |
| 滑点 | 计划价和实际成交价之间的偏差。 |
| 冲击成本 | 因为自己的订单吃掉盘口流动性，主动把成交均价推向不利方向。 |

说明：

```text
滑点是更宽的概念。
冲击成本可以看作滑点的一种来源。
```

例如：

```text
实际成交价变差，可能来自行情快速变化，也可能来自自己的订单太大。
前者更偏普通滑点，后者更偏冲击成本。
```

---

## 4. 和 EstimatedRoundTripCostPerLot 的关系

`EstimatedRoundTripCostPerLot` 是风险约束阶段使用的预估单手总成本。

当前默认：

```text
EstimatedRoundTripCostPerLot = RoundTripFeePerLot
```

如果后续需要更保守，可以把冲击成本缓冲加入预估成本：

```text
EstimatedRoundTripCostPerLot = RoundTripFeePerLot + SlippageBufferPerLot + SpreadBufferPerLot + ImpactBufferPerLot
```

其中：

| 字段 | 含义 |
|---|---|
| RoundTripFeePerLot | 单手开平合计手续费。 |
| SlippageBufferPerLot | 单手滑点缓冲。 |
| SpreadBufferPerLot | 单手买卖价差缓冲。 |
| ImpactBufferPerLot | 单手冲击成本缓冲。 |

---

## 5. 和 TradeR 的关系

`TradeR` 是单笔交易的计划风险。

```text
TradeR = OneLotTradeR × AllowedLots
```

其中：

```text
OneLotTradeR = OneLotPriceRisk + EstimatedRoundTripCostPerLot
```

如果 `EstimatedRoundTripCostPerLot` 中包含冲击成本缓冲，则冲击成本会进入 `TradeR`。

如果 `EstimatedRoundTripCostPerLot` 中不包含冲击成本缓冲，则冲击成本只会在交易完成后体现在实际盈亏里。

这会影响统计出来的：

```text
a = 实际平均亏损 / TradeR
c = 本笔交易总成本 / TradeR
```

---

## 6. 使用方式

当前规则中，冲击成本只保留为成本概念，不单独建模。

默认处理方式：

```text
ImpactBufferPerLot = 0
EstimatedRoundTripCostPerLot = RoundTripFeePerLot
```

原因：

1. 小资金账户通常交易手数较小；
2. 当前阶段更需要先把风险约束链路跑通；
3. 冲击成本需要盘口深度和成交明细，数据要求更高。

后续如果出现以下情况，需要重新评估冲击成本：

1. 单笔下单手数明显增加；
2. 交易品种盘口深度较差；
3. 实际成交均价长期明显劣于触发价；
4. 实际平均亏损长期高于计划亏损；
5. 成本占比 `c` 长期高于预估。

---

## 7. 结论

冲击成本不是固定手续费。

它来自订单本身对盘口的影响。

在当前规则中：

```text
冲击成本先不单独建模。
```

但在交易记录和复盘中，需要持续观察：

```text
实际成交均价是否长期劣于计划价
实际平均亏损是否长期高于 TradeR
实际成本占比是否长期高于预估 c
```

如果这些问题长期存在，就需要把 `ImpactBufferPerLot` 纳入 `EstimatedRoundTripCostPerLot`。
