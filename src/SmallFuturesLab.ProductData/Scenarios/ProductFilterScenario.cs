namespace SmallFuturesLab.ProductData.Scenarios;

/// <summary>
/// 单个测算场景，包含账户规模、止损距离和滑点假设。
/// </summary>
public record ProductFilterScenario
{
    private string _name = string.Empty;
    private double _accountEquity;
    private double _stopDistance;
    private int _slippageTicks;

    /// <summary>
    /// 场景名称，不能为空。
    /// </summary>
    public string Name
    {
        get => _name;
        init
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Name 不能为空", nameof(value));
            }

            _name = value;
        }
    }

    /// <summary>
    /// 账户权益，必须是有限数字且大于 0。
    /// </summary>
    public double AccountEquity
    {
        get => _accountEquity;
        init
        {
            if (double.IsNaN(value) || double.IsInfinity(value) || value <= 0)
            {
                throw new ArgumentException("AccountEquity 必须是有限数字且大于 0", nameof(value));
            }

            _accountEquity = value;
        }
    }

    /// <summary>
    /// 测算止损距离，必须是有限数字且大于 0。
    /// </summary>
    public double StopDistance
    {
        get => _stopDistance;
        init
        {
            if (double.IsNaN(value) || double.IsInfinity(value) || value <= 0)
            {
                throw new ArgumentException("StopDistance 必须是有限数字且大于 0", nameof(value));
            }

            _stopDistance = value;
        }
    }

    /// <summary>
    /// 预估滑点 tick 数，不能为负数。
    /// </summary>
    public int SlippageTicks
    {
        get => _slippageTicks;
        init
        {
            if (value < 0)
            {
                throw new ArgumentException("SlippageTicks 不能为负数", nameof(value));
            }

            _slippageTicks = value;
        }
    }
}
