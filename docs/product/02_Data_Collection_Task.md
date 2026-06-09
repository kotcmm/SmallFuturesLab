# 品种数据采集任务说明

文件：`docs/product/02_Data_Collection_Task.md`

---

## 1. 文档目的

本文档定义第一批候选品种的数据采集任务。

它用于指导人工或本地 AI 编码助手采集 `docs/product/01_Candidate_Product_Batch1.md` 中列出的品种数据，并填写到 `templates/product_filter_template.csv` 的副本中。

本文档不输出品种筛选结论。

本文档只定义：

```text
采集哪些字段；
哪些字段来自外部数据；
哪些字段由公式计算；
哪些字段由人工分级；
账户规模如何作为测算维度；
采集完成后如何验收。
```

---

## 2. 正式采集前闸门

本文档描述最终采集任务，但正式创建：

```text
data/product_filter/product_filter_batch1.csv
```

之前，必须先完成：

```text
docs/product/05_Batch1_Data_Collection_Gate.md
```

中的采集前检查清单。

如果检查清单未完成，不应创建正式 batch1 数据文件，不应生成 calculated CSV，也不应生成 summary report。

---

## 3. 输入文件

采集任务输入：

```text
docs/05_Product_Filter.md
docs/product/01_Candidate_Product_Batch1.md
templates/product_filter_template.csv
```

输出文件建议另存为：

```text
data/product_filter/product_filter_batch1.csv
```

如果 `data/product_filter/` 目录不存在，可以在执行采集任务时创建。

---

## 4. 采集范围

第一批采集范围以 `docs/product/01_Candidate_Product_Batch1.md` 为准。

包括三类：

```text
优先采集品种；
谨慎观察品种；
排除对照品种。
```

不允许临时加入新一批品种。

如果发现需要新增品种，先修改候选品种文档，再执行采集。

---

## 5. 账户规模建模

账户规模是测算维度，不是固定字段名。

模板使用：

```text
AccountEquity
```

表示当前这一行测算所使用的账户权益。

不要在模型或 CSV 中固化：

```text
RiskRate10k
RiskRate20k
MarginRate10k
MarginRate20k
Result10k
Result20k
```

原因：

```text
1 万和 2 万只是当前默认测算场景；
未来可能增加 3 万、4 万、5 万或其他账户规模；
账户规模变化不应该导致代码模型、CSV 表头和测试大面积修改。
```

默认第一轮账户规模：

```text
10,000 元；
20,000 元。
```

如果后续需要测算 30,000 元、40,000 元、50,000 元，只需要为对应 `AccountEquity` 增加记录行，不修改字段结构。

---

## 6. 字段分组

模板字段分为四类。

### 6.1 外部采集字段

这些字段必须从交易所、期货公司、行情软件或其他明确来源获得：

```text
Exchange
ProductName
ProductCode
ContractCode
Price
Multiplier
TickSize
MarginRate
RoundTripFeePerLot
TypicalAtr
LiquidityLevel
BookContinuityLevel
RolloverClarity
DataDate
DataSource
```

这些字段不得凭空猜测。

---

### 6.2 初始假设字段

这些字段允许先使用统一假设，但必须明确记录：

```text
SlippageTicks
StopDistance
AccountEquity
```

第一轮默认滑点：

```text
优先采集品种：SlippageTicks = 2
谨慎观察品种：SlippageTicks = 2 或 3
排除对照品种：SlippageTicks = 2 到 5
```

StopDistance 至少生成五类：

```text
3 tick
5 tick
10 tick
0.5 ATR
1.0 ATR
```

AccountEquity 第一轮至少生成两类：

```text
10000
20000
```

---

### 6.3 公式计算字段

这些字段由公式计算，不应人工手填：

```text
TickValue
MarginPerLot
AtrMoneyPerLot
StopRiskMoney
SlippageMoney
CostMoney
TotalRiskMoney
RiskRate
MarginRateOfEquity
CostRatio
```

公式以 `docs/05_Product_Filter.md` 和 `docs/04_Trade_Permission_Pipeline.md` 为准。

---

### 6.4 结论字段

这些字段必须基于测算结果填写：

```text
Result
Reasons
```

Result 只能使用：

```text
Allowed
Caution
Rejected
```

Reasons 必须写清楚原因，不能只写“通过”或“不适合”。

---

## 7. 每个品种的行数要求

每个品种至少生成：

```text
5 个止损场景 × 2 个账户规模 = 10 行测算记录。
```

止损场景：

```text
3 tick 止损；
5 tick 止损；
10 tick 止损；
0.5 ATR 止损；
1.0 ATR 止损。
```

账户规模：

```text
AccountEquity = 10000；
AccountEquity = 20000。
```

如果后续加入新的账户规模，例如 30,000 元，只增加对应行：

```text
5 个止损场景 × 3 个账户规模 = 15 行测算记录。
```

不修改 CSV 表头，不修改领域模型。

---

## 8. 采集顺序

按以下顺序执行：

```text
1. 完成 docs/product/05_Batch1_Data_Collection_Gate.md 中的采集前检查清单；
2. 复制 templates/product_filter_template.csv；
3. 保存为 data/product_filter/product_filter_batch1.csv；
4. 按候选品种清单逐个填写外部采集字段；
5. 为每个品种生成 StopDistance 场景；
6. 为每个 StopDistance 场景生成 AccountEquity 场景；
7. 计算所有公式字段；
8. 使用交易许可逻辑得出 Result；
9. 填写 Reasons；
10. 检查 DataDate 和 DataSource；
11. 按 AccountEquity 汇总候选白名单、谨慎观察列表和排除列表。
```

---

## 9. 数据来源记录要求

每条记录必须有 `DataDate` 和 `DataSource`。

DataSource 应写明来源类型，例如：

```text
交易所官网合约文本；
交易所保证金或手续费公告；
期货公司交易软件；
行情软件；
人工统计；
第三方资料临时参考。
```

如果使用第三方资料，Reasons 中必须说明该记录需要后续复核。

---

## 10. 人工分级口径

LiquidityLevel、BookContinuityLevel、RolloverClarity 先允许人工分级。

只允许以下值：

```text
Good
Medium
Poor
Unknown
```

含义：

```text
Good    = 良好；
Medium  = 一般；
Poor    = 较差；
Unknown = 暂无数据。
```

如果任一字段为 `Poor` 或 `Unknown`，该记录不能输出 `Allowed`。

---

## 11. 验收检查

采集完成后，必须检查：

```text
所有必填字段非空；
所有数值字段可解析为数字；
AccountEquity > 0；
Result 只包含 Allowed / Caution / Rejected；
每条记录都有 DataDate；
每条记录都有 DataSource；
每个品种至少覆盖 5 个止损场景；
每个品种至少覆盖 10000 和 20000 两个账户规模；
Reasons 非空；
没有未经说明的数据来源；
没有把 Unknown 当作 Allowed 的核心依据。
```

---

## 12. 当前不做什么

本任务不做：

```text
不做行情判断；
不做策略信号；
不做收益回测；
不做参数优化；
不做实盘建议；
不新增品种；
不修改风险许可公式；
不修改品种筛选规则。
```

---

## 13. 后续输出

采集完成后，应形成：

```text
data/product_filter/product_filter_batch1.csv
reports/product_filter_batch1_summary.md
```
