using SmallFuturesLab.TradingPlanet;

namespace SmallFuturesLab.TradingPlanet.Tests;

public sealed class TradingPlanetFileReaderTests
{
    [Fact]
    public void Read_Parses_Valid_Row_To_Product_Info()
    {
        var file = Path.Combine(AppContext.BaseDirectory, "Fixtures", "trading_planet_sample.xls");

        var result = new TradingPlanetFileReader().Read(file);

        var item = Assert.Single(result.Items);
        Assert.Equal("MA", item.Product.Identity.ProductCode);
        Assert.Equal("MA2601", item.Product.Identity.ContractCode);
        Assert.Equal("甲醇", item.Product.Identity.ProductName);
        Assert.Equal(2500, item.Product.Economics.Price);
        Assert.Equal(2500, item.Product.Economics.MarginPerLot);
        Assert.Equal(10, item.Product.Economics.TickValue);
        Assert.Equal(6, item.Product.Economics.RoundTripFeePerLot);
        Assert.True(item.NeedsReview);
        Assert.Equal("测试备注", item.RawRemark);
    }

    [Fact]
    public void Read_Bad_Row_Returns_Error_And_Does_Not_Enter_Items()
    {
        var file = Path.Combine(AppContext.BaseDirectory, "Fixtures", "trading_planet_sample.xls");

        var result = new TradingPlanetFileReader().Read(file);

        Assert.Single(result.Items);
        Assert.Contains(result.Errors, x => x.FieldName == "Price");
    }

    [Fact]
    public void Product_Provider_Does_Not_Expose_File_Path_In_Core_Interface()
    {
        var file = Path.Combine(AppContext.BaseDirectory, "Fixtures", "trading_planet_sample.xls");
        var provider = new TradingPlanetProductProvider(file);

        var products = provider.GetProducts();

        Assert.Single(products);
    }
}
