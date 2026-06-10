# AGENTS.md

## 1. 文档优先级

编码前必须阅读：

```text
README.md
docs/00_Project_Principles.md
docs/01_Account_Constraints.md
docs/02_Research_Roadmap.md
docs/03_Operation_Sketch.md
docs/04_Product_Evaluation_Formula.md
docs/05_Product_Filter.md
docs/product/01_Candidate_Product_Batch1.md
docs/product/02_Data_Collection_Task.md
docs/product/03_Product_Filter_Command_Line.md
templates/README.md
```

规则归属：

```text
docs/ = 业务规则
AGENTS.md = 工程规则
```

文档冲突时，先停下，先反馈，不要自行修改业务文档。

不要在本文件定义公式、阈值、交易规则或风险规则。

---

## 2. 文档变更职责

业务文档由用户和 ChatGPT 维护。

本地 AI 编码助手主要负责编码实现，不主动新增、重写或修订 `docs/` 下的业务文档。

如果编码时发现以下情况：

```text
业务文档不清楚
业务文档之间冲突
代码实现需要突破文档边界
测试预期和文档不一致
需要新增业务规则、公式、阈值或阶段边界
```

应停止编码，并在最终输出中说明需要先处理的文档问题。

只有任务明确要求修改文档时，才可以修改对应 Markdown 文件。

---

## 3. 当前任务边界

当前允许实现：

```text
docs/04_Product_Evaluation_Formula.md 中定义的品种测算公式和阈值判断；
docs/05_Product_Filter.md 中定义的品种筛选计算逻辑；
docs/product/03_Product_Filter_Command_Line.md 中定义的命令行工具。
```

当前不允许实现：

```text
策略、行情指标、回测、执行、实盘、信号生成、自动下单、CTP 连接、联网抓取、交易建议。
```

当前允许为了研究数据源适配器读取本地测试文件或测试 HTML fixture。

未经单独任务明确要求，不要实现真实联网爬虫。

---

## 4. 本地 AI 编码助手协作流程

默认远程仓库名：`origin`

开始任务前：`git fetch origin`，基于 `origin/main` 创建任务分支，清理本地已合并或已废弃的无效分支。

开发约束：不直接在 main 分支上修改代码，分支名应与任务说明一致，用户希望尽量少手动操作。

本地 AI 编码助手应主动完成：拉取、建分支、实现、测试、提交、推送、创建 Pull Request。

当前阶段默认验收命令：`dotnet test`

验收通过后：提交 commit、推送到 origin、创建 Pull Request。

如果本地已安装 GitHub CLI，使用 `gh pr create`。

如果没有 `gh`，最终输出必须给出可复制的 PR 创建命令或浏览器创建链接。

PR 描述必须包含：实现范围、测试结果、未实现范围、是否遵守 TDD、是否遵守一类一文件、是否遵守中文 XML 注释约定、是否修改了业务文档。

不允许因为追求速度而绕过：AGENTS.md、docs/ 文档、测试、工程规范。

---

## 5. 技术栈与项目结构

技术栈：C#、.NET 10 (net10.0)、xUnit。

当前生产项目：

```text
SmallFuturesLab.Core
SmallFuturesLab.TradingPlanet
SmallFuturesLab.Cli
```

当前测试项目：

```text
SmallFuturesLab.Core.Tests
SmallFuturesLab.TradingPlanet.Tests
```

更换技术栈或新增其他顶层模块前，先更新文档。

---

## 6. TDD 开发约束

开发必须遵循 TDD：先写测试 → 运行确认失败 → 写最小实现 → 运行确认通过 → 必要时重构 → 重构后再运行测试。

禁止先写实现再补测试。

新增业务行为、边界条件或异常处理时，必须先新增对应测试。

修复缺陷时，必须先补一个能复现缺陷的失败测试，再修复实现。

如果因为环境限制无法先看到失败测试，最终输出必须明确说明原因。

---

## 7. 实现风格

优先：小类、纯计算、不可变输入、业务命名清晰、无外部副作用。

风险、品种筛选、命令行工具和数据源适配器不得：读取实时行情、访问网络（除非任务明确要求）、写数据库、调用交易接口、连接 CTP 实盘柜台、依赖当前时间、自动生成交易建议。

---

## 8. 命名约束

避免模糊命名：Manager、Helper、Processor、Service、Engine、Strategy、Signal、Alpha、Predictor。

优先使用相关文档中的业务命名。

---

## 9. 生产代码注释约定

新增或修改 `src/` 下的生产代码时：

```text
public 类型必须有中文 XML 注释
public 属性必须有中文 XML 注释
public 方法必须有中文 XML 注释
public enum 及 enum 成员必须有中文 XML 注释
```

领域模型优先使用属性展开式 `record` 或 `class`，以便逐属性说明业务语义。

一个 `public class` / `record` / `interface` / `enum` 使用一个独立 `.cs` 文件。

文件名必须与 public 类型名一致。

如果使用英文术语，应在附近给出中文解释。

---

## 10. 测试要求

当前默认测试重点覆盖：标准示例、公式字段计算、Allowed 结果、Caution 结果、Rejected 结果、交易星球文件读取、坏数据行不进入 Products。

输入验证、CLI 参数深度校验、配置文件校验，后续在配置文件阶段单独设计，不作为当前默认测试要求。

测试名称应描述业务行为。

当前阶段允许使用 `double`。

浮点断言必须使用容差。

---

## 11. 文档与代码冲突处理

代码和文档冲突时：

```text
1. 停止编码
2. 记录冲突点
3. 在最终输出或 PR 描述中说明问题
4. 等待文档更新后再继续实现
```

不要让代码悄悄改变项目规则。

不要为了让测试通过而反向修改业务文档。

---

## 12. 完成标准

```text
SmallFuturesLab.Core 可以编译
SmallFuturesLab.Core.Tests 全部通过
SmallFuturesLab.TradingPlanet 可以编译
SmallFuturesLab.TradingPlanet.Tests 全部通过
SmallFuturesLab.Cli 可以编译
实现与 docs/04、docs/05、docs/product/03 一致
没有策略、行情指标、回测、执行、实盘或交易建议代码
已创建 Pull Request
```
