namespace DataAccess.DatabaseContext;

public class DbContextService : DbContext
{
    private readonly IConfiguration _configuration;

    public DbSet<NoteModel> Notes => Set<NoteModel>();
    public DbSet<UserModel> Users => Set<UserModel>();
    public DbSet<SessionUserModel> SessionUsers => Set<SessionUserModel>();
    public DbSet<JwtTokenModel> JwtTokenModels => Set<JwtTokenModel>();

    public DbContextService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(_configuration.GetConnectionString("DockerPostgresConnectionString"));
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<NoteModel>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("note_model_pkey");
            entity.ToTable("Notes");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.User_Id).HasColumnName("user_id");
            entity.Property(e => e.Title).HasColumnName("title");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");

        });

        modelBuilder.Entity<SessionUserModel>(entity =>
        {

            entity.HasKey(e => e.Id).HasName("session_model_pkey");
            entity.ToTable("SessionUsers");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e=>e.Token_Id).HasColumnName("token_id");
            entity.Property(e => e.User_Id).HasColumnName("user_id");

            entity.HasOne(e => e.JwtToken)
            .WithOne(x => x.SessionUser)
            .HasForeignKey<SessionUserModel>(e => e.Token_Id);

        });
        modelBuilder.Entity<JwtTokenModel>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("jwt_token_pkey");
            entity.ToTable("JwtTokenModels");

            entity.Property(e=> e.Id).HasColumnName("id");
            entity.Property(e => e.Session_Id).HasColumnName("session_id");
            entity.Property(e => e.RefreshTokenJti).HasColumnName("refresh_token_Jti");
            entity.Property(e => e.RefreshTokenExpiration).HasColumnName("refresh_token_expiration");

        });

        modelBuilder.Entity<UserModel>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("user_model_pkey");
            entity.ToTable("Users");

            entity.Property(e=>e.Id).HasColumnName("id");
            entity.Property(e=>e.UserName).HasColumnName("user_name");
            entity.Property(e => e.Login).HasColumnName("login");
            entity.Property(e => e.SaltPassword).HasColumnName("salt_password");
            entity.Property(e => e.Password).HasColumnName("password");


            entity.
            HasMany(n => n.NoteModels).
            WithOne(m => m.User).
            HasForeignKey(e => e.User_Id);

            entity.
            HasMany(t => t.Sessions).
            WithOne(u => u.User).
            HasForeignKey(i => i.User_Id);
        });

    }
}
