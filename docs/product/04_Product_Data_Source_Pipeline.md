# 品种数据源管线

文件：`docs/product/04_Product_Data_Source_Pipeline.md`

---

## 1. 文档目的

本文档定义品种筛选所需数据的来源分层、标准化模型和处理流程。

核心原则：

```text
研究阶段数据源和实盘阶段数据源可以不同；
但所有数据源必须转换成同一个标准输入模型；
后续风险计算、品种筛选和交易许可必须使用同一套逻辑。
```

---

## 2. 为什么要区分数据源和计算逻辑

品种筛选需要的数据包括：

```text
合约规格；
价格；
保证金；
手续费；
ATR；
流动性；
数据日期；
数据来源。
```

这些数据在不同阶段来源不同。

研究阶段可以使用：

```text
第三方网站；
交易所公开信息；
行情软件导出；
本地 CSV 配置。
```

实盘前和实盘阶段应优先使用：

```text
CTP 柜台返回的账户相关参数；
期货公司实际保证金；
期货公司实际手续费；
真实行情源。
```

但是，不能因为数据来源不同，就写两套风险判断逻辑。

正确结构是：

```text
不同数据源
  ↓
数据源适配器
  ↓
标准化品种数据模型
  ↓
ProductFilter
  ↓
TradePermission
  ↓
Allowed / Caution / Rejected
```

---

## 3. 数据源分层

### 3.1 研究数据源

研究数据源用于初筛，不用于实盘下单。

允许来源：

```text
第三方手续费和保证金网站；
交易所公开页面；
交易所公告；
行情软件导出数据；
人工维护的本地 CSV。
```

第三方来源必须标记：

```text
DataSourceType = ThirdPartyResearch；
NeedsReview = true。
```

第三方数据只能回答：

```text
某个品种是否值得继续研究；
是否明显不适合小资金账户；
哪些品种需要进一步复核。
```

不能回答：

```text
是否可以实盘交易；
实际账户手续费是多少；
实际账户保证金是多少。
```

---

### 3.2 账户数据源

账户数据源用于实盘前复核和实盘阶段。

优先来源：

```text
CTP 柜台；
期货公司交易系统；
期货公司结算单；
账户实际成交记录。
```

账户数据源必须标记：

```text
DataSourceType = AccountActual；
NeedsReview = false，除非字段缺失或异常。
```

账户数据源优先级高于研究数据源。

---

### 3.3 行情数据源

行情数据源用于价格、ATR 和流动性统计。

允许来源：

```text
行情 API；
行情软件导出；
本地历史 K 线文件；
本地历史 Tick 文件。
```

第一阶段只需要支持研究级统计，不要求接实盘行情。

---

## 4. 交易星球数据的定位

交易星球手续费页面可以作为研究阶段第三方数据源。

它适合提供：

```text
品种代码；
合约代码；
现价；
成交量；
交易所标准保证金比例；
交易所标准保证金金额；
交易所标准手续费；
开仓手续费；
平昨手续费；
平今手续费；
开平合计手续费；
主力标记。
```

使用该来源时必须注意：

```text
它是第三方研究数据源；
页面中的保证金和手续费是交易所标准或页面计算结果；
不同期货公司会在交易所标准基础上加收；
真实账户手续费和保证金必须以期货公司或 CTP 柜台为准。
```

因此，从该网站生成的数据必须写入：

```text
DataSource = 交易星球手续费页面；
DataSourceType = ThirdPartyResearch；
NeedsReview = true。
```

---

## 5. CTP 数据的定位

CTP 柜台数据用于账户实际交易前的最终校验。

CTP 或期货公司账户数据适合提供：

```text
实际手续费；
实际保证金；
账户权益；
可用资金；
持仓保证金；
账户特定风控参数。
```

CTP 数据不得绕过已有逻辑。

它也必须转换成同一套标准字段：

```text
AccountEquity；
MarginRate；
MarginPerLot；
RoundTripFeePerLot；
Price；
Multiplier；
TickSize；
StopDistance；
SlippageTicks；
DataDate；
DataSource。
```

然后继续调用：

```text
ProductFilterCalculator；
TradePermissionEvaluator。
```

---

## 6. 标准化模型

所有数据源最终必须转换成品种筛选 CSV 模板字段。

模板文件：

```text
templates/product_filter_template.csv
```

核心字段：

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
SlippageTicks
TypicalAtr
StopDistance
AccountEquity
LiquidityLevel
BookContinuityLevel
RolloverClarity
DataDate
DataSource
```

公式字段仍由 ProductFilter 计算，不由数据源适配器手写：

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
Result
Reasons
```

---

## 7. 建议新增数据源元信息

为了区分研究数据和账户实际数据，后续可以扩展模板或内部模型，增加：

```text
DataSourceType
NeedsReview
```

推荐取值：

```text
DataSourceType:
- ThirdPartyResearch
- ExchangePublic
- BrokerTable
- CtpAccountActual
- MarketDataApi
- ManualConfig

NeedsReview:
- true
- false
```

第一阶段如果暂时不扩展 CSV 表头，也必须把这些信息写入 `DataSource` 或 `Reasons`。

---

## 8. 第一版自动采集建议

第一版不要直接做复杂爬虫和实盘 CTP。

第一版建议只实现数据源适配器框架：

```text
IProductDataSource
ProductDataRecord
ProductDataNormalizer
ProductFilterCsvExporter
```

第一批适配器：

```text
TradingPlanetHtmlSource       交易星球页面数据源；
LocalMarginFeeConfigSource    本地保证金手续费配置；
LocalMarketStatSource         本地行情统计文件。
```

后续再新增：

```text
CtpAccountFeeMarginSource     CTP 账户实际手续费保证金数据源；
MarketDataApiSource           行情 API 数据源。
```

所有适配器只负责产生标准化数据，不负责判断 Allowed / Caution / Rejected。

---

## 9. 处理流程

研究阶段流程：

```text
交易星球或其他研究数据源
  ↓
研究数据适配器
  ↓
标准化 product_filter_batch1.csv
  ↓
ProductFilter.Cli
  ↓
calculated.csv + summary.md
```

实盘前复核流程：

```text
CTP 柜台 / 期货公司实际账户数据
  ↓
账户数据适配器
  ↓
标准化 product_filter_account_actual.csv
  ↓
ProductFilter.Cli
  ↓
account_actual_calculated.csv + summary.md
```

二者区别只在数据来源。

二者不允许使用不同的风险判断逻辑。

---

## 10. 冲突处理

如果研究数据和 CTP 账户实际数据不一致：

```text
以 CTP 账户实际数据为准；
保留研究数据作为历史参考；
summary 中标记差异；
不得用研究数据覆盖账户实际数据。
```

示例：

```text
研究数据源显示保证金 9%，CTP 账户实际保证金 13%，则实盘前测算必须使用 13%。
```

---

## 11. 当前不做什么

当前阶段不做：

```text
不接实盘交易；
不下单；
不读取账户持仓；
不自动生成交易建议；
不因为研究数据 Allowed 就允许实盘；
不把第三方数据当作账户真实数据。
```

---

## 12. 当前结论

数据源可以分阶段演进。

但计算逻辑必须保持唯一：

```text
同一套 ProductFilter；
同一套 TradePermission；
同一套 Allowed / Caution / Rejected 语义。
```

研究阶段用第三方数据源提高效率。

实盘前用 CTP 或期货公司账户实际数据替换输入。

替换的是数据，不是逻辑。