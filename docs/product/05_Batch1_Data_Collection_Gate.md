# Batch1 真实数据采集前检查清单

文件：`docs/product/05_Batch1_Data_Collection_Gate.md`

---

## 1. 文档目的

本文件不是采集结果，不是筛选结论，也不是交易建议。

本文件用于在正式创建真实 batch1 数据文件前，冻结以下内容：

```text
数据源允许范围；
字段采集口径；
字段验收标准；
第三方数据标记规则；
正式数据文件创建条件；
当前仍然禁止输出的内容。
```

本文档的核心作用是建立一道采集前闸门：

```text
先确认数据从哪里来、怎么记录、怎么复核，
再创建正式 batch1 数据文件。
```

---

## 2. 当前阶段状态

当前已经完成的是：

```text
研究数据进入 ProductFilter 输入层的测试闭环。
```

当前闭环为：

```text
测试 fixture
→ IProductDataSource.Read
→ ProductDataReadResult
→ ProductDataRecordMerger
→ ProductFilterScenarioExpander
→ ProductDataLocalCompositionPipeline
→ ProductFilterCsvExporter 临时导出
```

这个闭环说明：

```text
数据读取、错误隔离、多来源合并、场景展开、临时导出已经能在测试数据上跑通。
```

但它仍然不代表：

```text
已经完成真实品种筛选；
已经判断某个品种可以交易；
已经生成实盘建议；
已经可以跳过数据复核。
```

---

## 3. 采集前必须满足的条件

正式开始 batch1 真实数据采集前，必须满足以下条件：

```text
候选品种范围已冻结；
候选品种范围来自 docs/product/01_Candidate_Product_Batch1.md；
不允许临时增加新一批品种；
如果要新增候选品种，必须先修改候选品种文档；
每个字段的数据来源类型已经明确；
第三方研究数据的复核规则已经明确；
ProductData 模块的数据流向已经确认；
ProductFilter CLI 的输入输出边界已经确认；
用户明确确认可以开始真实 batch1 数据采集。
```

如果以上条件未全部满足，不创建正式数据文件。

---

## 4. 候选品种范围

第一批真实数据采集只允许覆盖：

```text
docs/product/01_Candidate_Product_Batch1.md
```

中已有的候选品种。

候选品种分为三类：

```text
优先采集品种；
谨慎观察品种；
排除对照品种。
```

本文件不新增品种。

如果后续发现需要新增品种，必须先修改候选品种文档，再执行数据采集。

---

## 5. 允许使用的数据源

batch1 真实数据采集允许使用以下数据源：

```text
交易所公开合约规格资料；
交易所公开手续费、保证金公告；
期货公司公开或软件导出的手续费、保证金表；
行情软件导出的历史统计数据；
人工统计的 ATR、成交量、持仓量、流动性等级；
第三方研究资料，例如交易星球手续费页面，但只能作为 ThirdPartyResearch；
本地人工维护 CSV，但必须记录 DataDate 和 DataSource。
```

不同数据源可以并存，但进入 ProductFilter 前必须转换为统一的 `ProductDataRecord`，再走同一套读取、合并、展开和筛选流程。

---

## 6. 禁止使用的数据源或行为

本阶段禁止：

```text
不联网抓取真实网页；
不连接 CTP；
不读取实时行情；
不读取实盘账户数据；
不调用交易接口；
不写数据库；
不使用未经说明来源的数据；
不凭空猜测保证金、手续费、价格、ATR；
不把第三方研究数据标记为 CtpAccountActual；
不用研究数据覆盖未来的账户实际数据；
不通过本任务生成交易建议。
```

注意：

```text
研究阶段可以使用第三方资料作为临时参考，
但必须明确标记、保留来源，并进入后续复核流程。
```

---

## 7. 字段采集口径

### 7.1 Exchange

交易所字段，必须来自合约资料或数据源原始记录。

不能为空。

### 7.2 ProductName

品种名称，必须与候选品种文档或数据源原始记录一致。

不能为空。

### 7.3 ProductCode

品种代码，必须明确到品种级别，例如 MA、RB、M 等。

不能为空。

### 7.4 ContractCode

合约代码，必须明确到具体合约。

不能为空。

### 7.5 Price

典型价格，必须有日期和来源。

Price 不能凭空估算。可以来自行情软件导出、人工统计记录或其他明确来源。

### 7.6 Multiplier

合约乘数，必须来自交易所合约规格或可追溯的合约资料。

不能凭经验填写。

### 7.7 TickSize

最小变动价位，必须来自交易所合约规格或可追溯的合约资料。

不能凭经验填写。

### 7.8 MarginRate

保证金比例，必须有明确来源。

允许来源包括交易所公告、期货公司公告、期货公司软件导出或人工维护表，但必须记录 `DataSource`。

不允许猜测。

### 7.9 RoundTripFeePerLot

单手开平总手续费估计，必须说明是否包含：

```text
开仓；
平昨；
平今。
```

如果无法拆分开仓、平昨、平今，必须在来源说明中记录原因。

### 7.10 OpenFeePerLot

开仓手续费。

如果来源能拆分手续费，应单独填写。若无法拆分，应说明原因，并确保 `RoundTripFeePerLot` 的口径可追溯。

### 7.11 CloseYesterdayFeePerLot

平昨手续费。

如果来源能拆分手续费，应单独填写。若无法拆分，应说明原因。

### 7.12 CloseTodayFeePerLot

平今手续费。

如果来源能拆分手续费，应单独填写。若无法拆分，应说明原因。

### 7.13 TypicalAtr

典型 ATR，必须说明统计周期和数据日期。

例如应记录：

```text
使用哪个合约；
使用多少周期；
使用哪段日期；
ATR 的计算口径。
```

本字段不能凭感觉填写。

### 7.14 Volume

成交量，必须说明统计日期或统计区间。

允许为 0，但必须是有限数字且不能为负数。

### 7.15 OpenInterest

持仓量，必须说明统计日期或统计区间。

允许为 0，但必须是有限数字且不能为负数。

### 7.16 LiquidityLevel

流动性等级，只允许：

```text
Good
Medium
Poor
Unknown
```

如果人工分级，必须说明依据。

### 7.17 BookContinuityLevel

盘口连续性等级，只允许：

```text
Good
Medium
Poor
Unknown
```

如果人工分级，必须说明依据。

### 7.18 RolloverClarity

主力合约换月清晰度，只允许：

```text
Good
Medium
Poor
Unknown
```

如果人工分级，必须说明依据。

### 7.19 DataDate

数据日期，不能为空。

DataDate 表示该条数据对应的采集日期、统计日期或来源发布日期。

### 7.20 DataSource

数据来源，不能为空。

DataSource 必须能让后续复核者知道数据从哪里来。

### 7.21 DataSourceType

数据来源类型。

研究阶段可使用：

```text
ThirdPartyResearch
ExchangePublic
BrokerTable
MarketDataApi
ManualConfig
```

当前不得使用：

```text
CtpAccountActual
```

CtpAccountActual 以后才用于实盘前复核和实盘阶段。

### 7.22 NeedsReview

是否需要复核。

第三方研究数据必须：

```text
NeedsReview = true
```

---

## 8. 字段验收标准

正式数据进入 ProductFilter 前，至少必须通过以下检查：

```text
ProductCode 非空；
ContractCode 非空；
Price 是有限数字且 > 0；
Multiplier 是有限数字且 > 0；
TickSize 是有限数字且 > 0；
MarginRate 是有限数字且 > 0；
RoundTripFeePerLot 是有限数字且 > 0；
TypicalAtr 是有限数字且 > 0；
Volume 是有限数字且 >= 0；
OpenInterest 是有限数字且 >= 0；
LiquidityLevel 只能是 Good / Medium / Poor / Unknown；
BookContinuityLevel 只能是 Good / Medium / Poor / Unknown；
RolloverClarity 只能是 Good / Medium / Poor / Unknown；
DataDate 非空；
DataSource 非空；
第三方研究数据 NeedsReview 必须为 true；
不允许出现没有来源说明的数据；
不允许出现公式字段手填；
不允许出现 Result 手填；
不允许出现交易建议措辞。
```

---

## 9. 第三方数据标记规则

交易星球等第三方页面可以作为研究阶段参考数据源。

但必须遵守以下规则：

```text
第三方数据源必须标记为 ProductDataSourceType.ThirdPartyResearch；
第三方数据源必须 NeedsReview = true；
第三方数据不能作为 CtpAccountActual；
第三方数据不能作为最终账户实际数据；
第三方数据进入 ProductFilter 前必须经过 ProductData 读取、合并、展开流程；
如果同一字段存在多个来源且冲突，不能静默覆盖，必须进入合并错误。
```

第三方资料的作用是研究阶段辅助，不是实盘确认依据。

---

## 10. 真实数据文件创建条件

在以下条件满足前，不允许创建正式文件：

```text
用户确认开始真实 batch1 数据采集；
候选品种清单已确认；
字段口径已确认；
数据来源清单已确认；
第三方数据复核规则已确认；
ProductDataLocalCompositionPipeline 的测试已通过；
ProductFilter CLI 的输入输出边界已确认。
```

正式文件包括：

```text
data/product_filter/product_filter_batch1.csv
data/product_filter/product_filter_batch1_calculated.csv
reports/product_filter_batch1_summary.md
```

其中：

```text
product_filter_batch1.csv 是 ProductFilter CLI 的输入文件；
product_filter_batch1_calculated.csv 必须由 ProductFilter CLI 生成；
product_filter_batch1_summary.md 必须由 ProductFilter CLI 生成。
```

---

## 11. 采集后必须执行的流程

真实研究数据采集后，必须走以下流程：

```text
真实研究数据
→ ProductData source
→ ProductDataReadResult
→ ProductDataRecordMerger
→ ProductFilterScenarioExpander
→ ProductDataLocalCompositionPipeline
→ ProductFilterCsvExporter
→ ProductFilter CLI
→ calculated CSV
→ summary report
```

流程约束：

```text
不允许手写 calculated CSV；
不允许手写 summary report；
不允许手写 Allowed / Caution / Rejected；
不允许手写 RiskRate / MarginRateOfEquity / CostRatio 等公式字段；
不允许绕过 ProductFilter CLI 生成正式计算结果。
```

---

## 12. 当前仍然不输出的内容

当前仍然不输出：

```text
交易建议；
买卖方向；
入场信号；
止盈止损策略；
收益预测；
回测结果；
实盘建议；
某个品种一定可以交易的判断；
某个品种未来会盈利的判断。
```

即使后续生成 `product_filter_batch1_calculated.csv` 和 summary report，它们也只表示：

```text
某个品种是否允许进入后续周期研究；
是否需要谨慎观察；
是否因为账户、成本、保证金或流动性原因排除。
```

它们不表示：

```text
可以实盘交易；
可以开仓；
存在收益机会。
```
