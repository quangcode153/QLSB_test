using SportChain.Tests.Helpers;
using SportChain.WebApi.Data;
using SportChain.WebApi.Repositories;
using Xunit;

namespace SportChain.Tests.Backend;

public class MultiBranchIsolationTests
{
    private readonly AppDbContext _context;
    private readonly CourtRepository _courtRepo;

    public MultiBranchIsolationTests()
    {
        _context = TestDbHelper.CreateInMemoryDbContext();
        _courtRepo = new CourtRepository(_context);
    }

    [Fact]
    public async Task GetCourtsByBranchAsync_StrictlyIsolatesBranchData()
    {
        // Act: Lấy danh sách sân của Chi nhánh 1 (Cầu Giấy)
        var branch1Courts = await _courtRepo.GetCourtsByBranchAsync(1);

        // Act: Lấy danh sách sân của Chi nhánh 2 (Hai Bà Trưng)
        var branch2Courts = await _courtRepo.GetCourtsByBranchAsync(2);

        // Assert (Rule BR-MULTI-01: Cách ly cơ sở)
        Assert.NotEmpty(branch1Courts);
        Assert.NotEmpty(branch2Courts);

        // 100% sân thuộc chi nhánh 1 phải có BranchId == 1
        Assert.All(branch1Courts, c => Assert.Equal(1, c.BranchId));

        // 100% sân thuộc chi nhánh 2 phải có BranchId == 2
        Assert.All(branch2Courts, c => Assert.Equal(2, c.BranchId));

        // Sân của chi nhánh 2 không được phép xuất hiện trong danh sách chi nhánh 1
        var branch2CourtIds = branch2Courts.Select(c => c.Id).ToHashSet();
        Assert.DoesNotContain(branch1Courts, c => branch2CourtIds.Contains(c.Id));
    }
}
