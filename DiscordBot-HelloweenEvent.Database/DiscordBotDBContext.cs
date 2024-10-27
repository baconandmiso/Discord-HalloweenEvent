using DiscordBot_HelloweenEvent.Database.Models;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;

namespace DiscordBot_HelloweenEvent.Database;

public class DiscordBotDBContext : DbContext
{
    private readonly string _connectionString;

    public DiscordBotDBContext(MySqlConnectionStringBuilder connectionStringBuilder)
    {
        _connectionString = connectionStringBuilder.ConnectionString;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseMySQL(_connectionString);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EventPoint>().ToSqlQuery("SELECT * FROM event.points");
    }

    public DbSet<EventPoint> EventPoints { get; set; }
}
