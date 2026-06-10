using SmallFuturesLab.Core;
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

var accountConfig = new AccountRiskConfig { AccountEquity = accountEquity };
var riskConfig = new ProductRiskConfig
{
    StopTicks = stopTicks,
    SlippageTicks = slippageTicks,
    Lots = lots,
};

foreach (var product in readResult.Products)
{
    var eval = new ProductEvaluation(product, accountEquity, riskConfig);
    var status = eval.Evaluate(accountConfig);

    Console.WriteLine($"品种: {product.ProductId} 合约: {product.InstrumentId}");
    Console.WriteLine($"  价格: {product.Price:F2}");
    Console.WriteLine($"  保证金金额: {eval.MarginPerLot * riskConfig.Lots:F2}");
    Console.WriteLine($"  总风险金额: {eval.TotalRiskMoney:F2}");
    Console.WriteLine($"  风险比例: {eval.RiskRate:P2}");
    Console.WriteLine($"  保证金比例: {eval.MarginRateOfEquity:P2}");
    Console.WriteLine($"  成本比例: {eval.CostRatio:P2}");
    Console.WriteLine($"  状态: {status}");
    Console.WriteLine();
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

static void PrintUsage()
{
    Console.WriteLine("用法：");
    Console.WriteLine("  dotnet run --project src/SmallFuturesLab.Cli -- filter --input <file.xls> --account 10000 --stop-ticks 10 --slippage-ticks 2");
}
