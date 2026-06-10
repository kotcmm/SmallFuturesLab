# 品种数据采集任务说明

文件：`docs/product/02_Data_Collection_Task.md`

---

> **说明：本文档描述的是 CSV 批量采集和计算流程，属于未来阶段规划。**
>
> 当前代码已实现的是：交易星球单文件读取 → `ProductEvaluation` 测算 → CLI 打印。
>
> CSV 批量输入、多场景自动展开、公式字段批量计算等功能尚未实现。

---

## 1. 文档目的

本文档定义第一批候选品种的数据采集任务。

它用于指导人工或本地 AI 编码助手采集 `docs/product/01_Candidate_Product_Batch1.md` 中列出的品种数据。

本文档不输出品种筛选结论。

---

## 2. 当前状态

当前已实现的数据采集方式：

```text
1. 从交易星球下载手续费和保证金表格（HTML .xls 文件）；
2. 使用 TradingPlanetFileReader 解析为 Product 对象；
3. 使用 CLI 命令行工具运行 ProductEvaluation 测算；
4. 查看 CLI 输出结果。
```

尚未实现：

```text
CSV 批量输入和校验；
多止损场景自动展开；
多账户规模自动展开；
公式字段批量计算；
计算后 CSV 输出；
Markdown 汇总报告生成。
```

---

## 3. 采集范围

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

## 4. 账户规模建模

账户规模是测算维度，不是固定字段名。

默认第一轮账户规模：

```text
10,000 元；
20,000 元。
```

如果后续需要测算 30,000 元、40,000 元、50,000 元，只需要增加对应测算记录，不修改领域模型。

---

## 5. 字段分组

### 5.1 外部采集字段

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
RoundTripFee
DataDate
DataSource
```

这些字段不得凭空猜测。

### 5.2 初始假设字段

这些字段允许先使用统一假设，但必须明确记录：

```text
SlippageTicks
StopTicks
AccountEquity
```

第一轮默认滑点：

```text
优先采集品种：SlippageTicks = 2
谨慎观察品种：SlippageTicks = 2 或 3
排除对照品种：SlippageTicks = 2 到 5
```

StopTicks 至少生成三类：

```text
3 tick
5 tick
10 tick
```

AccountEquity 第一轮至少生成两类：

```text
10000
20000
```

### 5.3 公式计算字段

这些字段由公式计算，不应人工手填：

```text
TickValue
MarginPerLot
StopRiskMoney
SlippageMoney
CostMoney
TotalRiskMoney
RiskRate
MarginRateOfEquity
CostRatio
```

公式以 `docs/04_Product_Evaluation_Formula.md` 为准。

### 5.4 结论字段

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

Reasons 必须写清楚原因，不能只写"通过"或"不适合"。

---

## 6. 每个品种的行数要求

每个品种至少生成：

```text
3 个止损场景 × 2 个账户规模 = 6 行测算记录。
```

止损场景：

```text
3 tick 止损；
5 tick 止损；
10 tick 止损。
```

账户规模：

```text
AccountEquity = 10000；
AccountEquity = 20000。
```

---

## 7. 数据来源记录要求

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

如果使用第三方资料，必须说明该记录需要后续复核。

---

## 8. 验收检查

采集完成后，必须检查：

```text
所有必填字段非空；
所有数值字段可解析为数字；
AccountEquity > 0；
Result 只包含 Allowed / Caution / Rejected；
每条记录都有 DataDate；
每条记录都有 DataSource；
Reasons 非空；
没有未经说明的数据来源。
```

---

## 9. 当前不做什么

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

## 10. 当前结论

当前阶段优先使用交易星球下载文件 + CLI 进行单文件测算。

CSV 批量采集和自动化计算属于未来阶段，待 `ProductEvaluation` 公式稳定后再扩展。
