using SmallFuturesLab.TradingPlanet;

namespace SmallFuturesLab.TradingPlanet.Tests;

public class TradingPlanetTableReaderTests
{
    [Fact]
    public void ParseHtml_Reads_TradingPlanet_Row()
    {
        const string html = """
        <html><body><table>
          <tr><td>郑州商品交易所</td></tr>
          <tr>
            <td>甲醇509</td><td>MA509</td><td>2500</td><td>123456</td>
            <td>10%</td><td>11%</td><td>2750</td>
            <td>2元</td><td>2元</td><td>2元</td><td>4元</td>
            <td>10</td><td>6</td><td>主力</td>
          </tr>
        </table></body></html>
        """;

        var rows = new TradingPlanetTableReader().ParseHtml(html, "期货手续费和保证金一览表2026年06月09日更新.xls");

        var row = Assert.Single(rows);
        Assert.Equal("郑州商品交易所", row.Exchange);
        Assert.Equal("MA", row.ProductCode);
        Assert.Equal("MA509", row.ContractCode);
        Assert.Equal(2500, row.Price);
        Assert.Equal(0.11, row.MarginRate, 6);
        Assert.Equal(2750, row.MarginPerLot);
        Assert.Equal(4, row.RoundTripFeePerLot);
        Assert.Equal(10, row.TickValue);
        Assert.Equal("2026-06-09", row.DataDate);
        Assert.True(row.IsMainContract);
    }
}
