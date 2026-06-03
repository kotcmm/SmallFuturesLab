# AGENTS.md

本文件约束所有后续参与 SmallFuturesLab 的代码实现者和 AI 编程助手。

SmallFuturesLab 是一个独立的小资金期货生存研究项目。任何代码实现必须服从本文档和 `docs/` 下的项目文档。

---

## 1. 必须先读的文档

实现代码前，必须先阅读：

```text
README.md
docs/00_Project_Principles.md
docs/01_Account_Constraints.md
docs/02_Research_Roadmap.md
docs/03_Operation_Sketch.md
docs/04_Trade_Permission_Pipeline.md
```

代码实现不得突破这些文档定义的边界。

---

## 2. 当前阶段目标

当前阶段只允许实现：

```text
风险预算；
交易许可；
账户约束计算；
合约规格换算；
止损风险计算；
手续费和滑点计算；
1R 计算；
保证金占用检查；
连续亏损压力测试；
每日亏损和交易次数检查；
Allowed / Caution / Rejected 许可结果。
```

当前阶段不实现：

```text
行情方向判断；
入场信号；
技术指标；
策略规则；
收益回测；
实盘下单；
自动交易；
盘口高频逻辑；
隔夜策略；
加仓逻辑；
多品种组合交易；
资金曲线优化。
```

核心原则：

```text
风险没有通过，行情没有资格被讨论。
```

---

## 3. 独立性约束

本项目不得引入任何外部项目的策略概念、市场结构定义、入场规则、止损规则、退出规则或参数体系。

允许使用通用工程实践，例如：

```text
单元测试；
分层设计；
不可变数据对象；
清晰命名；
异常输入校验；
文档同步。
```

但这些工程实践不得携带任何外部策略假设。

---

## 4. 推荐工程栈

当前默认使用：

```text
C#
.NET
xUnit
```

如果未来改用其他语言或测试框架，必须先修改本文档和 README，再开始实现。

---

## 5. 推荐目录结构

当前阶段推荐结构：

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

不要提前创建策略、行情、回测、执行、实盘接口等目录。

---

## 6. 命名与语义约束

代码命名必须表达业务语义。

推荐核心类型：

```text
AccountSnapshot       当前账户快照
InstrumentSpec        合约规格
TradeIdea             候选交易设想
RiskPolicy            风险政策
RiskMetrics           风险计算指标
TradePermissionStatus 交易许可状态
TradePermissionResult 交易许可结果
TradePermissionEvaluator 交易许可评估器
```

禁止使用模糊命名，例如：

```text
Manager
Helper
Processor
Strategy
Signal
Alpha
Predictor
```

除非后续文档明确允许。

---

## 7. 交易许可状态

交易许可结果只能有三类：

```text
Allowed   允许继续研究或模拟；
Caution   谨慎，只允许继续观察或模拟；
Rejected  拒绝，行情再好也不做。
```

不得只返回 `true / false`。

结果必须包含：

```text
状态；
核心计算指标；
通过项；
警告项；
拒绝项；
可读原因。
```

---

## 8. 必须实现的计算公式

实现必须以 `docs/04_Trade_Permission_Pipeline.md` 为准。

核心公式包括：

```text
TickValue = T × M
NotionalPerLot = P × M
MarginPerLot = P × M × μ

RawStopDistance = abs(Pe - Ps)
StopTicks = ceil(RawStopDistance / T)
AdjustedStopDistance = StopTicks × T

StopRiskPerLot = AdjustedStopDistance × M
StopRisk = StopRiskPerLot × L

SlippageMoney = S × T × M × L
CostMoney = F × L + SlippageMoney

TotalRiskMoney = StopRisk + CostMoney
RiskRate = TotalRiskMoney / E

CostRatio = CostMoney / StopRisk

MarginMoney = MarginPerLot × L
MarginRateOfEquity = MarginMoney / E

LossAfter5 = TotalRiskMoney × 5
LossAfter8 = TotalRiskMoney × 8
LossAfter10 = TotalRiskMoney × 10

LossAfter5Rate = LossAfter5 / E
LossAfter8Rate = LossAfter8 / E
LossAfter10Rate = LossAfter10 / E

ProjectedDailyLoss = D + TotalRiskMoney
DailyLossLimitCash = E × d_max
```

不得随意更改公式含义。

如果发现公式不合理，必须先修改文档，再改代码。

---

## 9. 默认风险政策

默认风险政策必须与文档一致：

```text
推荐单笔风险比例：0.5% = 0.005
常规单笔风险上限：1.0% = 0.010
极限单笔风险上限：2.0% = 0.020
每日最大亏损比例：2.0% = 0.020
推荐保证金占用上限：40% = 0.40
极限保证金占用上限：50% = 0.50
推荐成本占比上限：0.20
极限成本占比上限：0.30
每日最大交易次数：3
当前阶段固定手数：1
```

这些值应集中定义在 `RiskPolicy` 中，不得散落在代码各处。

---

## 10. 直接拒绝条件

满足任意一条，必须输出 `Rejected`：

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

实现时应尽量返回全部触发原因，而不是只返回第一个原因。

---

## 11. 谨慎条件

没有触发拒绝，但满足任意一条，应输出 `Caution`：

```text
单笔风险 > 1%；
保证金占用 > 40%；
成本占比 > 0.2R；
连续亏损压力接近上限；
今日已经出现亏损，但未达到日亏损上限。
```

`Caution` 不等于允许实盘。

`Caution` 只表示可以继续观察或模拟。

---

## 12. 允许条件

只有同时满足以下条件，才可输出 `Allowed`：

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

## 13. 单元测试要求

实现必须测试先行。

第一批测试至少覆盖：

```text
1. 文档中的完整示例输出 Caution；
2. 单笔风险超过 2% 输出 Rejected；
3. 成本占比超过 0.3R 输出 Rejected；
4. 保证金占用超过 50% 输出 Rejected；
5. 本笔亏损后超过每日亏损上限输出 Rejected；
6. 今日交易次数达到上限输出 Rejected；
7. 不是 1 手输出 Rejected；
8. 隔夜交易输出 Rejected；
9. 加仓交易输出 Rejected；
10. 所有条件优秀输出 Allowed；
11. 单笔风险超过 1% 但不超过 2% 输出 Caution；
12. 保证金占用超过 40% 但不超过 50% 输出 Caution；
13. 成本占比超过 0.2R 但不超过 0.3R 输出 Caution；
14. 多个拒绝原因必须同时返回。
```

测试名称应描述业务场景，不要只写技术名称。

---

## 14. 输入校验要求

必须处理异常输入。

以下输入应直接拒绝或抛出明确异常，具体方式应在测试中固定：

```text
账户权益 <= 0；
合约乘数 <= 0；
最小变动价位 <= 0；
保证金比例 < 0；
手续费 < 0；
滑点 tick < 0；
手数 <= 0；
入场价或止损价无效；
入场价等于止损价；
StopRisk = 0 导致成本占比无法计算。
```

不要让无效输入产生看似正常的许可结果。

---

## 15. 数值精度要求

当前阶段金额和比率可使用 `double`。

比较浮点数时，测试中应使用容差。

建议：

```text
金额断言容差：1e-6
比例断言容差：1e-8
```

除非后续文档明确要求，否则不要引入复杂金额类型。

---

## 16. 代码风格

代码应保持：

```text
小类；
不可变输入；
纯计算优先；
无外部副作用；
不访问网络；
不读写数据库；
不读取实时行情；
不依赖当前时间；
不在 Evaluator 内部硬编码策略信号。
```

`TradePermissionEvaluator` 应是纯计算器。

同样输入必须得到同样输出。

---

## 17. 文档同步要求

如果实现过程中发现文档和代码冲突：

```text
先停止；
先修改文档；
再修改测试；
最后修改实现。
```

不得让代码偷偷改变项目原则。

所有新规则必须先出现在文档中，再进入代码。

---

## 18. 禁止事项

当前阶段严禁实现或引入：

```text
Strategy；
Signal；
Alpha；
Indicator；
BacktestEngine；
Order；
Broker；
Execution；
MarketDataFeed；
RealtimeQuote；
PositionManager；
Portfolio；
PnLCurve；
Optimization；
ParameterSearch。
```

这些名称或模块会把项目提前带入策略、行情、回测或实盘方向。

如确需引入，必须先由项目文档明确批准。

---

## 19. 当前完成标准

第一阶段代码完成标准：

```text
SmallFuturesLab.Risk 项目可以独立编译；
TradePermissionEvaluator 可以跑通文档中的完整示例；
第一批单元测试全部通过；
没有策略、行情、回测、实盘相关代码；
README 或相关文档已同步说明当前实现范围。
```

---

## 20. 最重要的一句话

```text
先判断账户是否允许，再判断行情是否值得看。
```

如果交易许可输出 `Rejected`，行情再好也不做。

如果交易许可输出 `Caution`，只能观察或模拟。

只有交易许可输出 `Allowed`，才允许进入候选行情研究。