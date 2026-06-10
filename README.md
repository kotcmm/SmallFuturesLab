# SmallFuturesLab

SmallFuturesLab 是一个小资金期货品种准入过滤器。

它当前只回答一个问题：

```text
某个期货合约的一手，在指定账户规模下，
保证金、止损、手续费和滑点压力是否可承受？
```

当前不做：

```text
策略；
信号；
行情判断；
回测；
实盘；
自动下单；
交易建议。
```

## 当前项目结构

```text
src/SmallFuturesLab.Core
src/SmallFuturesLab.TradingPlanet
src/SmallFuturesLab.Cli
```

### SmallFuturesLab.Core

核心领域模块，只保留最小模型：

```text
Product                 — 品种信息（class）
AccountRiskConfig       — 账户风险配置（class）
FilterCondition         — 测算条件（class）
ProductEvaluation       — 公式计算与状态判断（class）
ProductEvaluationStatus — Allowed / Caution / Rejected（enum）
```

流程：

```text
Product + AccountRiskConfig + FilterCondition
→ ProductEvaluation
→ ProductEvaluationStatus
```

Core 不知道数据来自交易星球、Excel、CSV、网络还是 CTP。

### SmallFuturesLab.TradingPlanet

交易星球下载表格读取模块。

它只负责：

```text
读取本地交易星球下载文件；
解析字段；
通过 ProductSpecLookup 补齐 Multiplier / TickSize；
生成 Product；
保留读取错误。
```

它不做风险计算，也不输出 Allowed / Caution / Rejected。

### SmallFuturesLab.Cli

命令行入口。

它负责：

```text
读取命令行参数；
调用 TradingPlanet 读取本地文件得到 Products；
构造 AccountRiskConfig 和 FilterCondition；
调用 ProductEvaluation 计算每个 Product；
打印结果。
```

## 运行示例

```bash
dotnet run --project src/SmallFuturesLab.Cli -- filter --input "期货手续费和保证金一览表2026年06月09日更新.xls" --account 10000 --stop-ticks 10 --slippage-ticks 2
```

输出结果只表示是否进入后续研究，不表示可以买卖或实盘交易。
