using SmallFuturesLab.Core.Models;
using SmallFuturesLab.Core.Services;
using SmallFuturesLab.TradingPlanet;

if (args.Length == 0 || args[0] is "-h" or "--help")
{
    PrintHelp();
    return;
}

if (!string.Equals(args[0], "filter", StringComparison.OrdinalIgnoreCase))
{
    Console.Error.WriteLine("未知命令。当前只支持 filter。");
    PrintHelp();
    Environment.ExitCode = 1;
    return;
}

var options = ParseOptions(args.Skip(1).ToArray());
if (!options.TryGetValue("input", out var input) || string.IsNullOrWhiteSpace(input))
{
    Console.Error.WriteLine("缺少 --input 参数。");
    PrintHelp();
    Environment.ExitCode = 1;
    return;
}

var setting = new AccountRiskSetting
{
    AccountEquity = ReadDouble(options, "account", 10_000),
    StopTicks = ReadInt(options, "stop-ticks", 10),
    SlippageTicks = ReadInt(options, "slippage-ticks", 2),
};

var reader = new TradingPlanetTableReader();
var calculator = new ProductFilterCalculator();
var products = reader.Read(input);
var results = products.Select(p => calculator.Calculate(p, setting)).ToList();

PrintResults(results);

static Dictionary<string, string> ParseOptions(string[] args)
{
    var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    for (var i = 0; i < args.Length; i++)
    {
        var token = args[i];
        if (!token.StartsWith("--", StringComparison.Ordinal))
        {
            continue;
        }

        var key = token[2..];
        if (i + 1 < args.Length && !args[i + 1].StartsWith("--", StringComparison.Ordinal))
        {
            values[key] = args[++i];
        }
        else
        {
            values[key] = "true";
        }
    }

    return values;
}

static double ReadDouble(IReadOnlyDictionary<string, string> options, string key, double defaultValue)
{
    return options.TryGetValue(key, out var value) && double.TryParse(value, out var parsed)
        ? parsed
        : defaultValue;
}

static int ReadInt(IReadOnlyDictionary<string, string> options, string key, int defaultValue)
{
    return options.TryGetValue(key, out var value) && int.TryParse(value, out var parsed)
        ? parsed
        : defaultValue;
}

static void PrintResults(IReadOnlyList<ProductFilterResult> results)
{
    Console.WriteLine("Product\tContract\tMargin%\tRisk%\tCost%\tStatus\tReasons");
    foreach (var r in results.OrderBy(x => x.ProductCode).ThenBy(x => x.ContractCode))
    {
        Console.WriteLine(
            $"{r.ProductCode}\t{r.ContractCode}\t{r.MarginRateOfEquity:P2}\t{r.RiskRate:P2}\t{r.CostRatio:P2}\t{r.Status}\t{r.Reasons}");
    }
}

static void PrintHelp()
{
    Console.WriteLine("SmallFuturesLab CLI");
    Console.WriteLine();
    Console.WriteLine("用法：");
    Console.WriteLine("  dotnet run --project src/SmallFuturesLab.Cli -- filter --input <交易星球xls路径> --account 10000 --stop-ticks 10 --slippage-ticks 2");
    Console.WriteLine();
    Console.WriteLine("说明：输出结果仅表示能否进入后续研究，不是交易建议。");
}
