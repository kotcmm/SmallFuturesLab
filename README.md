# SmallFuturesLab

SmallFuturesLab 是一个小资金期货品种准入过滤器。

当前目标很简单：

```text
交易星球下载表格
→ 读取成品种信息
→ 输入账户资金和风险假设
→ 计算一手合约对账户的压力
→ 输出 Allowed / Caution / Rejected
```

本项目当前不做策略、不做信号、不做回测、不做实盘、不自动下单。

---

## 项目结构

```text
src/SmallFuturesLab.Core           核心计算：账户风险、品种过滤、读取接口
src/SmallFuturesLab.TradingPlanet  交易星球下载表格读取实现
src/SmallFuturesLab.Cli            命令行入口，读取表格并输出过滤结果
```

测试项目：

```text
test/SmallFuturesLab.Core.Tests
test/SmallFuturesLab.TradingPlanet.Tests
```

---

## 核心概念

`ProductInfo` 表示一个期货合约的基础数据。

`AccountRiskSetting` 表示当前小账户的测算假设，例如账户权益、止损 tick、滑点 tick、风险上限和保证金上限。

`ProductFilterCalculator` 根据一手合约保证金、止损风险、手续费、滑点和账户规模，输出：

```text
Allowed  = 可以进入后续研究
Caution  = 谨慎观察
Rejected = 当前账户规模下排除
```

这些结果不是交易建议，只是是否进入后续研究的准入过滤。

---

## CLI 示例

```bash
dotnet run --project src/SmallFuturesLab.Cli -- filter --input "期货手续费和保证金一览表2026年06月09日更新.xls" --account 10000 --stop-ticks 10 --slippage-ticks 2
```

也可以测算 20000 账户：

```bash
dotnet run --project src/SmallFuturesLab.Cli -- filter --input "期货手续费和保证金一览表2026年06月09日更新.xls" --account 20000 --stop-ticks 10 --slippage-ticks 2
```

---

## 当前边界

本项目当前只回答：

```text
这个合约的一手，对 1 万 / 2 万账户来说，保证金、止损、手续费和滑点压力是否可承受？
```

不回答：

```text
能不能赚钱；
该不该买；
该不该卖；
什么时候入场；
如何止盈；
是否实盘交易。
```
