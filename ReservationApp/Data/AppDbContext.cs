using Microsoft.EntityFrameworkCore;
using ReservationApp.Models;


namespace ReservationApp.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Venue> Venues => Set<Venue>();
    public DbSet<Resource> Resources => Set<Resource>();
    public DbSet<Reservation> Reservations => Set<Reservation>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<User>().HasIndex(u => u.Email).IsUnique();

        b.Entity<Resource>()
            .HasOne(r => r.Venue).WithMany(v => v.Resources).HasForeignKey(r => r.VenueId);

        b.Entity<Reservation>()
            .HasOne(r => r.Resource).WithMany(res => res.Reservations).HasForeignKey(r => r.ResourceId);

        b.Entity<Reservation>()
            .HasOne(r => r.User).WithMany(u => u.Reservations).HasForeignKey(r => r.UserId);

        b.Entity<Reservation>().HasIndex(r => new { r.ResourceId, r.StartTime, r.EndTime });
        b.Entity<Reservation>().Property(r => r.FullName).HasMaxLength(120);
    }
}

