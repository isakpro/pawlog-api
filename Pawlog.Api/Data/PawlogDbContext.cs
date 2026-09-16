using Microsoft.EntityFrameworkCore;
using Pawlog.Api.Models;

namespace Pawlog.Api.Data;

public class PawlogDbContext(DbContextOptions<PawlogDbContext> options) : DbContext(options)
{
    public DbSet<DiaryEntry> Entries => Set<DiaryEntry>();
}
