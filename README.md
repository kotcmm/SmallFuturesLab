# SmallFuturesLab

SmallFuturesLab 是一个独立的小资金期货生存研究项目。

它研究的不是“怎样快速赚钱”，而是：

```text
1 万到 2 万账户，怎样在期货市场里避免被一手合约、杠杆、手续费、滑点、连续亏损和主观失控淘汰。
```

本项目当前不设计最终策略，也不追求收益率结论。

---

## 1. 当前定位

当前阶段只做五件事：

```text
账户约束
交易许可流水线
品种筛选
周期筛选
风控底线
```

只有这五件事清楚之后，才讨论候选交易方法。

---

## 2. 核心原则

```text
小资金策略不是大资金策略的缩小版。

本项目不追求预测正确，
而是追求错误可承受。

保证金够开仓，不等于账户承受得起这笔交易。
```

---

## 3. 当前阶段默认形态

为了避免一开始就发散，当前阶段先按下面的交易形态研究：

```text
只做 1 手
优先日内
不研究隔夜
不研究加仓
不研究高频盘口
每天最多 1 到 3 笔
只研究少数活跃品种
每笔交易入场前必须知道 1R 是多少钱
```

这不是最终策略规则，只是当前研究边界。

---

## 4. 明确不做

当前阶段不研究：

```text
网格
摊平
扛单
重仓翻倍
亏损后加仓回本
每天数十笔高频交易
依赖主观盘口感觉的交易
需要专职盯盘才能执行的交易
```

---

## 5. 外部案例使用原则

外部交易案例只能用于提取：

```text
风险约束
执行纪律
品种筛选思路
交易心理警示
```

不能用于复制：

```text
收益率
仓位
交易频率
主观经验
冠军故事
```

---

## 6. 文档结构

```text
docs/00_Project_Principles.md                  项目最高原则
docs/01_Account_Constraints.md                 账户和风险约束
docs/02_Research_Roadmap.md                    研究路线
docs/03_Operation_Sketch.md                    未来操作大概长什么样
docs/04_Trade_Permission_Pipeline.md           交易许可流水线
docs/05_Product_Filter.md                      品种筛选
docs/product/01_Candidate_Product_Batch1.md    第一批候选品种与数据采集口径
docs/product/02_Data_Collection_Task.md        品种数据采集任务说明
docs/product/03_Product_Filter_Command_Line.md 品种筛选命令行工具
docs/product/05_Batch1_Data_Collection_Gate.md Batch1 真实数据采集前检查清单
```

---

## 7. 本地工具

### 7.1 品种筛选 CLI

品种筛选 CLI 用于把本地 CSV 采集表转换成可审查的计算结果。

它只做：

```text
读取本地 CSV
校验 CSV
计算公式字段
输出计算后的 CSV
输出 Markdown summary
```

它不做：

```text
不联网
不采集交易所数据
不读取行情
不判断交易方向
不生成交易信号
不生成实盘建议
```

运行示例：

```bash
dotnet run --project src/SmallFuturesLab.ProductFilter.Cli -- product-filter run --input data/product_filter/product_filter_batch1.csv --output data/product_filter/product_filter_batch1_calculated.csv --summary reports/product_filter_batch1_summary.md
```

输入 CSV 表头以此模板为准：

```text
templates/product_filter_template.csv
```

输出文件建议：

```text
data/product_filter/product_filter_batch1_calculated.csv
reports/product_filter_batch1_summary.md
```

这些输出只表示：

```text
当前账户规模下是否允许进入后续周期研究；
是否需要谨慎观察；
是否因为账户、成本、保证金或流动性原因排除。
```

这些输出不表示：

```text
可以实盘交易；
可以买入；
可以做多；
可以做空；
存在收益机会。
```

---

## 8. 当前下一步

ProductData 模块已经完成测试 fixture 到 ProductFilterRow 的最小数据闭环；正式采集 batch1 真实数据前，应先阅读 `src/SmallFuturesLab.ProductData/README.md`，确认数据流向、模块边界和禁止事项。

正式创建 batch1 真实数据文件前，必须先完成 `docs/product/05_Batch1_Data_Collection_Gate.md` 中的检查清单。

当前下一步是采集第一批品种的真实合约规格、保证金、手续费、ATR 和流动性信息。

采集结果应保存为：

```text
data/product_filter/product_filter_batch1.csv
```

然后使用品种筛选 CLI 生成：

```text
data/product_filter/product_filter_batch1_calculated.csv
reports/product_filter_batch1_summary.md
```

没有 DataDate 和 DataSource 的记录，不进入正式筛选表。

---

## 9. 当前结论

SmallFuturesLab 的起点不是信号，而是账户约束。

先证明某个交易行为不会让小资金账户快速死亡，再讨论它有没有研究价值。
