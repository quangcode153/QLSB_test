using Microsoft.EntityFrameworkCore;
using SportChain.Shared.Enums;
using SportChain.WebApi.Data;
using SportChain.WebApi.Entities;

namespace SportChain.WebApi.Repositories;

public class CourtRepository : ICourtRepository
{
    private readonly AppDbContext _context;

    public CourtRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Court>> GetCourtsByBranchAsync(int branchId, SportType? sportType = null)
    {
        var query = _context.Courts
            .Include(c => c.Branch)
            .Where(c => c.BranchId == branchId && c.IsActive);

        if (sportType.HasValue)
        {
            query = query.Where(c => c.SportType == sportType.Value);
        }

        return await query.ToListAsync();
    }

    public async Task<Court?> GetByIdAsync(int id)
    {
        return await _context.Courts
            .Include(c => c.Branch)
            .Include(c => c.PriceRules)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<List<TimeSlot>> GetAllTimeSlotsAsync()
    {
        return await _context.TimeSlots
            .OrderBy(t => t.StartTime)
            .ToListAsync();
    }

    public async Task<List<PriceRule>> GetPriceRulesAsync(int courtId)
    {
        return await _context.PriceRules
            .Include(p => p.TimeSlot)
            .Where(p => p.CourtId == courtId)
            .ToListAsync();
    }

    public async Task AddCourtAsync(Court court)
    {
        await _context.Courts.AddAsync(court);
    }

    public Task UpdateCourtAsync(Court court)
    {
        _context.Courts.Update(court);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
