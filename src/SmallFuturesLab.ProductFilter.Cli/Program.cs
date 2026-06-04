namespace SmallFuturesLab.ProductFilter.Cli;

/// <summary>
/// 品种筛选命令行工具入口。
/// </summary>
public class Program
{
    /// <summary>
    /// 程序入口。
    /// </summary>
    /// <param name="args">命令行参数。</param>
    public static int Main(string[] args)
    {
        var runner = new ProductFilterCliRunner();
        return runner.Run(args);
    }
}
