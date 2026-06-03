# 交易许可流水线

文件：`docs/04_Trade_Permission_Pipeline.md`

---

## 1. 文档目的

本文档定义 SmallFuturesLab 的交易许可模块。

该模块不判断行情方向，不判断买卖点，不判断策略是否有优势。

它只回答一个问题：

```text
在当前账户、当前品种、当前周期和当前止损设想下，这类交易是否被账户允许继续研究？
```

核心原则：

```text
风险没有通过，行情没有资格被讨论。
```

---

## 2. 模块定位

交易许可模块位于所有行情判断之前。

流程顺序是：

```text
账户预算
→ 合约换算
→ 止损风险
→ 交易成本
→ 实际 1R
→ 单笔风险占比
→ 保证金占用
→ 成本占比
→ 连续亏损压力
→ 每日亏损限制
→ 交易次数限制
→ 硬性禁止条件
→ 交易许可结果
```

输出结果分为三类：

```text
Allowed   = 允许继续研究或模拟；
Caution   = 谨慎，只允许继续观察或模拟；
Rejected  = 拒绝，行情再好也不做。
```

---

## 3. 输入定义

### 3.1 账户输入

| 符号 | 含义 |
|---|---|
| `E` | 当前账户权益 |
| `A` | 当前可用资金 |
| `D` | 今日已亏损金额 |
| `N` | 今日已交易次数 |

示例：

```text
E = 20,000 元
A = 18,000 元
D = 0 元
N = 0 次
```

---

### 3.2 风险政策输入

| 符号 | 含义 | 当前值 |
|---|---|---:|
| `r_rec` | 推荐单笔风险比例 | 0.005 |
| `r_norm` | 常规单笔风险上限 | 0.010 |
| `r_max` | 极限单笔风险上限 | 0.020 |
| `d_max` | 每日最大亏损比例 | 0.020 |
| `m_pref` | 推荐保证金占用上限 | 0.40 |
| `m_max` | 极限保证金占用上限 | 0.50 |
| `c_pref` | 推荐成本占比上限 | 0.20 |
| `c_max` | 极限成本占比上限 | 0.30 |
| `N_max` | 每日最大交易次数 | 3 |

---

### 3.3 合约输入

| 符号 | 含义 |
|---|---|
| `P` | 当前价格 |
| `M` | 合约乘数 |
| `T` | 最小变动价位 |
| `μ` | 保证金比例 |
| `F` | 单手开平总手续费估计 |

示例：

```text
P = 2500
M = 10
T = 1
μ = 0.10
F = 6 元
```

---

### 3.4 交易设想输入

该输入不是策略信号，只是用于风险测算的候选交易设想。

| 符号 | 含义 |
|---|---|
| `Pe` | 假设入场价 |
| `Ps` | 假设止损价 |
| `L` | 手数 |
| `S` | 预估总滑点跳数 |

当前阶段固定：

```text
L = 1 手
```

示例：

```text
Pe = 2500
Ps = 2488
L = 1
S = 2 tick
```

---

## 4. 节点 1：账户风险预算

### 4.1 目的

先计算账户允许亏多少钱。

### 4.2 公式

```text
推荐单笔风险金额 = E × r_rec
常规单笔风险上限 = E × r_norm
极限单笔风险上限 = E × r_max
每日亏损上限 = E × d_max
```

### 4.3 示例

当 `E = 20,000`：

```text
推荐单笔风险金额 = 20,000 × 0.005 = 100 元
常规单笔风险上限 = 20,000 × 0.010 = 200 元
极限单笔风险上限 = 20,000 × 0.020 = 400 元
每日亏损上限 = 20,000 × 0.020 = 400 元
```

### 4.4 输出

```text
RecommendedRiskCash = 100
NormalRiskCash = 200
ExtremeRiskCash = 400
DailyLossLimitCash = 400
```

---

## 5. 节点 2：合约基础金额计算

### 5.1 目的

把合约规格转换成账户实际金额。

### 5.2 公式

```text
TickValue = T × M
NotionalPerLot = P × M
MarginPerLot = P × M × μ
```

### 5.3 示例

```text
T = 1
M = 10
P = 2500
μ = 0.10
```

计算：

```text
TickValue = 1 × 10 = 10 元
NotionalPerLot = 2500 × 10 = 25,000 元
MarginPerLot = 2500 × 10 × 0.10 = 2,500 元
```

### 5.4 输出

```text
一跳金额 = 10 元
一手名义金额 = 25,000 元
一手保证金 = 2,500 元
```

---

## 6. 节点 3：止损距离标准化

### 6.1 目的

把入场价与止损价之间的距离转换成最小变动单位。

### 6.2 公式

```text
RawStopDistance = abs(Pe - Ps)
StopTicks = ceil(RawStopDistance / T)
AdjustedStopDistance = StopTicks × T
```

### 6.3 示例

```text
Pe = 2500
Ps = 2488
T = 1
```

计算：

```text
RawStopDistance = abs(2500 - 2488) = 12 点
StopTicks = ceil(12 / 1) = 12 tick
AdjustedStopDistance = 12 × 1 = 12 点
```

### 6.4 输出

```text
StopTicks = 12
AdjustedStopDistance = 12
```

---

## 7. 节点 4：一手止损风险计算

### 7.1 目的

计算如果触发止损，一手会亏多少钱。

### 7.2 公式

```text
StopRiskPerLot = AdjustedStopDistance × M
StopRisk = StopRiskPerLot × L
```

当前阶段固定：

```text
L = 1
```

### 7.3 示例

```text
AdjustedStopDistance = 12
M = 10
L = 1
```

计算：

```text
StopRiskPerLot = 12 × 10 = 120 元
StopRisk = 120 × 1 = 120 元
```

### 7.4 输出

```text
StopRisk = 120 元
```

---

## 8. 节点 5：手续费和滑点计算

### 8.1 目的

计算短线交易中不可忽略的执行成本。

### 8.2 公式

```text
SlippageMoney = S × T × M × L
CostMoney = F × L + SlippageMoney
```

其中 `F` 使用开平合计手续费估计。

### 8.3 示例

```text
S = 2 tick
T = 1
M = 10
L = 1
F = 6 元
```

计算：

```text
SlippageMoney = 2 × 1 × 10 × 1 = 20 元
CostMoney = 6 × 1 + 20 = 26 元
```

### 8.4 输出

```text
SlippageMoney = 20 元
CostMoney = 26 元
```

---

## 9. 节点 6：实际 1R 定义

### 9.1 目的

定义本笔交易的完整风险单位。

SmallFuturesLab 使用保守定义：

```text
1R = 止损亏损 + 手续费 + 滑点
```

### 9.2 公式

```text
TotalRiskMoney = StopRisk + CostMoney
```

### 9.3 示例

```text
StopRisk = 120
CostMoney = 26
```

计算：

```text
TotalRiskMoney = 120 + 26 = 146 元
```

### 9.4 输出

```text
1R = 146 元
```

---

## 10. 节点 7：单笔风险占比判断

### 10.1 目的

判断这笔交易的风险是否超过账户承受能力。

### 10.2 公式

```text
RiskRate = TotalRiskMoney / E
```

### 10.3 示例

```text
TotalRiskMoney = 146
E = 20,000
```

计算：

```text
RiskRate = 146 / 20,000 = 0.0073 = 0.73%
```

### 10.4 判断逻辑

```text
RiskRate <= 0.5%        优秀
0.5% < RiskRate <= 1%   允许
1% < RiskRate <= 2%     谨慎
RiskRate > 2%           拒绝
```

### 10.5 示例判断

```text
0.73% <= 1%
```

结果：

```text
单笔风险允许
```

---

## 11. 节点 8：成本占比判断

### 11.1 目的

判断手续费和滑点是否过高。

成本占比使用止损风险作为分母：

```text
CostRatio = CostMoney / StopRisk
```

原因：

```text
StopRisk 代表行情判断错误时的正常风险空间；
如果 CostMoney 占 StopRisk 太高，说明该品种或周期对短线不友好。
```

### 11.2 示例

```text
CostMoney = 26
StopRisk = 120
```

计算：

```text
CostRatio = 26 / 120 = 0.2167 = 21.67%
```

### 11.3 判断逻辑

```text
CostRatio <= 0.20        优先研究
0.20 < CostRatio <= 0.30 谨慎研究
CostRatio > 0.30         原则上排除
```

### 11.4 示例判断

```text
21.67% > 20%
21.67% <= 30%
```

结果：

```text
成本偏高，但没有直接排除。
```

---

## 12. 节点 9：保证金占用判断

### 12.1 目的

判断一手保证金是否占用过多账户权益。

### 12.2 公式

```text
MarginMoney = MarginPerLot × L
MarginRateOfEquity = MarginMoney / E
```

### 12.3 示例

```text
MarginPerLot = 2,500
L = 1
E = 20,000
```

计算：

```text
MarginMoney = 2,500 × 1 = 2,500 元
MarginRateOfEquity = 2,500 / 20,000 = 12.5%
```

### 12.4 判断逻辑

```text
MarginRateOfEquity <= 40%        优先研究
40% < MarginRateOfEquity <= 50%  谨慎研究
MarginRateOfEquity > 50%         原则上排除
```

### 12.5 示例判断

```text
12.5% <= 40%
```

结果：

```text
保证金占用允许
```

---

## 13. 节点 10：连续亏损压力测试

### 13.1 目的

判断连续亏损后账户是否还能继续执行。

### 13.2 公式

```text
LossAfter5 = TotalRiskMoney × 5
LossAfter8 = TotalRiskMoney × 8
LossAfter10 = TotalRiskMoney × 10

LossAfter5Rate = LossAfter5 / E
LossAfter8Rate = LossAfter8 / E
LossAfter10Rate = LossAfter10 / E
```

### 13.3 示例

```text
TotalRiskMoney = 146
E = 20,000
```

计算：

```text
LossAfter5 = 146 × 5 = 730 元，占 3.65%
LossAfter8 = 146 × 8 = 1,168 元，占 5.84%
LossAfter10 = 146 × 10 = 1,460 元，占 7.30%
```

### 13.4 判断逻辑

```text
连续 5 次亏损 <= 账户 5%
连续 8 次亏损 <= 账户 8%
连续 10 次亏损 <= 账户 10%
```

### 13.5 示例判断

```text
3.65% <= 5%
5.84% <= 8%
7.30% <= 10%
```

结果：

```text
连续亏损压力测试通过
```

---

## 14. 节点 11：每日亏损限制检查

### 14.1 目的

判断如果本笔交易亏损，是否会突破当日亏损上限。

### 14.2 公式

```text
ProjectedDailyLoss = D + TotalRiskMoney
DailyLossLimitCash = E × d_max
```

判断：

```text
ProjectedDailyLoss <= DailyLossLimitCash
```

### 14.3 示例一：通过

```text
D = 0
TotalRiskMoney = 146
DailyLossLimitCash = 400
```

计算：

```text
ProjectedDailyLoss = 0 + 146 = 146 元
146 <= 400
```

结果：

```text
通过
```

### 14.4 示例二：拒绝

如果今日已经亏损 300 元：

```text
D = 300
ProjectedDailyLoss = 300 + 146 = 446 元
446 > 400
```

结果：

```text
拒绝。
原因：如果本笔亏损，将超过每日亏损上限。
```

---

## 15. 节点 12：交易次数检查

### 15.1 目的

防止过度交易。

### 15.2 公式

```text
N < N_max
```

### 15.3 示例一：通过

```text
N = 0
N_max = 3
0 < 3
```

结果：

```text
通过
```

### 15.4 示例二：拒绝

```text
N = 3
N_max = 3
3 < 3 为 false
```

结果：

```text
拒绝。
原因：今日交易次数已达到上限。
```

---

## 16. 节点 13：硬性禁止条件检查

### 16.1 目的

检查是否违反当前阶段研究边界。

当前阶段直接拒绝：

```text
不是 1 手；
无明确止损；
隔夜；
加仓；
摊平；
多品种同时持仓；
高频交易；
主观盘口感觉；
止损无法提前定义。
```

### 16.2 判断逻辑

```text
如果 L != 1，拒绝。
如果 IsOvernight = true，拒绝。
如果 IsAddPosition = true，拒绝。
如果 HasStop = false，拒绝。
如果 StopPrice 无法提前定义，拒绝。
如果交易理由依赖主观感觉且无法记录，拒绝。
```

只要任意硬性条件失败：

```text
Status = Rejected
```

---

## 17. 综合许可评级

### 17.1 直接拒绝条件

满足任意一条，输出 `Rejected`：

```text
不是 1 手；
无明确止损；
隔夜；
加仓；
单笔风险 > 2%；
保证金占用 > 50%；
成本占比 > 0.3R；
本笔亏损后超过每日亏损上限；
今日交易次数已达上限；
连续亏损压力测试严重失败；
依赖扛单、摊平或主观感觉。
```

---

### 17.2 谨慎条件

没有触发拒绝，但满足任意一条，输出 `Caution`：

```text
单笔风险 > 1%；
保证金占用 > 40%；
成本占比 > 0.2R；
连续亏损压力接近上限；
今日已经出现亏损，但未达到日亏损上限。
```

---

### 17.3 允许条件

必须同时满足，输出 `Allowed`：

```text
只做 1 手；
有明确止损；
不隔夜；
不加仓；
单笔风险 <= 1%；
保证金占用 <= 40%；
成本占比 <= 0.2R；
连续亏损压力测试通过；
本笔亏损后不超过每日亏损上限；
今日交易次数未达到上限。
```

---

## 18. 完整数值推演

### 18.1 输入

账户：

```text
E = 20,000 元
D = 0 元
N = 0 次
N_max = 3
```

风险政策：

```text
推荐单笔风险 = 0.5%
常规单笔上限 = 1%
极限单笔上限 = 2%
每日亏损上限 = 2%
保证金优先上限 = 40%
保证金极限上限 = 50%
成本优先上限 = 0.2R
成本极限上限 = 0.3R
```

合约：

```text
P = 2500
M = 10
T = 1
μ = 0.10
F = 6 元
```

交易设想：

```text
Pe = 2500
Ps = 2488
L = 1
S = 2 tick
不隔夜
不加仓
```

---

### 18.2 计算过程

账户预算：

```text
推荐单笔风险 = 20,000 × 0.5% = 100 元
常规单笔上限 = 20,000 × 1% = 200 元
极限单笔上限 = 20,000 × 2% = 400 元
每日亏损上限 = 20,000 × 2% = 400 元
```

合约金额：

```text
TickValue = 1 × 10 = 10 元
MarginPerLot = 2500 × 10 × 10% = 2,500 元
```

止损距离：

```text
RawStopDistance = abs(2500 - 2488) = 12 点
StopTicks = ceil(12 / 1) = 12 tick
AdjustedStopDistance = 12 × 1 = 12 点
```

止损风险：

```text
StopRisk = 12 × 10 × 1 = 120 元
```

手续费和滑点：

```text
SlippageMoney = 2 × 1 × 10 × 1 = 20 元
CostMoney = 6 + 20 = 26 元
```

实际 1R：

```text
1R = 120 + 26 = 146 元
```

单笔风险占比：

```text
RiskRate = 146 / 20,000 = 0.73%
```

成本占比：

```text
CostRatio = 26 / 120 = 21.67%
```

保证金占比：

```text
MarginRateOfEquity = 2,500 / 20,000 = 12.5%
```

连续亏损压力：

```text
连续 5 次亏损 = 146 × 5 = 730 元，占 3.65%
连续 8 次亏损 = 146 × 8 = 1,168 元，占 5.84%
连续 10 次亏损 = 146 × 10 = 1,460 元，占 7.30%
```

每日亏损检查：

```text
ProjectedDailyLoss = 0 + 146 = 146 元
146 <= 400，通过
```

交易次数检查：

```text
N = 0
0 < 3，通过
```

硬性条件：

```text
L = 1，通过
不隔夜，通过
不加仓，通过
有明确止损，通过
```

---

### 18.3 最终输出

```text
Status = Caution
```

原因：

```text
单笔风险 = 0.73%，账户可承受；
保证金占用 = 12.5%，账户可承受；
连续亏损压力测试通过；
每日亏损限制通过；
交易次数限制通过；
硬性条件通过；
但手续费 + 滑点占止损风险 21.67%，超过 0.2R，属于谨慎研究。
```

结论：

```text
可以继续观察或模拟，
但不是优先候选，
不应直接进入实盘。
```

---

## 19. 拒绝案例推演

假设止损更远：

```text
Pe = 2500
Ps = 2460
```

其他条件不变。

止损距离：

```text
RawStopDistance = abs(2500 - 2460) = 40 点
StopTicks = 40
AdjustedStopDistance = 40 点
```

止损风险：

```text
StopRisk = 40 × 10 = 400 元
```

成本：

```text
SlippageMoney = 20 元
CostMoney = 6 + 20 = 26 元
```

实际 1R：

```text
1R = 400 + 26 = 426 元
```

风险占比：

```text
RiskRate = 426 / 20,000 = 2.13%
```

判断：

```text
2.13% > 2%
```

输出：

```text
Status = Rejected
```

原因：

```text
单笔风险超过账户 2%，直接拒绝。
```

此时不再讨论行情是否好。

---

## 20. 标准输出格式

每次评估应输出：

```text
Status: Allowed / Caution / Rejected

核心指标：
账户权益
1R
单笔风险占比
保证金占比
成本占比
连续 5 次亏损占比
连续 8 次亏损占比
连续 10 次亏损占比
本笔亏损后今日累计亏损

通过项：
...

警告项：
...

拒绝项：
...

结论：
...
```

示例：

```text
Status: Caution

核心指标：
账户权益：20,000
1R：146 元
单笔风险占比：0.73%
保证金占比：12.5%
成本占比：21.67%
连续 5 次亏损：3.65%
连续 8 次亏损：5.84%
连续 10 次亏损：7.30%
本笔亏损后今日累计亏损：146 / 400

通过项：
单笔风险通过
保证金通过
连续亏损压力通过
每日亏损限制通过
交易次数通过
硬性条件通过

警告项：
成本占比超过 0.2R

拒绝项：
无

结论：
可继续模拟观察，但不是优先候选。
```

---

## 21. 后续代码结构建议

未来可以按以下结构实现：

```text
src/
  SmallFuturesLab.Risk/
    AccountSnapshot.cs
    InstrumentSpec.cs
    TradeIdea.cs
    RiskPolicy.cs
    RiskMetrics.cs
    TradePermissionStatus.cs
    TradePermissionResult.cs
    TradePermissionEvaluator.cs

test/
  SmallFuturesLab.Risk.Tests/
    TradePermissionEvaluatorTests.cs
```

当前阶段先以文档为准，不急于写代码。

---

## 22. 当前结论

交易许可模块的本质是：

```text
先判断账户是否允许，
再判断行情是否值得看。
```

如果风险许可输出 `Rejected`，行情再好也不做。

如果风险许可输出 `Caution`，只能继续观察或模拟。

只有风险许可输出 `Allowed`，才允许进入候选行情研究。