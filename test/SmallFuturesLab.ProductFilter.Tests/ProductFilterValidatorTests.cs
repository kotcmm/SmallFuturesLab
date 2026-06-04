namespace SmallFuturesLab.ProductFilter.Tests;

public class ProductFilterValidatorTests
{
    /// <summary>
    /// 数值字段不可解析时校验失败。
    /// </summary>
    [Fact]
    public void Validate_NonNumericValue_ReturnsFailure()
    {
        var row = new ProductFilterRow
        {
            Exchange = "Test",
            ProductName = "Test",
            ProductCode = "T",
            ContractCode = "T2501",
            Price = double.NaN,
            Multiplier = 10,
            TickSize = 1,
            MarginRate = 0.1,
            RoundTripFeePerLot = 6,
            SlippageTicks = 2,
            TypicalAtr = 20,
            StopDistance = 12,
            AccountEquity = 10000,
            LiquidityLevel = LiquidityLevel.Good,
            BookContinuityLevel = BookContinuityLevel.Good,
            RolloverClarity = RolloverClarity.Good,
            DataDate = "2024-01-01",
            DataSource = "Test",
        };

        var validator = new ProductFilterValidator();
        var result = validator.Validate(row, 1);

        Assert.False(result.IsValid);
    }

    /// <summary>
    /// Price 小于等于 0 时校验失败。
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_PriceNotPositive_ReturnsFailure(double price)
    {
        var row = CreateValidRow() with { Price = price };
        var validator = new ProductFilterValidator();
        var result = validator.Validate(row, 1);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.FieldName == "Price");
    }

    /// <summary>
    /// TickSize 小于等于 0 时校验失败。
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_TickSizeNotPositive_ReturnsFailure(double tickSize)
    {
        var row = CreateValidRow() with { TickSize = tickSize };
        var validator = new ProductFilterValidator();
        var result = validator.Validate(row, 1);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.FieldName == "TickSize");
    }

    /// <summary>
    /// Multiplier 小于等于 0 时校验失败。
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void Validate_MultiplierNotPositive_ReturnsFailure(double multiplier)
    {
        var row = CreateValidRow() with { Multiplier = multiplier };
        var validator = new ProductFilterValidator();
        var result = validator.Validate(row, 1);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.FieldName == "Multiplier");
    }

    /// <summary>
    /// StopDistance 小于等于 0 时校验失败。
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Validate_StopDistanceNotPositive_ReturnsFailure(double stopDistance)
    {
        var row = CreateValidRow() with { StopDistance = stopDistance };
        var validator = new ProductFilterValidator();
        var result = validator.Validate(row, 1);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.FieldName == "StopDistance");
    }

    /// <summary>
    /// AccountEquity 小于等于 0 时校验失败。
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_AccountEquityNotPositive_ReturnsFailure(double equity)
    {
        var row = CreateValidRow() with { AccountEquity = equity };
        var validator = new ProductFilterValidator();
        var result = validator.Validate(row, 1);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.FieldName == "AccountEquity");
    }

    /// <summary>
    /// 枚举字段不是 Good / Medium / Poor / Unknown 时校验失败。
    /// </summary>
    [Fact]
    public void Validate_InvalidEnumValue_ReturnsFailure()
    {
        var row = CreateValidRow() with { LiquidityLevel = (LiquidityLevel)999 };
        var validator = new ProductFilterValidator();
        var result = validator.Validate(row, 1);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.FieldName == "LiquidityLevel");
    }

    /// <summary>
    /// 多个错误应一次返回，而不是只返回第一个错误。
    /// </summary>
    [Fact]
    public void Validate_MultipleErrors_ReturnsAllErrors()
    {
        var row = CreateValidRow() with
        {
            Price = -100,
            Multiplier = 0,
            TickSize = -1,
            StopDistance = 0,
        };
        var validator = new ProductFilterValidator();
        var result = validator.Validate(row, 1);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.FieldName == "Price");
        Assert.Contains(result.Errors, e => e.FieldName == "Multiplier");
        Assert.Contains(result.Errors, e => e.FieldName == "TickSize");
        Assert.Contains(result.Errors, e => e.FieldName == "StopDistance");
    }

    private static ProductFilterRow CreateValidRow() => new()
    {
        Exchange = "TestExchange",
        ProductName = "TestProduct",
        ProductCode = "TP",
        ContractCode = "TP2501",
        Price = 2500,
        Multiplier = 10,
        TickSize = 1,
        MarginRate = 0.10,
        RoundTripFeePerLot = 6,
        SlippageTicks = 2,
        TypicalAtr = 20,
        StopDistance = 12,
        AccountEquity = 10000,
        LiquidityLevel = LiquidityLevel.Good,
        BookContinuityLevel = BookContinuityLevel.Good,
        RolloverClarity = RolloverClarity.Good,
        DataDate = "2024-01-01",
        DataSource = "TestSource",
    };
}
