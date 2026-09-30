using SportChain.Shared.DTOs.Courts;

namespace SportChain.WebApi.Services;

public interface ICourtMatrixService
{
    Task<BranchMatrixDto> GetBranchMatrixAsync(int branchId, DateOnly date);
}
