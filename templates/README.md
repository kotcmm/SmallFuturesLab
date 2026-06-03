# Templates

本目录存放研究阶段使用的数据模板。

模板只定义字段结构，不代表实际筛选结论。

---

## product_filter_template.csv

文件：`templates/product_filter_template.csv`

用途：

```text
记录候选期货品种的合约规格、风险测算、成本测算、流动性等级和账户适配结论。
```

使用原则：

```text
1. 不填入未经确认的数据；
2. 每条记录必须包含 DataDate；
3. 每条记录必须包含 DataSource；
4. Result10k 和 Result20k 只能使用 Allowed / Caution / Rejected；
5. Reasons 必须写明结论原因；
6. 该模板不是策略规则；
7. 该模板不判断行情方向；
8. 该模板不代表实盘许可。
```

字段含义以 `docs/05_Product_Filter.md` 为准。

如果模板字段和文档冲突，先修改文档，再修改模板。
