
using ReservationApp.Data;
using ReservationApp.Enums;
using ReservationApp.Models;

namespace ReservationApp.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(AppDbContext db)
    {
        if (!db.Venues.Any())
        {
            var venue = new Venue { Name = "Merkez Şube", WorkingHours = "09:00-22:00" };
            var table5 = new Resource { Venue = venue, Name = "Masa 5", Capacity = 4 };
            db.AddRange(venue, table5);
        }

        if (!db.Users.Any())
        {
            var admin = new User
            {
                FullName = "Admin Kullanıcı",
                Email = "admin@demo.local",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                Role = Role.Admin
            };
            var user = new User
            {
                FullName = "Demo Kullanıcı",
                Email = "user@demo.local",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("User123!")
            };
            db.AddRange(admin, user);
        }

        await db.SaveChangesAsync();
    }
}

