using SmallFuturesLab.Core;

namespace SmallFuturesLab.TradingPlanet.Tests;

public sealed class TradingPlanetFileReaderTests
{
    [Fact]
    public void Read_Parses_Valid_Row_To_Product()
    {
        var file = Path.Combine(AppContext.BaseDirectory, "Fixtures", "trading_planet_sample.xls");

        var result = new TradingPlanetFileReader().Read(file);

        var product = Assert.Single(result.Products);
        Assert.Equal("MA", product.ProductId);
        Assert.Equal("MA2601", product.InstrumentId);
        Assert.Equal("甲醇", product.Name);
        Assert.Equal(2500, product.Price);
        Assert.Equal(10, product.Multiplier);
        Assert.Equal(1, product.TickSize);
        Assert.Equal(0.10, product.MarginRate, 6);
        Assert.Equal(6, product.RoundTripFee);
    }

    [Fact]
    public void Read_Parses_Code_And_Contract()
    {
        var file = Path.Combine(AppContext.BaseDirectory, "Fixtures", "trading_planet_sample.xls");

        var result = new TradingPlanetFileReader().Read(file);

        var product = Assert.Single(result.Products);
        Assert.Equal("MA", product.ProductId);
        Assert.Equal("MA2601", product.InstrumentId);
    }

    [Fact]
    public void Read_Parses_Price()
    {
        var file = Path.Combine(AppContext.BaseDirectory, "Fixtures", "trading_planet_sample.xls");

        var result = new TradingPlanetFileReader().Read(file);

        var product = Assert.Single(result.Products);
        Assert.Equal(2500, product.Price);
    }

    [Fact]
    public void Read_Parses_MarginRate()
    {
        var file = Path.Combine(AppContext.BaseDirectory, "Fixtures", "trading_planet_sample.xls");

        var result = new TradingPlanetFileReader().Read(file);

        var product = Assert.Single(result.Products);
        Assert.Equal(0.10, product.MarginRate, 6);
    }

    [Fact]
    public void Read_Parses_RoundTripFee()
    {
        var file = Path.Combine(AppContext.BaseDirectory, "Fixtures", "trading_planet_sample.xls");

        var result = new TradingPlanetFileReader().Read(file);

        var product = Assert.Single(result.Products);
        Assert.Equal(6, product.RoundTripFee);
    }

    [Fact]
    public void Read_Uses_ProductSpecLookup_To_Fill_Multiplier_And_TickSize()
    {
        var file = Path.Combine(AppContext.BaseDirectory, "Fixtures", "trading_planet_sample.xls");
        var lookup = new ProductSpecLookup(new Dictionary<string, ProductSpec>
        {
            { "MA", new ProductSpec(20, 2) },
        });

        var result = new TradingPlanetFileReader(lookup).Read(file);

        var product = Assert.Single(result.Products);
        Assert.Equal(20, product.Multiplier);
        Assert.Equal(2, product.TickSize);
    }

    [Fact]
    public void Read_Returns_Error_When_ProductSpec_Not_Found()
    {
        var file = Path.Combine(AppContext.BaseDirectory, "Fixtures", "trading_planet_sample.xls");
        var lookup = new ProductSpecLookup(new Dictionary<string, ProductSpec>());

        var result = new TradingPlanetFileReader(lookup).Read(file);

        Assert.Empty(result.Products);
        Assert.Contains(result.Errors, e => e.FieldName == "Code");
    }

    [Fact]
    public void Read_Bad_Numeric_Row_Does_Not_Enter_Products()
    {
        var file = Path.Combine(AppContext.BaseDirectory, "Fixtures", "trading_planet_sample.xls");

        var result = new TradingPlanetFileReader().Read(file);

        Assert.Single(result.Products);
        Assert.Contains(result.Errors, e => e.FieldName == "Price");
    }

    [Fact]
    public void Reader_Does_Not_Call_ProductFilter()
    {
        var file = Path.Combine(AppContext.BaseDirectory, "Fixtures", "trading_planet_sample.xls");

        var result = new TradingPlanetFileReader().Read(file);

        Assert.All(result.Products, p => Assert.IsType<ProductInfo>(p));
        Assert.DoesNotContain(result.Products, p => p.ProductId == string.Empty);
    }
}
