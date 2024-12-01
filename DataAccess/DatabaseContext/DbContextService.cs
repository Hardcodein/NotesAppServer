using DataAccess.Models;

namespace DataAccess.DatabaseContext;
public class DbContextService : DbContext
{
    private readonly IConfiguration _configuration;
    public DbSet<NoteModel> Notes => Set<NoteModel>();

    public DbContextService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(_configuration.GetConnectionString("Database"));
    }
}
