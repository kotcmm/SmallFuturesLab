# SmallFuturesLab.ProductData 模块说明

## 模块定位

SmallFuturesLab.ProductData **不是策略模块，不是行情判断模块，也不是回测模块**。

它的职责是：

```text
把不同研究数据源中的品种数据读取、清洗、合并、展开为 ProductFilter 可以接收的输入行。
```

ProductData 只回答数据从哪来、怎么合并、怎么标准化；它不回答某个品种是否可以交易、某个方向是否有收益机会。

---

## 当前目录结构

```text
src/SmallFuturesLab.ProductData/
  Abstractions/      数据源接口和数据源类型枚举
  Models/            统一标准数据模型 ProductDataRecord
  Reading/           读取结果和读取错误
  Sources/           具体数据源适配器
  Normalization/     ProductDataRecord 到 ProductFilterRow 的标准化
  Merging/           多个来源的 ProductDataRecord 合并
  Scenarios/         账户规模和止损距离测算场景展开
  Pipeline/          本地组合管线，串联读取、合并、展开
  Exporting/         CSV 导出
```

---

## 数据流向

```text
研究数据源
  → IProductDataSource.Read
  → ProductDataReadResult
  → ProductDataRecordMerger
  → ProductFilterScenarioExpander
  → ProductDataLocalCompositionPipeline
  → ProductFilterCsvExporter
  → ProductFilter CLI
  → calculated CSV
  → summary report
```

说明：

1. **ProductDataLocalCompositionPipeline** 当前只负责到未计算的 `ProductFilterRow`。
2. **ProductFilterCalculator** 不在 ProductData 中调用，它在 ProductFilter CLI 中执行。
3. **ProductFilter CLI** 才负责生成 `calculated.csv` 和 `summary report`。
4. **ProductData** 不生成 `Allowed / Caution / Rejected`。
5. **ProductData** 不生成交易建议。

---

## 各目录职责

### Abstractions

定义数据源适配器必须实现的接口 `IProductDataSource`，以及数据来源类型枚举 `ProductDataSourceType`。

所有具体数据源适配器都必须实现 `IProductDataSource`，返回 `ProductDataReadResult`。这样可以保证无论数据来自本地 HTML、本地 CSV 还是未来的 CTP 柜台，下游合并和展开逻辑都不需要修改。

### Models

定义统一标准数据模型 `ProductDataRecord`。

`ProductDataRecord` 是数据源适配器和合并器之间的通用语言。它包含合约规格、保证金、手续费、ATR、流动性等字段，但不包含公式计算字段（如 TickValue、MarginPerLot、RiskRate 等），也不包含 Allowed / Caution / Rejected 结论。

### Reading

定义读取结果 `ProductDataReadResult` 和读取错误 `ProductDataReadError`。

读取失败必须通过 `ProductDataReadError` 显式返回，不能用 0、false、Unknown 等默认值伪装成功。坏行不能进入 `Records`，必须留在 `Errors` 中，避免脏数据进入后续合并和展开流程。

### Sources

存放具体数据源适配器。

当前适配器包括：
- `TradingPlanetHtmlSource`：解析本地 HTML fixture；
- `LocalMarginFeeConfigSource`：读取本地保证金手续费 CSV；
- `LocalMarketStatSource`：读取本地行情统计 CSV。

Sources 只负责读取和基础校验，不计算公式字段，不判断 Allowed / Caution / Rejected，不生成交易建议。

### Normalization

负责把 `ProductDataRecord` 标准化为 `ProductFilterRow`。

`ProductDataNormalizer` 只做字段校验和映射，不做公式计算。`ProductFilterRow` 中的公式字段（TickValue、MarginPerLot、AtrMoneyPerLot、StopRiskMoney 等）在 Normalization 阶段仍然为 0，由下游 `ProductFilterCalculator` 统一计算。

### Merging

负责把多个来源的 `ProductDataRecord` 按 `ProductCode + ContractCode` 合并成完整记录。

`ProductDataRecordMerger` 会检测字段冲突。如果同一 key 的不同来源在 Exchange、Price、MarginRate 等字段上出现不一致，该 key 会进入 `ProductDataMergeError`，而不是静默覆盖。这是为了防止错误数据被掩盖。

### Scenarios

负责把一条完整的 `ProductDataRecord` 展开成多条 `ProductFilterRow`。

默认生成两个账户规模（10000 元和 20000 元）与五类止损距离（3 tick、5 tick、10 tick、0.5 ATR、1.0 ATR），因此一条记录展开为 10 行。账户规模是测算维度，不是固定字段名；未来增加 30000 / 50000 等账户时，应增加场景，不应修改模型字段。

### Pipeline

本地组合管线，串联读取、合并、展开三个步骤。

`ProductDataLocalCompositionPipeline` 只是 orchestration（编排），不做业务结论。它会收集读取错误、合并错误、展开错误，统一转换为 `ProductDataPipelineError`，但自己不判断 Allowed / Caution / Rejected，也不计算公式字段。

### Exporting

负责把 `ProductFilterRow` 导出为 CSV。

`ProductFilterCsvExporter` 只导出输入行，不负责筛选判断。导出的 CSV 可以用于人工审查，也可以作为 ProductFilter CLI 的输入。当前测试级别的导出只允许写入临时目录，不允许生成正式的 `product_filter_batch1.csv`。

---

## 当前已经完成的能力

1. 可以读取本地 HTML fixture。
2. 可以读取本地保证金手续费 CSV fixture。
3. 可以读取本地行情统计 CSV fixture。
4. 读取失败会返回 `ProductDataReadError`。
5. 坏行不会进入 `Records`。
6. 可以按 `ProductCode + ContractCode` 合并多来源记录。
7. 字段冲突不会静默覆盖。
8. 可以生成 10000 / 20000 两个账户规模。
9. 可以生成 3 tick / 5 tick / 10 tick / 0.5 ATR / 1.0 ATR 五类止损距离。
10. 可以把一条完整记录展开成 10 条 `ProductFilterRow`。
11. 可以通过本地组合管线完成测试 fixture 到 `ProductFilterRow` 的最小闭环。
12. 可以把测试行导出到临时 CSV。

---

## 当前明确不做的事情

1. 不联网。
2. 不抓取真实网页。
3. 不连接 CTP。
4. 不读取实时行情。
5. 不实现策略。
6. 不实现信号。
7. 不实现技术指标。
8. 不实现回测。
9. 不实现实盘。
10. 不自动下单。
11. 不生成交易建议。
12. 不判断某个品种是否可交易。
13. 不生成正式 `product_filter_batch1.csv`。
14. 不生成正式 `calculated CSV`。
15. 不生成正式 `summary report`。
16. 不手写 ProductFilter 公式字段。
17. 不手写 `Allowed / Caution / Rejected`。

---

## 后续扩展原则

1. 新的数据源适配器放在 `Sources`。
2. 新的数据源必须实现 `IProductDataSource`。
3. 新的数据源必须返回 `ProductDataReadResult`。
4. 解析失败不能用 0、false、Unknown 伪装成功。
5. 坏行不能进入 `Records`。
6. 多来源冲突必须进入 `ProductDataMergeError`。
7. 账户规模是数据维度，不是字段名。
8. 未来增加 30000 / 50000 账户时，应增加场景，不应修改模型字段。
9. 研究数据源和未来 CTP 实盘数据源只能替换输入，不能替换 `ProductFilter / TradePermission` 逻辑。
10. `ThirdPartyResearch` 必须 `NeedsReview = true`。
11. `CtpAccountActual` 以后才引入，当前不得伪造。
