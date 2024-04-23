using CalenderBooking;
using Microsoft.EntityFrameworkCore;

public class BookingContext : DbContext
{
    public DbSet<Booking> Booking { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=BookingDb;Trusted_Connection=True;MultipleActiveResultSets=true");
    }
}