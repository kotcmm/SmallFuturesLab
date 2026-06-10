using SmallFuturesLab.Core;

namespace SmallFuturesLab.TradingPlanet.Tests;

public sealed class TradingPlanetFileReaderTests
{
    [Fact]
    public void Read_Parses_Valid_Row_To_FuturesContract()
    {
        var file = Path.Combine(AppContext.BaseDirectory, "Fixtures", "trading_planet_sample.xls");

        var result = new TradingPlanetFileReader().Read(file);

        var contract = Assert.Single(result.Contracts);
        Assert.Equal("MA", contract.ProductCode);
        Assert.Equal("MA2601", contract.ContractCode);
        Assert.Equal("甲醇", contract.ProductName);
        Assert.Equal(2500, contract.Price);
        Assert.Equal(10, contract.Multiplier);
        Assert.Equal(1, contract.TickSize);
        Assert.Equal(0.10, contract.MarginRate, 6);
        Assert.Equal(6, contract.RoundTripFee);
        Assert.Equal(10, contract.StopTicks);
        Assert.Equal(2, contract.SlippageTicks);
        Assert.Equal(1, contract.Lots);
    }

    [Fact]
    public void Read_Parses_ProductCode_And_ContractCode()
    {
        var file = Path.Combine(AppContext.BaseDirectory, "Fixtures", "trading_planet_sample.xls");

        var result = new TradingPlanetFileReader().Read(file);

        var contract = Assert.Single(result.Contracts);
        Assert.Equal("MA", contract.ProductCode);
        Assert.Equal("MA2601", contract.ContractCode);
    }

    [Fact]
    public void Read_Parses_Price()
    {
        var file = Path.Combine(AppContext.BaseDirectory, "Fixtures", "trading_planet_sample.xls");

        var result = new TradingPlanetFileReader().Read(file);

        var contract = Assert.Single(result.Contracts);
        Assert.Equal(2500, contract.Price);
    }

    [Fact]
    public void Read_Parses_MarginRate()
    {
        var file = Path.Combine(AppContext.BaseDirectory, "Fixtures", "trading_planet_sample.xls");

        var result = new TradingPlanetFileReader().Read(file);

        var contract = Assert.Single(result.Contracts);
        Assert.Equal(0.10, contract.MarginRate, 6);
    }

    [Fact]
    public void Read_Parses_RoundTripFee()
    {
        var file = Path.Combine(AppContext.BaseDirectory, "Fixtures", "trading_planet_sample.xls");

        var result = new TradingPlanetFileReader().Read(file);

        var contract = Assert.Single(result.Contracts);
        Assert.Equal(6, contract.RoundTripFee);
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

        var contract = Assert.Single(result.Contracts);
        Assert.Equal(20, contract.Multiplier);
        Assert.Equal(2, contract.TickSize);
    }

    [Fact]
    public void Read_Returns_Error_When_ProductSpec_Not_Found()
    {
        var file = Path.Combine(AppContext.BaseDirectory, "Fixtures", "trading_planet_sample.xls");
        var lookup = new ProductSpecLookup(new Dictionary<string, ProductSpec>());

        var result = new TradingPlanetFileReader(lookup).Read(file);

        Assert.Empty(result.Contracts);
        Assert.Contains(result.Errors, e => e.FieldName == "ProductCode");
    }

    [Fact]
    public void Read_Bad_Numeric_Row_Does_Not_Enter_Contracts()
    {
        var file = Path.Combine(AppContext.BaseDirectory, "Fixtures", "trading_planet_sample.xls");

        var result = new TradingPlanetFileReader().Read(file);

        Assert.Single(result.Contracts);
        Assert.Contains(result.Errors, e => e.FieldName == "Price");
    }

    [Fact]
    public void Reader_Does_Not_Call_ProductFilter()
    {
        var file = Path.Combine(AppContext.BaseDirectory, "Fixtures", "trading_planet_sample.xls");

        var result = new TradingPlanetFileReader().Read(file);

        Assert.All(result.Contracts, c => Assert.IsType<FuturesContract>(c));
        Assert.DoesNotContain(result.Contracts, c => c.ProductCode == string.Empty);
    }
}
