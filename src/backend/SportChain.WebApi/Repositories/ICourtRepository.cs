using SportChain.Shared.Enums;
using SportChain.WebApi.Entities;

namespace SportChain.WebApi.Repositories;

public interface ICourtRepository
{
    Task<List<Court>> GetCourtsByBranchAsync(int branchId, SportType? sportType = null);
    Task<Court?> GetByIdAsync(int id);
    Task<List<TimeSlot>> GetAllTimeSlotsAsync();
    Task<List<PriceRule>> GetPriceRulesAsync(int courtId);
    Task AddCourtAsync(Court court);
    Task UpdateCourtAsync(Court court);
    Task SaveChangesAsync();
}
