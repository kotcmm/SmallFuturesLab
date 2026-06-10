# 品种筛选命令行工具

文件：`docs/product/03_Product_Filter_Command_Line.md`

---

## 1. 文档目的

本文档定义品种筛选命令行工具的输入、输出和边界。

命令行工具只负责：

```text
读取本地交易星球下载文件；
解析品种数据；
构造测算条件；
执行 ProductEvaluation 计算；
打印每个品种的风险指标和筛选结论。
```

命令行工具不负责：

```text
采集真实交易所数据；
联网抓取行情；
判断交易方向；
生成交易信号；
生成实盘建议；
CSV 批量输入输出；
Markdown 汇总报告。
```

---

## 2. 工具定位

`SmallFuturesLab.Cli` 是命令行入口。

它引用：

```text
SmallFuturesLab.Core      — 核心领域模型和测算公式
SmallFuturesLab.TradingPlanet — 交易星球文件读取
```

命令行工具不得重新实现业务公式。

命令行工具必须调用 `ProductEvaluation` 中已经实现的计算能力。

---

## 3. 项目结构

```text
src/SmallFuturesLab.Cli/
  Program.cs

test/SmallFuturesLab.Cli.Tests/
  (CLI 参数和文件不存在测试)
```

CLI 项目引用：

```text
SmallFuturesLab.Core
SmallFuturesLab.TradingPlanet
```

---

## 4. 命令形式

当前命令：

```bash
dotnet run --project src/SmallFuturesLab.Cli -- filter --input <文件路径> --account <权益金额> --stop-ticks <止损跳数> --slippage-ticks <滑点跳数>
```

参数：

| 参数 | 必填 | 说明 |
|---|---|---|
| `--input` | 是 | 交易星球下载文件路径（HTML .xls） |
| `--account` | 是 | 账户权益金额 |
| `--stop-ticks` | 是 | 止损跳数 |
| `--slippage-ticks` | 是 | 预估滑点跳数 |

示例：

```bash
dotnet run --project src/SmallFuturesLab.Cli -- filter --input "期货手续费和保证金一览表2026年06月09日更新.xls" --account 10000 --stop-ticks 10 --slippage-ticks 2
```

---

## 5. 参数规则

必须提供所有参数。

如果缺少任一参数，工具应：

```text
输出可读错误；
返回非 0 退出码。
```

如果 `--input` 文件不存在，工具应：

```text
输出可读错误；
返回非 0 退出码。
```

---

## 6. 处理流程

命令执行顺序：

```text
1. 解析命令行参数；
2. 检查 input 文件是否存在；
3. 调用 TradingPlanetFileReader 读取文件；
4. 构造 AccountRiskConfig（使用默认阈值）；
5. 构造 FilterCondition（使用命令行参数）；
6. 对每个 Product 执行 ProductEvaluation 计算；
7. 调用 Evaluate(AccountRiskConfig) 获取状态；
8. 打印每个品种的指标和状态；
9. 返回 0。
```

---

## 7. 输出格式

CLI 打印每个品种的以下字段：

```text
Code（品种代码）
Contract（合约代码）
Price（价格）
MarginMoney（一手保证金）
TotalRiskMoney（总风险金额）
RiskRate（风险占权益比例）
MarginRateOfEquity（保证金占权益比例）
CostRatio（成本占止损比例）
Status（Allowed / Caution / Rejected）
```

输出只表示是否进入后续研究，不表示可以买卖或实盘交易。

---

## 8. 错误输出要求

文件不存在或读取失败时，应输出可读错误信息。

---

## 9. 当前不做什么

当前 CLI 不做：

```text
不做交互式输入；
不做配置文件；
不做联网采集；
不做行情读取；
不做数据库；
不做图表；
不做 Excel；
不做 CSV 输入输出；
不做 Markdown 汇总报告；
不做多命令体系；
不做真实品种数据内置；
不做交易建议。
```

---

## 10. 测试要求

CLI 测试至少覆盖：

```text
缺少 input 参数返回失败；
缺少 account 参数返回失败；
缺少 stop-ticks 参数返回失败；
缺少 slippage-ticks 参数返回失败；
input 文件不存在返回失败；
有效文件可以正常输出结果；
输出不包含交易建议措辞。
```

---

## 11. 完成标准

CLI 完成时，必须满足：

```text
dotnet test 通过；
命令行工具可以读取本地交易星球文件；
命令行工具可以打印测算结果；
无效输入返回非 0；
没有新增行情、策略、回测、实盘或联网采集代码。
```

---

## 12. 当前结论

品种筛选 CLI 是本地研究工具，不是交易系统入口。

它的价值是把交易星球下载文件变成可重复、可审查、可自动化计算的研究产物。

CSV 批量输入输出和 Markdown 汇总属于未来阶段功能。
