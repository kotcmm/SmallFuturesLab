# AGENTS.md

## 1. 文档优先级

编码前必须阅读：

```text
README.md
docs/00_Project_Principles.md
docs/01_Account_Constraints.md
docs/02_Research_Roadmap.md
docs/03_Operation_Sketch.md
docs/04_Trade_Permission_Pipeline.md
docs/05_Product_Filter.md
docs/product/01_Candidate_Product_Batch1.md
docs/product/02_Data_Collection_Task.md
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

## 3. 当前实现范围

当前允许实现：

```text
docs/04_Trade_Permission_Pipeline.md 中定义的交易许可模块；
docs/05_Product_Filter.md 中定义的品种筛选计算与校验；
docs/product/02_Data_Collection_Task.md 中定义的数据文件校验、计算和汇总输出。
```

当前不允许实现：

```text
策略
行情
指标
回测
执行
实盘
优化
信号生成
自动采集外部数据
联网抓取交易所数据
根据行情生成交易建议
```

---

## 4. 本地 AI 编码助手协作流程

默认远程仓库名：

```text
origin
```

开始任务前：

```text
git fetch origin
基于 origin/main 创建任务分支
清理本地已合并或已废弃的无效分支
```

开发约束：

```text
不直接在 main 分支上修改代码
分支名应与任务说明一致
用户希望尽量少手动操作
```

本地 AI 编码助手应主动完成：

```text
拉取
建分支
实现
测试
提交
推送
创建 Pull Request
```

当前阶段默认验收命令：

```text
dotnet test
```

如果仓库后续增加解决方案文件，优先使用文档指定的解决方案级验收命令。

验收通过后：

```text
提交 commit
推送到 origin
创建 Pull Request
```

如果本地已安装 GitHub CLI，使用：

```text
gh pr create
```

如果没有 `gh`，最终输出必须给出可复制的 PR 创建命令或浏览器创建链接。

PR 描述必须包含：

```text
实现范围
测试结果
未实现范围
是否遵守 TDD
是否遵守一类一文件
是否遵守中文 XML 注释约定
是否修改了业务文档
```

不允许因为追求速度而绕过：

```text
AGENTS.md
docs/ 文档
测试
工程规范
```

用户最好只需要启动一次本地 AI 编码助手命令，然后把 PR 链接交给 ChatGPT review。

---

## 5. 技术栈与项目结构

技术栈：

```text
C#
.NET 10 (TargetFramework: net10.0)
xUnit
```

当前允许结构：

```text
src/
  SmallFuturesLab.Risk/
  SmallFuturesLab.ProductFilter/

test/
  SmallFuturesLab.Risk.Tests/
  SmallFuturesLab.ProductFilter.Tests/
```

更换技术栈或新增其他顶层模块前，先更新文档。

---

## 6. TDD 开发约束

开发必须遵循 TDD。

顺序：

```text
1. 先写测试
2. 运行测试，确认测试失败
3. 写最小实现
4. 运行测试，确认测试通过
5. 必要时重构
6. 重构后再次运行测试
```

禁止先写实现再补测试。

新增业务行为、边界条件或异常处理时，必须先新增对应测试。

修复缺陷时，必须先补一个能复现缺陷的失败测试，再修复实现。

如果因为环境限制无法先看到失败测试，最终输出必须明确说明原因。

---

## 7. 实现风格

优先：

```text
小类
纯计算
不可变输入
业务命名清晰
无外部副作用
```

风险和品种筛选模块不得：

```text
读取行情
访问网络
写数据库
调用交易接口
依赖当前时间
自动生成交易建议
```

---

## 8. 命名约束

避免模糊命名：

```text
Manager
Helper
Processor
Service
Engine
Strategy
Signal
Alpha
Predictor
```

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

测试必须覆盖：

```text
标准示例
边界输入
无效输入
Allowed 结果
Caution 结果
Rejected 结果
单次返回多个原因
CSV 表头校验
必填字段校验
公式字段计算
汇总输出
```

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
SmallFuturesLab.Risk 可以编译
SmallFuturesLab.Risk.Tests 全部通过
SmallFuturesLab.ProductFilter 可以编译
SmallFuturesLab.ProductFilter.Tests 全部通过
实现与 docs/04、docs/05、docs/product/02 一致
没有策略、行情、回测、执行或实盘代码
已创建 Pull Request
```