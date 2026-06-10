# 小资金期货交易研究路线图

文件：`docs/02_Research_Roadmap.md`

---

## 1. 当前阶段目标

当前阶段不是策略实现，也不是收益回测。

当前阶段已完成的最小闭环：

```text
交易星球下载文件
→ TradingPlanetFileReader 解析
→ Product
→ AccountRiskConfig + FilterCondition
→ ProductEvaluation（公式计算 + 阈值判断）
→ ProductEvaluationStatus（Allowed / Caution / Rejected）
→ CLI 输出
```

这个闭环只回答一个问题：

```text
给定品种的一手，在指定账户规模和止损设想下，是否可承受？
```

---

## 2. 总体路线

```text
账户约束
→ 品种测算公式
→ 品种筛选
→ 周期筛选
→ 风控底线
→ 候选方向评价
→ 数据实验
→ 规则冻结
→ 回测证伪
→ 模拟交易
→ 小规模实盘
```

每个阶段必须有明确产出。

前一阶段不通过，不进入下一阶段。

---

## 3. 当前已完成

当前代码已实现：

```text
1. 账户约束（AccountRiskConfig 中的阈值配置）；
2. 品种测算公式（ProductEvaluation 中的 9 个公式）；
3. 品种筛选（ProductEvaluation.Evaluate 输出 Allowed / Caution / Rejected）。
```

当前不做：

```text
CSV 批量输入输出；
多止损场景展开；
ATR 止损测算；
周期筛选；
最终策略规则；
收益率结论；
实盘准备；
自动化交易。
```

---

## 4. 当前阶段不研究

```text
高频盘口交易；
每天数十笔交易；
多品种快速切换交易；
依赖主观盘口感觉的交易；
需要专职盯盘才能执行的交易；
隔夜持仓策略；
加仓策略；
摊平策略；
网格策略；
重仓翻倍策略。
```

---

## 5. 阶段 1：品种测算公式（已完成）

目标：建立可计算、可测试的品种风险测算公式。

当前产出：

```text
docs/04_Product_Evaluation_Formula.md
src/SmallFuturesLab.Core/ProductEvaluation.cs
test/SmallFuturesLab.Core.Tests/ProductEvaluationTests.cs
```

通过标准：

```text
所有 9 个公式有独立测试；
RiskRate、MarginRateOfEquity、CostRatio 阈值判断有测试；
Allowed / Caution / Rejected 三种状态有测试；
dotnet test 全部通过。
```

---

## 6. 阶段 2：数据源读取（已完成）

目标：从本地交易星球下载文件中读取品种数据。

当前产出：

```text
src/SmallFuturesLab.TradingPlanet/TradingPlanetFileReader.cs
test/SmallFuturesLab.TradingPlanet.Tests/TradingPlanetFileReaderTests.cs
```

通过标准：

```text
能解析 HTML 表格中的价格、保证金、手续费；
能补齐合约乘数和最小变动价位；
能保留读取错误；
dotnet test 全部通过。
```

---

## 7. 阶段 3：CLI 入口（已完成）

目标：提供命令行入口，运行单文件测算。

当前产出：

```text
src/SmallFuturesLab.Cli/Program.cs
```

通过标准：

```text
能读取本地交易星球文件；
能构造 AccountRiskConfig 和 FilterCondition；
能调用 ProductEvaluation 计算每个品种；
能打印结果；
参数缺失时返回非 0。
```

---

## 8. 未来阶段：批量筛选与 CSV 输入

> 以下阶段尚未实现，仅在规划中。

目标：支持从 CSV 批量输入，输出计算后 CSV 和 Markdown 汇总。

需要实现：

```text
CSV 读取与校验；
多止损场景展开；
多账户规模展开；
公式字段自动计算；
结果汇总报告。
```

---

## 9. 未来阶段：周期筛选

目标：判断哪些周期适合小资金账户和人工执行。

候选周期：

```text
1 分钟；
3 分钟；
5 分钟；
15 分钟；
30 分钟。
```

---

## 10. 未来阶段：风控底线

目标：在候选方向之前，先冻结风控底线。

至少定义：

```text
1R；
单笔最大风险；
每日最大亏损；
手续费 + 滑点占 1R 上限；
单日最大交易次数；
连续亏损后的暂停规则；
是否允许隔夜；
是否允许加仓；
是否允许多品种同时持仓；
强制停止交易条件。
```

---

## 11. 后续阶段

### 11.1 候选方向评价

在账户、品种、周期和风控底线完成后，才允许提出候选方向。

候选方向不是最终策略，只回答：

```text
适合什么品种；
适合什么周期；
入场思想是否简单；
失败退出是否清楚；
是否符合风控底线。
```

---

### 11.2 数据实验

数据实验不是收益优化，只检查：

```text
信号是否过多或过少；
止损距离是否可承受；
成本是否过高；
是否需要过度盯盘；
是否存在明显灾难性结构。
```

---

### 11.3 规则冻结

冻结：

```text
交易品种；
交易周期；
入场条件；
初始止损；
失败退出；
止盈或保护；
不交易条件；
每日限制；
连续亏损处理。
```

---

### 11.4 回测证伪

回测只用于暴露问题，不用于证明未来赚钱。

必须检查：

```text
最大连续亏损；
最大回撤；
单笔最大亏损；
手续费占比；
滑点敏感性；
样本外表现；
极端行情表现。
```

---

### 11.5 模拟交易与小规模实盘

进入实盘前必须满足：

```text
账户约束通过；
品种测算通过；
周期约束通过；
风控规则冻结；
回测证伪无致命问题；
模拟交易可执行；
交易记录模板完成。
```

---

## 12. 当前下一步

当前下一步：

```text
1. 继续验证和完善 docs/04_Product_Evaluation_Formula.md 中的公式；
2. 扩展 TradingPlanet 读取覆盖更多品种；
3. 在 CLI 中增加更多输出格式选项。
```

> CSV 批量输入、多场景展开、ATR 测算等功能属于未来阶段，不在当前任务范围。

---

## 13. 当前结论

现在先把"给定品种和账户，一手风险是否可承受"说清楚，再谈"什么行情值得交易"。
