using SmallFuturesLab.Core.Accounts;
using SmallFuturesLab.Core.Filtering;
using SmallFuturesLab.Core.Risk;
using SmallFuturesLab.TradingPlanet;

if (args.Length == 0 || args[0] is "-h" or "--help")
{
    PrintUsage();
    return 0;
}

if (!string.Equals(args[0], "filter", StringComparison.OrdinalIgnoreCase))
{
    Console.Error.WriteLine("未知命令。");
    PrintUsage();
    return 1;
}

var input = GetOption(args, "--input");
var accountText = GetOption(args, "--account");
var stopTicksText = GetOption(args, "--stop-ticks") ?? "10";
var slippageTicksText = GetOption(args, "--slippage-ticks") ?? "2";
var lotsText = GetOption(args, "--lots") ?? "1";

if (string.IsNullOrWhiteSpace(input)
    || !double.TryParse(accountText, out var accountEquity)
    || !int.TryParse(stopTicksText, out var stopTicks)
    || !int.TryParse(slippageTicksText, out var slippageTicks)
    || !int.TryParse(lotsText, out var lots))
{
    Console.Error.WriteLine("参数错误。");
    PrintUsage();
    return 1;
}

var reader = new TradingPlanetFileReader();
var readResult = reader.Read(input);

foreach (var error in readResult.Errors)
{
    Console.Error.WriteLine($"读取错误 行{error.RowNumber} {error.FieldName}: {error.Reason}");
}

var calculator = new ProductFilterCalculator();
var account = new AccountProfile { Equity = accountEquity };
var scenario = new FilterScenario
{
    Lots = lots,
    StopTicks = stopTicks,
    SlippageTicks = slippageTicks,
};

Console.WriteLine("ProductCode,ContractCode,Price,MarginPerLot,MarginRateOfEquity,TotalRiskMoney,RiskRate,CostRatio,Status,Reasons");

foreach (var item in readResult.Items)
{
    var decision = calculator.Calculate(item.Product, account, scenario, RiskPolicy.Default);
    var product = item.Product;

    Console.WriteLine(string.Join(",",
        product.Identity.ProductCode,
        product.Identity.ContractCode,
        product.Economics.Price.ToString("F2"),
        decision.MarginPerLot.ToString("F2"),
        decision.MarginRateOfEquity.ToString("P2"),
        decision.TotalRiskMoney.ToString("F2"),
        decision.RiskRate.ToString("P2"),
        decision.CostRatio.ToString("P2"),
        decision.Status,
        Quote(string.Join("；", decision.Reasons))));
}

return readResult.Errors.Count > 0 ? 2 : 0;

static string? GetOption(string[] args, string name)
{
    for (var i = 0; i < args.Length - 1; i++)
    {
        if (string.Equals(args[i], name, StringComparison.OrdinalIgnoreCase))
        {
            return args[i + 1];
        }
    }

    return null;
}

static string Quote(string value)
{
    var escaped = value.Replace("\"", "\"\"", StringComparison.Ordinal);
    return $"\"{escaped}\"";
}
static void PrintUsage()
{
    Console.WriteLine("用法：");
    Console.WriteLine("  dotnet run --project src/SmallFuturesLab.Cli -- filter --input <file.xls> --account 10000 --stop-ticks 10 --slippage-ticks 2");
}
