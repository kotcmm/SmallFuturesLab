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

var reader = new TradingPlanetFileReader(stopTicks: stopTicks, slippageTicks: slippageTicks, lots: lots);
var readResult = reader.Read(input);

foreach (var error in readResult.Errors)
{
    Console.Error.WriteLine($"读取错误 行{error.RowNumber} {error.FieldName}: {error.Reason}");
}

var filter = new ProductFilter();
var config = new RiskConfig { AccountEquity = accountEquity };

foreach (var contract in readResult.Contracts)
{
    var result = filter.Evaluate(contract, config);

    Console.WriteLine($"品种: {result.ProductCode} 合约: {result.ContractCode}");
    Console.WriteLine($"  价格: {contract.Price:F2}");
    Console.WriteLine($"  保证金金额: {result.MarginMoney:F2}");
    Console.WriteLine($"  总风险金额: {result.TotalRiskMoney:F2}");
    Console.WriteLine($"  风险比例: {result.RiskRate:P2}");
    Console.WriteLine($"  保证金比例: {result.MarginRate:P2}");
    Console.WriteLine($"  成本比例: {result.CostRatio:P2}");
    Console.WriteLine($"  状态: {result.Status}");
    Console.WriteLine($"  原因: {string.Join("；", result.Reasons)}");
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
