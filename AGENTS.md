# AGENTS.md

## 1. 先读文档

编码前先读：

```text
README.md
docs/00_Project_Principles.md
docs/01_Account_Constraints.md
docs/02_Research_Roadmap.md
docs/03_Operation_Sketch.md
docs/04_Trade_Permission_Pipeline.md
```

文档冲突时，先停下，先修正文档。

---

## 2. 规则来源

```text
docs/ = 业务规则
AGENTS.md = 工程规则
```

不要在本文件定义公式、阈值、交易规则或风险规则。

---

## 3. 当前范围

只实现：

```text
docs/04_Trade_Permission_Pipeline.md
```

不要新增策略、行情、指标、回测、执行、实盘、优化、信号生成等模块。

---

## 4. 本地 Kimi CLI 协作约定

如果由本地 Kimi CLI 执行任务，遵守：

```text
默认远程仓库名为 origin。
开始前执行 git fetch origin。
基于 origin/main 创建任务分支。
不直接在 main 分支上修改代码。
分支名应与任务说明一致。
```

开始任务前，应清理本地已合并或已废弃的无效分支。

完成实现后，必须执行任务指定的验收命令。

当前阶段默认验收命令：

```text
dotnet test
```

如果仓库后续增加解决方案文件，应优先使用文档指定的解决方案级验收命令。

验收通过后：

```text
提交 commit；
推送到 origin；
创建 Pull Request。
```

如果本地已安装 GitHub CLI，应使用：

```text
gh pr create
```

如果没有 `gh`，最终输出必须给出可复制的 PR 创建命令或浏览器创建链接。

PR 描述必须包含：

```text
实现范围；
测试结果；
未实现范围；
是否遵守 TDD；
是否遵守一类一文件；
是否遵守中文 XML 注释约定。
```

不允许因为追求速度而绕过：

```text
AGENTS.md；
docs/ 文档；
测试；
工程规范。
```

用户希望尽量少手动操作。Kimi CLI 应主动完成：

```text
拉取；
建分支；
实现；
测试；
提交；
推送；
创建 PR。
```

用户最好只需要启动一次 Kimi 命令，然后把 PR 链接交给 ChatGPT review。

---

## 5. 技术栈

```text
C#
.NET
xUnit
```

更换技术栈前，先更新文档。

---

## 6. 项目结构

使用：

```text
src/
  SmallFuturesLab.Risk/

test/
  SmallFuturesLab.Risk.Tests/
```

未写入文档前，不要创建额外顶层模块。

---

## 7. 实现风格

优先：

```text
小类
纯计算
不可变输入
业务命名清晰
测试先行
无外部副作用
```

风险模块不得读取行情、访问网络、写数据库、调用交易接口或依赖当前时间。

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

新增或修改 `src/` 下的生产代码时，遵守：

```text
public 类型必须有中文 XML 注释。
public 属性必须有中文 XML 注释。
public 方法必须有中文 XML 注释。
public enum 及 enum 成员必须有中文 XML 注释。
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
```

测试名称应描述业务行为。

---

## 11. 数值精度

当前阶段允许使用 `double`。

浮点断言必须使用容差。

---

## 12. 文档同步

代码和文档冲突时：

```text
1. 停止编码
2. 更新文档
3. 更新测试
4. 更新实现
```

不要让代码悄悄改变项目规则。

---

## 13. 完成标准

```text
SmallFuturesLab.Risk 可以编译
SmallFuturesLab.Risk.Tests 全部通过
实现与 docs/04 一致
没有策略、行情、回测、执行或实盘代码
已创建 Pull Request
```