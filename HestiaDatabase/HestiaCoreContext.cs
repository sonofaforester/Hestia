using HestiaDatabase.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace HestiaDatabase
{
    public class HestiaCoreContext : DbContext
    {
        private readonly IConfiguration configuration;

        public DbSet<TicketModel> Tickets { get; set; }

        public HestiaCoreContext(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
    }
}
