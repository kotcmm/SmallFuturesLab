# AGENTS.md

## 1. Read first

Before coding, read:

```text
README.md
docs/00_Project_Principles.md
docs/01_Account_Constraints.md
docs/02_Research_Roadmap.md
docs/03_Operation_Sketch.md
docs/04_Trade_Permission_Pipeline.md
```

If documents conflict, stop and resolve the documents first.

---

## 2. Source of truth

```text
docs/ = business rules
AGENTS.md = engineering rules
```

Do not define formulas, thresholds, trading rules, or risk rules in this file.

---

## 3. Current scope

Implement only the module described in:

```text
docs/04_Trade_Permission_Pipeline.md
```

Do not add strategy, market data, indicators, backtesting, execution, live trading, optimization, or signal generation.

---

## 4. Stack

```text
C#
.NET
xUnit
```

Changing the stack requires a documentation update first.

---

## 5. Project structure

Use:

```text
src/
  SmallFuturesLab.Risk/

test/
  SmallFuturesLab.Risk.Tests/
```

Do not create extra top-level modules before they are documented.

---

## 6. Implementation style

Prefer:

```text
small classes
pure calculation
immutable inputs
clear business names
tests first
no external side effects
```

The risk module must not read market data, access networks, write databases, call brokers, or depend on current time.

---

## 7. Naming

Avoid vague names:

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

Use names from the relevant docs when possible.

---

## 8. Tests

Tests must cover:

```text
standard examples
boundary inputs
invalid inputs
allowed result
caution result
rejected result
multiple reasons in one result
```

Test names should describe business behavior.

---

## 9. Numeric precision

`double` is acceptable for the current stage.

Use tolerances in floating-point assertions.

---

## 10. Documentation flow

When code and docs conflict:

```text
1. stop coding
2. update docs
3. update tests
4. update implementation
```

Do not let code change project rules silently.

---

## 11. Done means

```text
SmallFuturesLab.Risk builds
SmallFuturesLab.Risk.Tests pass
implementation matches docs/04
no strategy, market data, backtest, execution, or live-trading code
```