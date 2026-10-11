using Microsoft.EntityFrameworkCore;

namespace PasswordGenerator.Api.Shared.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<StoredPassword> Passwords => Set<StoredPassword>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<StoredPassword>(entity =>
        {
            entity.ToTable("Passwords", table =>
                table.HasCheckConstraint("CK_Passwords_Senha_Length", "length(Senha) BETWEEN 16 AND 128"));
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Id).ValueGeneratedNever();
            entity.Property(p => p.Senha).IsRequired();
            entity.Property(p => p.CreatedAtUtc).IsRequired();
        });
    }
}
