# SmallFuturesLab

SmallFuturesLab 是一个小资金期货品种准入过滤器。

它当前只回答一个问题：

```text
某个期货合约的一手，在 1 万 / 2 万账户下，
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

核心领域模块。

只包含：

```text
品种信息；
账户资料；
风控阈值；
测算场景；
品种过滤计算；
过滤结果。
```

Core 不知道数据来自交易星球、Excel、CSV、网络还是 CTP。

### SmallFuturesLab.TradingPlanet

交易星球下载表格读取模块。

它只负责：

```text
读取本地交易星球下载文件；
解析字段；
转换成 Core 的 ProductInfo；
保留读取错误和来源备注。
```

它不做风险计算，也不输出 Allowed / Caution / Rejected。

### SmallFuturesLab.Cli

命令行入口。

它负责：

```text
读取命令行参数；
调用 TradingPlanet 读取本地文件；
调用 Core 过滤；
输出结果。
```

## 运行示例

```bash
dotnet run --project src/SmallFuturesLab.Cli -- filter --input "期货手续费和保证金一览表2026年06月09日更新.xls" --account 10000 --stop-ticks 10 --slippage-ticks 2
```

输出结果只表示是否进入后续研究，不表示可以买卖或实盘交易。
