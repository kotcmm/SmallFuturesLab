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

`EstimatedRoundTripCostPerLot` 是风险约束阶段使用的交易前预估单手成本。

当前默认：

```text
EstimatedRoundTripCostPerLot = RoundTripFeePerLot
```

含义：

```text
交易前风险约束只使用可相对稳定估计的手续费。
冲击成本不进入当前默认的 EstimatedRoundTripCostPerLot。
```

原因：

1. 冲击成本只有成交后才能确认；
2. 冲击成本依赖盘口深度、下单手数、成交路径和当时市场状态；
3. 交易前把冲击成本写成固定配置，容易制造虚假的精确感。

成交后再记录实际成本：

```text
ActualRoundTripCost = ActualFee + ActualSlippageCost + ActualSpreadCost + ActualMarketImpactCost
```

其中：

| 字段 | 含义 |
|---|---|
| ActualFee | 实际手续费。 |
| ActualSlippageCost | 实际滑点成本。 |
| ActualSpreadCost | 实际买卖价差成本。 |
| ActualMarketImpactCost | 实际冲击成本。 |

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

当前默认下：

```text
EstimatedRoundTripCostPerLot = RoundTripFeePerLot
```

所以冲击成本不会提前进入 `TradeR`。

冲击成本会在交易完成后体现在实际盈亏和实际成本统计里，并影响：

```text
a = 实际平均亏损 / TradeR
ActualCostR = ActualRoundTripCost / TradeR
```

如果长期出现：

```text
实际平均亏损明显高于 TradeR
ActualCostR 明显高于交易前预估成本占比 c
```

说明执行质量、盘口流动性或成本模型需要复盘。

---

## 6. 使用方式

当前规则中，冲击成本只作为成交后统计项，不作为交易前风险约束输入。

交易前：

```text
EstimatedRoundTripCostPerLot = RoundTripFeePerLot
```

成交后：

```text
记录 ActualMarketImpactCost
统计 ActualRoundTripCost
复盘 ActualCostR
```

后续如果出现以下情况，需要重新评估冲击成本：

1. 单笔下单手数明显增加；
2. 交易品种盘口深度较差；
3. 实际成交均价长期明显劣于触发价；
4. 实际平均亏损长期高于计划亏损；
5. 实际成本占比长期高于交易前预估。

重新评估时，优先检查：

1. 是否应该减少下单手数；
2. 是否应该过滤流动性更差的品种；
3. 是否应该调整下单方式；
4. 是否需要在交易计划阶段引入更保守的成本模型。

---

## 7. 结论

冲击成本不是固定手续费。

它来自订单本身对盘口的影响，并且只能在成交后确认。

在当前规则中：

```text
冲击成本不进入交易前风险约束输入。
```

但在交易记录和复盘中，需要持续观察：

```text
实际成交均价是否长期劣于计划价
实际平均亏损是否长期高于 TradeR
实际成本占比是否长期高于交易前预估
```

如果这些问题长期存在，应该先复盘执行质量和品种流动性，再决定是否调整成本模型。
