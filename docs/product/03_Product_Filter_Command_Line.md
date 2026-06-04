# 品种筛选命令行工具

文件：`docs/product/03_Product_Filter_Command_Line.md`

---

## 1. 文档目的

本文档定义品种筛选命令行工具的输入、输出和边界。

命令行工具只负责：

```text
读取本地 CSV；
校验 CSV；
计算公式字段；
输出计算后的 CSV；
输出 Markdown 汇总报告。
```

命令行工具不负责：

```text
采集真实交易所数据；
联网抓取行情；
判断交易方向；
生成交易信号；
生成实盘建议。
```

---

## 2. 工具定位

`SmallFuturesLab.ProductFilter` 是品种筛选计算库。

命令行工具只是这个库的本地执行入口。

命令行工具不得重新实现业务公式。

命令行工具必须调用 `SmallFuturesLab.ProductFilter` 中已经实现的读取、校验、计算和汇总能力。

---

## 3. 推荐项目结构

允许新增：

```text
src/SmallFuturesLab.ProductFilter.Cli/

test/SmallFuturesLab.ProductFilter.Cli.Tests/
```

CLI 项目引用：

```text
SmallFuturesLab.ProductFilter
```

CLI 测试项目引用：

```text
SmallFuturesLab.ProductFilter.Cli
SmallFuturesLab.ProductFilter
```

---

## 4. 命令形式

第一版命令只支持一个子命令：

```text
product-filter run
```

建议执行形式：

```bash
dotnet run --project src/SmallFuturesLab.ProductFilter.Cli -- product-filter run --input data/product_filter/product_filter_batch1.csv --output data/product_filter/product_filter_batch1_calculated.csv --summary reports/product_filter_batch1_summary.md
```

参数：

```text
--input    输入 CSV 路径；
--output   计算后 CSV 输出路径；
--summary  Markdown 汇总输出路径。
```

---

## 5. 参数规则

必须提供：

```text
--input
--output
--summary
```

如果缺少任一参数，工具应：

```text
输出可读错误；
返回非 0 退出码；
不创建输出文件。
```

如果 `--input` 文件不存在，工具应：

```text
输出可读错误；
返回非 0 退出码；
不创建输出文件。
```

如果输出目录不存在，工具可以自动创建目录。

---

## 6. 处理流程

命令执行顺序：

```text
1. 解析命令行参数；
2. 检查 input 文件是否存在；
3. 读取 CSV；
4. 校验 CSV 表头和字段；
5. 如校验失败，输出错误并返回非 0；
6. 对每条记录执行品种筛选计算；
7. 写出计算后的 CSV；
8. 写出 Markdown summary；
9. 输出简短成功信息；
10. 返回 0。
```

---

## 7. 输出 CSV 要求

计算后的 CSV 表头必须与 `templates/product_filter_template.csv` 保持一致。

输出 CSV 应覆盖公式字段：

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

如果输入 CSV 中这些字段已有值，以计算结果为准。

---

## 8. Summary 要求

Markdown summary 内容由 `ProductFilterSummaryWriter` 生成。

CLI 不得自行拼接新的业务结论。

Summary 不得出现：

```text
推荐交易；
可以买入；
可以做多；
可以做空；
收益机会。
```

---

## 9. 错误输出要求

校验失败时，应输出所有错误，而不是只输出第一个错误。

每个错误至少包含：

```text
行号；
字段名；
错误原因。
```

---

## 10. 当前不做什么

第一版 CLI 不做：

```text
不做交互式输入；
不做配置文件；
不做联网采集；
不做行情读取；
不做数据库；
不做图表；
不做 Excel；
不做多命令体系；
不做真实品种数据内置；
不做交易建议。
```

---

## 11. 测试要求

CLI 测试至少覆盖：

```text
缺少 input 参数返回失败；
缺少 output 参数返回失败；
缺少 summary 参数返回失败；
input 文件不存在返回失败；
有效 CSV 可以生成 output CSV；
有效 CSV 可以生成 summary markdown；
无效 CSV 返回失败并输出所有错误；
输出 CSV 表头与模板一致；
输出内容包含 AccountEquity / RiskRate / Result；
summary 不包含交易建议措辞。
```

---

## 12. 完成标准

第一版 CLI 完成时，必须满足：

```text
dotnet test 通过；
命令行工具可以读取本地测试 CSV；
命令行工具可以输出计算后 CSV；
命令行工具可以输出 Markdown summary；
无效输入返回非 0；
没有新增行情、策略、回测、实盘或联网采集代码。
```

---

## 13. 当前结论

品种筛选 CLI 是本地研究工具，不是交易系统入口。

它的价值是把 CSV 采集表变成可重复、可审查、可自动化计算的研究产物。