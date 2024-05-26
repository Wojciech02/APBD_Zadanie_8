public class YourDbContext : DbContext
{
    public YourDbContext(DbContextOptions<YourDbContext> options) : base(options) { }

    public DbSet<Client> Clients { get; set; }
    public DbSet<Trip> Trips { get; set; }
    public DbSet<ClientTrip> ClientTrips { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<ClientTrip>()
            .HasKey(ct => new { ct.IdClient, ct.IdTrip });
    }
}