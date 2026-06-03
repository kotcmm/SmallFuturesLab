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

## 4. Git 协作流程

每次开始工作前：

```text
同步远程默认分支；
基于最新默认分支开始；
清理本地已合并或失效分支。
```

开发新功能或修改现有功能时：

```text
新建独立分支；
不要直接在默认分支提交；
分支名应表达变更目的。
```

完成实现后：

```text
运行测试；
提交代码；
推送分支；
创建 Pull Request。
```

远程分支删除需确认已合并或已明确废弃。

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