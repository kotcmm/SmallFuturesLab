namespace SmallFuturesLab.ProductFilter.Tests;

public class ProductFilterCsvReaderTests
{
    private static string FixturePath(string fileName) =>
        Path.Combine(AppContext.BaseDirectory, "Fixtures", fileName);

    /// <summary>
    /// 模板表头完整时校验通过，且能正确读取数据行。
    /// </summary>
    [Fact]
    public void Read_ValidHeaderAndData_ReturnsRows()
    {
        var reader = new ProductFilterCsvReader();
        var result = reader.Read(FixturePath("valid_product_filter.csv"));

        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Rows.Count);
        Assert.Equal("TestProductA", result.Rows[0].ProductName);
        Assert.Equal(2500, result.Rows[0].Price);
    }

    /// <summary>
    /// 字段顺序错误时校验失败。
    /// </summary>
    [Fact]
    public void Read_WrongHeaderOrder_ReturnsFailure()
    {
        var reader = new ProductFilterCsvReader();
        var result = reader.Read(FixturePath("invalid_header.csv"));

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.FieldName.Contains("表头") || e.Reason.Contains("表头"));
    }

    /// <summary>
    /// 缺少字段时校验失败。
    /// </summary>
    [Fact]
    public void Read_MissingRequiredField_ReturnsFailure()
    {
        var reader = new ProductFilterCsvReader();
        var result = reader.Read(FixturePath("invalid_missing_field.csv"));

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Reason.Contains("必填") || e.Reason.Contains("空"));
    }
}
