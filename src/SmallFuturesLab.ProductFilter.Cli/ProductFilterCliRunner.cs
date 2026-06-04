namespace SmallFuturesLab.ProductFilter.Cli;

/// <summary>
/// 品种筛选命令行运行器。
/// </summary>
public class ProductFilterCliRunner
{
    private readonly ProductFilterCsvReader _csvReader = new();
    private readonly ProductFilterValidator _validator = new();
    private readonly ProductFilterCalculator _calculator = new();
    private readonly ProductFilterCsvWriter _csvWriter = new();
    private readonly ProductFilterSummaryWriter _summaryWriter = new();

    /// <summary>
    /// 运行 CLI。
    /// </summary>
    /// <param name="args">命令行参数。</param>
    /// <returns>退出码，0 表示成功，非 0 表示失败。</returns>
    public int Run(string[] args)
    {
        var inputPath = GetArgValue(args, "--input");
        var outputPath = GetArgValue(args, "--output");
        var summaryPath = GetArgValue(args, "--summary");

        if (string.IsNullOrWhiteSpace(inputPath))
        {
            Console.WriteLine("错误：缺少必需参数 --input");
            return 1;
        }

        if (string.IsNullOrWhiteSpace(outputPath))
        {
            Console.WriteLine("错误：缺少必需参数 --output");
            return 1;
        }

        if (string.IsNullOrWhiteSpace(summaryPath))
        {
            Console.WriteLine("错误：缺少必需参数 --summary");
            return 1;
        }

        if (!File.Exists(inputPath))
        {
            Console.WriteLine($"错误：输入文件不存在：{inputPath}");
            return 1;
        }

        var readResult = _csvReader.Read(inputPath);
        if (!readResult.IsSuccess)
        {
            Console.WriteLine("CSV 校验失败：");
            foreach (var error in readResult.Errors)
            {
                Console.WriteLine($"  行 {error.RowNumber}，字段 '{error.FieldName}'：{error.Reason}");
            }
            return 1;
        }

        var validationErrors = new List<ProductFilterValidationError>();
        var calculatedResults = new List<ProductFilterCalculationResult>();

        for (int i = 0; i < readResult.Rows.Count; i++)
        {
            var row = readResult.Rows[i];
            var validation = _validator.Validate(row, i + 2);
            if (!validation.IsValid)
            {
                validationErrors.AddRange(validation.Errors);
                continue;
            }

            var result = _calculator.Calculate(row);
            calculatedResults.Add(result);
        }

        if (validationErrors.Count > 0)
        {
            Console.WriteLine("数据校验失败：");
            foreach (var error in validationErrors)
            {
                Console.WriteLine($"  行 {error.RowNumber}，字段 '{error.FieldName}'：{error.Reason}");
            }
            return 1;
        }

        var calculatedRows = calculatedResults.Select(r => r.Row).ToList();
        _csvWriter.Write(outputPath, calculatedRows);
        var markdown = _summaryWriter.WriteMarkdown(calculatedResults);
        File.WriteAllText(summaryPath, markdown, System.Text.Encoding.UTF8);

        Console.WriteLine($"完成。输出 CSV：{outputPath}，汇总报告：{summaryPath}");
        return 0;
    }

    private static string? GetArgValue(string[] args, string key)
    {
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (args[i] == key)
            {
                return args[i + 1];
            }
        }
        return null;
    }
}
