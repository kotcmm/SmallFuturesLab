# AGENTS.md

## 1. 文档优先级

编码前先阅读：

```text
README.md
docs/00_SmallFuturesLab_System_Framework.md
docs/01_Positive_Expectancy_and_Risk_Constraints.md
docs/02_Daily_Candidate_Product_Selection.md
docs/03_Trading_Market_Structure.md
```

涉及术语解释时阅读：

```text
docs/01_Positive_Expectancy_and_Risk_Constraints/Actual_Average_Loss.md
```

规则归属：

```text
docs/ = 业务规则
AGENTS.md = 工程规则
README.md = 项目入口说明
```

代码实现必须服从 `docs/` 下的业务文档。

---

## 2. 当前业务方向

SmallFuturesLab 当前围绕一条主线推进：

```text
正期望条件 → 风险约束 → 候选品种筛选 → 交易行情结构 → 交易计划验算 → 执行记录 → 验证修正
```

当前核心文档已经覆盖前三步：

```text
建立正期望与风险约束
日内候选品种筛选
寻找交易行情结构
```

第二步的筛选时间是：

```text
开盘前
```

第三步第一版只实现：

```text
开盘区间突破
```

核心概念：

```text
AccountR
TradeR
p
b
a
c
E
MinPlannedRewardR
PerTradeCostMaxR
MaxMarginUsageRatio
DailyLossLimit
DailyProfitLockR
MaxDailyTrades
OneLotMargin
TickValue
MinimumOneLotTradeR
TradeSetup
TargetPrice
AllowedLots
OpeningRangeHigh
OpeningRangeLow
DailyStructureState
```

实现代码时，命名应优先使用这些业务术语。

---

## 3. 文档变更职责

业务文档由用户和 ChatGPT 维护。

本地 AI 编码助手主要负责编码实现。

发现以下情况时，先停止编码并反馈：

```text
业务文档不清楚
业务文档之间冲突
代码实现需要新增业务规则
测试预期和文档不一致
需要新增公式、阈值或流程边界
```

只有任务明确要求修改文档时，才修改对应 Markdown 文件。

---

## 4. 当前实现边界

当前代码实现应优先服务于：

```text
正期望模型计算
最低胜率反推
AccountR 计算
TradeR 计算
MinPlannedRewardR 目标价推导
PerTradeCostMaxR 单笔成本检查
MaxMarginUsageRatio 保证金占用检查
AllowedLots 手数计算
DailyLossLimit 每日亏损停止
DailyProfitLockR 每日盈利保护
MaxDailyTrades 每日交易次数限制
日内候选品种筛选
开盘区间突破 TradeSetup 生成
单品种日内结构状态控制
参数输入到输出约束的完整推算
```

实现应保持纯计算、可测试、无外部副作用。

---

## 5. 本地 AI 编码助手协作流程

默认远程仓库名：`origin`。

开始任务前：

```text
git fetch origin
基于 origin/main 创建任务分支
```

开发流程：

```text
先写测试
确认测试失败
写最小实现
确认测试通过
必要时重构
再次运行测试
```

默认验收命令：

```bash
dotnet test
```

验收通过后：

```text
提交 commit
推送到 origin
创建 Pull Request
```

PR 描述应包含：

```text
实现范围
测试结果
是否修改业务文档
是否遵守 TDD
遗留问题
```

---

## 6. 技术栈与项目结构

技术栈：

```text
C#
.NET 10
xUnit
```

当前生产项目：

```text
SmallFuturesLab.Core
SmallFuturesLab.TradingPlanet
SmallFuturesLab.Cli
```

当前测试项目：

```text
SmallFuturesLab.Core.Tests
SmallFuturesLab.TradingPlanet.Tests
```

新增顶层模块前，先确认文档方向。

---

## 7. TDD 开发约束

开发必须遵循 TDD：

```text
先写测试 → 运行确认失败 → 写最小实现 → 运行确认通过 → 重构
```

新增业务行为、边界条件、异常处理时，必须先新增测试。

修复缺陷时，必须先补一个能复现缺陷的失败测试，再修复实现。

如果环境限制导致无法运行测试，最终输出必须说明原因。

---

## 8. 实现风格

优先：

```text
小类
纯计算
不可变输入
业务命名清晰
无外部副作用
```

风险计算、期望计算、品种筛选、交易结构、参数推导应设计成可独立测试的领域对象或纯函数。

---

## 9. 命名约束

避免模糊命名：

```text
Manager
Helper
Processor
Service
Engine
Strategy
Signal
Alpha
Predictor
```

优先使用文档中的业务命名。

示例：

```text
AccountR
TradeR
ExpectedValue
CostInR
MinimumWinRate
RiskConstraint
CandidateProductSelection
CandidateProductSelectionResult
OpeningRangeBreakout
TradeSetup
TradePlan
DailyStructureState
```

---

## 10. 生产代码注释约定

新增或修改 `src/` 下的生产代码时：

```text
public 类型必须有中文 XML 注释
public 属性必须有中文 XML 注释
public 方法必须有中文 XML 注释
public enum 及 enum 成员必须有中文 XML 注释
```

一个 `public class` / `record` / `interface` / `enum` 使用一个独立 `.cs` 文件。

文件名必须与 public 类型名一致。

如果使用英文术语，应在附近给出中文解释。

---

## 11. 测试要求

测试名称应描述业务行为。

当前阶段测试重点：

```text
AccountR 计算
TradeR 计算
TradeR <= AccountR 约束
成本换算为 R
最低胜率反推
正期望计算
MinPlannedRewardR 目标价推导
AllowedLots 手数计算
DailyLossLimit 每日亏损停止
DailyProfitLockR 每日盈利保护
MaxDailyTrades 每日交易次数限制
最大回撤金额计算
一手保证金计算
最小交易颗粒度过滤
日内候选品种筛选状态
拒绝原因
开盘区间计算
开盘区间突破 TradeSetup 生成
单品种同结构每日只触发一次
DailyStructureState 状态切换
```

当前阶段允许使用 `double`。

浮点断言必须使用容差。

---

## 12. 文档与代码冲突处理

代码和文档冲突时：

```text
1. 停止编码
2. 记录冲突点
3. 在最终输出或 PR 描述中说明问题
4. 等待文档更新后再继续实现
```

不要让代码悄悄改变项目规则。

---

## 13. 完成标准

一次编码任务完成时，应满足：

```text
代码可以编译
相关测试全部通过
实现与当前 docs/ 文档一致
没有引入未定义的业务规则
已提交并创建 Pull Request
```
