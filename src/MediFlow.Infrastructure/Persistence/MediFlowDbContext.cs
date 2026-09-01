using MediFlow.Domain.Common;
using MediFlow.Domain.Entities;
using MediFlow.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace MediFlow.Infrastructure.Persistence;

public class MediFlowDbContext(DbContextOptions<MediFlowDbContext> options) : DbContext(options)
{
    public DbSet<Claim> Claims => Set<Claim>();
    public DbSet<ClaimLineItem> ClaimLineItems => Set<ClaimLineItem>();
    public DbSet<Provider> Providers => Set<Provider>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Ignore<DomainEvent>();

        // Provider configuration
        modelBuilder.Entity<Provider>(builder =>
        {
            builder.HasKey(p => p.Id);
            
            builder.Property(p => p.Npi)
                .HasConversion(
                    npi => npi.Value,
                    value => NationalProviderId.Create(value))
                .HasMaxLength(10)
                .IsRequired();

            builder.HasIndex(p => p.Npi).IsUnique();

            builder.Property(p => p.FirstName).HasMaxLength(100).IsRequired();
            builder.Property(p => p.LastName).HasMaxLength(100).IsRequired();
            builder.Property(p => p.Specialty).HasMaxLength(100).IsRequired();
            builder.Property(p => p.TaxonomyCode).HasMaxLength(50).IsRequired();
            builder.Property(p => p.State).HasMaxLength(2).IsRequired();
        });

        // Claim configuration
        modelBuilder.Entity<Claim>(builder =>
        {
            builder.HasKey(c => c.Id);
            builder.HasIndex(c => c.ClaimNumber).IsUnique();
            builder.Property(c => c.ClaimNumber).HasMaxLength(50).IsRequired();
            builder.Property(c => c.PatientId).HasMaxLength(50).IsRequired();
            builder.Property(c => c.PatientName).HasMaxLength(100).IsRequired();

            // Value Objects mappings
            builder.OwnsOne(c => c.PrimaryDiagnosis, diag =>
            {
                diag.Property(d => d.Code).HasColumnName("PrimaryDiagnosisCode").HasMaxLength(20).IsRequired();
                diag.Property(d => d.Description).HasColumnName("PrimaryDiagnosisDescription").HasMaxLength(250);
            });

            builder.OwnsOne(c => c.TotalBilledAmount, money =>
            {
                money.Property(m => m.Amount).HasColumnName("TotalBilledAmount").HasPrecision(18, 2).IsRequired();
                money.Property(m => m.Currency).HasColumnName("BilledCurrency").HasMaxLength(3).IsRequired();
            });

            builder.OwnsOne(c => c.ApprovedAmount, money =>
            {
                money.Property(m => m.Amount).HasColumnName("ApprovedAmount").HasPrecision(18, 2).IsRequired();
                money.Property(m => m.Currency).HasColumnName("ApprovedCurrency").HasMaxLength(3).IsRequired();
            });

            builder.HasOne(c => c.Provider)
                .WithMany(p => p.Claims)
                .HasForeignKey(c => c.ProviderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(c => c.LineItems)
                .WithOne(li => li.Claim)
                .HasForeignKey(li => li.ClaimId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ClaimLineItem configuration
        modelBuilder.Entity<ClaimLineItem>(builder =>
        {
            builder.HasKey(li => li.Id);
            builder.Property(li => li.ProcedureCode).HasMaxLength(20).IsRequired();
            builder.Property(li => li.Description).HasMaxLength(200);

            builder.OwnsOne(li => li.BilledAmount, money =>
            {
                money.Property(m => m.Amount).HasColumnName("BilledAmount").HasPrecision(18, 2).IsRequired();
                money.Property(m => m.Currency).HasColumnName("Currency").HasMaxLength(3).IsRequired();
            });
        });

        // Outbox Message configuration
        modelBuilder.Entity<OutboxMessage>(builder =>
        {
            builder.HasKey(o => o.Id);
            builder.Property(o => o.EventType).HasMaxLength(200).IsRequired();
            builder.Property(o => o.Payload).IsRequired();
            builder.HasIndex(o => o.ProcessedAtUtc);
        });
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Intercept Domain Events and convert into Transactional Outbox Messages
        var domainEntities = ChangeTracker
            .Entries<BaseEntity>()
            .Where(x => x.Entity.DomainEvents.Any())
            .Select(x => x.Entity)
            .ToList();

        var domainEvents = domainEntities
            .SelectMany(x => x.DomainEvents)
            .ToList();

        domainEntities.ForEach(entity => entity.ClearDomainEvents());

        foreach (var domainEvent in domainEvents)
        {
            var outboxMessage = new OutboxMessage
            {
                EventType = domainEvent.GetType().AssemblyQualifiedName ?? domainEvent.GetType().Name,
                Payload = JsonSerializer.Serialize(domainEvent, domainEvent.GetType()),
                CreatedAtUtc = DateTime.UtcNow
            };
            OutboxMessages.Add(outboxMessage);
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
