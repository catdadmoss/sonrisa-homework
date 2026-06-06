using Microsoft.EntityFrameworkCore;
using AlertNotificationSystem.Models;

namespace AlertNotificationSystem.Data;

public class ApplicationDbContext : DbContext
{
	public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
		: base(options)
	{
	}

	public DbSet<Alert> Alerts { get; set; }
	public DbSet<Subscription> Subscriptions { get; set; }
	public DbSet<NotificationLog> NotificationLogs { get; set; }

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);

		modelBuilder.Entity<Alert>(entity =>
		{
			entity.HasKey(e => e.Id);
			entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
			entity.Property(e => e.Message).IsRequired();
			entity.Property(e => e.Type).HasConversion<string>();
			entity.Property(e => e.Status).HasConversion<string>();
			entity.HasIndex(e => e.Status);
			entity.HasIndex(e => e.CreatedAt);
		});

		modelBuilder.Entity<Subscription>(entity =>
		{
			entity.HasKey(e => e.Id);
			entity.Property(e => e.UserIdentifier).IsRequired().HasMaxLength(100);
			entity.Property(e => e.Destination).IsRequired().HasMaxLength(255);
			entity.Property(e => e.Channel).HasConversion<string>();
			entity.Property(e => e.AlertTypeFilter).HasConversion<string>();
			entity.HasIndex(e => e.IsActive);
			entity.HasIndex(e => new { e.UserIdentifier, e.Channel });
		});

		modelBuilder.Entity<NotificationLog>(entity =>
		{
			entity.HasKey(e => e.Id);
			entity.Property(e => e.Channel).HasConversion<string>();
			entity.HasIndex(e => e.AlertId);
			entity.HasIndex(e => e.SubscriptionId);
			entity.HasIndex(e => e.SentAt);

			entity.HasOne(e => e.Alert)
				.WithMany()
				.HasForeignKey(e => e.AlertId)
				.OnDelete(DeleteBehavior.Cascade);

			entity.HasOne(e => e.Subscription)
				.WithMany()
				.HasForeignKey(e => e.SubscriptionId)
				.OnDelete(DeleteBehavior.Cascade);
		});
	}
}
