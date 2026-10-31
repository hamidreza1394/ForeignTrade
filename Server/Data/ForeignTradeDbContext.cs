using ForeignTrade.Server.Domain;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ForeignTrade.Server.Data;

public sealed class ForeignTradeDbContext(DbContextOptions<ForeignTradeDbContext> options)
    : IdentityDbContext<ApplicationUser, Microsoft.AspNetCore.Identity.IdentityRole<Guid>, Guid>(options)
{
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<UserCompanyMembership> UserCompanyMemberships => Set<UserCompanyMembership>();
    public DbSet<OperationalPeriod> OperationalPeriods => Set<OperationalPeriod>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Tenant>(entity =>
        {
            entity.ToTable("Tenants");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.HasIndex(x => x.Name);
        });

        builder.Entity<Company>(entity =>
        {
            entity.ToTable("Companies");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(250).IsRequired();
            entity.Property(x => x.NationalId).HasMaxLength(32);
            entity.HasOne<Tenant>().WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => new { x.TenantId, x.Name });
            entity.HasAlternateKey(x => new { x.TenantId, x.Id });
            entity.HasIndex(x => new { x.TenantId, x.NationalId }).IsUnique().HasFilter("[NationalId] IS NOT NULL");
        });

        builder.Entity<UserCompanyMembership>(entity =>
        {
            entity.ToTable("UserCompanyMemberships");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.RoleName).HasMaxLength(120).IsRequired();
            entity.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<Company>().WithMany().HasForeignKey(x => new { x.TenantId, x.CompanyId }).HasPrincipalKey(x => new { x.TenantId, x.Id }).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<Tenant>().WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => new { x.TenantId, x.UserId, x.CompanyId }).IsUnique();
        });

        builder.Entity<OperationalPeriod>(entity =>
        {
            entity.ToTable("OperationalPeriods");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Title).HasMaxLength(160).IsRequired();
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(24);
            entity.Property(x => x.RowVersion).IsRowVersion();
            entity.HasOne<Tenant>().WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<Company>().WithMany().HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => new { x.TenantId, x.CompanyId, x.StartDate, x.EndDate });
            entity.ToTable(t => t.HasCheckConstraint("CK_OperationalPeriods_DateRange", "[StartDate] <= [EndDate]"));
        });
    }
}
