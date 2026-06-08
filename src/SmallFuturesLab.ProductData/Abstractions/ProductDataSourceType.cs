namespace SmallFuturesLab.ProductData.Abstractions;

/// <summary>
/// 品种数据来源类型。
/// </summary>
public enum ProductDataSourceType
{
    /// <summary>
    /// 第三方研究数据，如交易星球等手续费保证金网站。
    /// </summary>
    ThirdPartyResearch,

    /// <summary>
    /// 交易所公开信息。
    /// </summary>
    ExchangePublic,

    /// <summary>
    /// 期货公司表格或公告。
    /// </summary>
    BrokerTable,

    /// <summary>
    /// CTP 账户实际数据，仅限实盘前复核和实盘阶段使用。
    /// </summary>
    CtpAccountActual,

    /// <summary>
    /// 行情 API 或行情软件导出数据。
    /// </summary>
    MarketDataApi,

    /// <summary>
    /// 人工维护的本地配置。
    /// </summary>
    ManualConfig,
}
