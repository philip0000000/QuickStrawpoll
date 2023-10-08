using Microsoft.EntityFrameworkCore;
using QuickStrawpoll.Server.Models;

namespace QuickStrawpoll.Server.Data
{
    public class QuickStrawpollContext : DbContext
    {
        public QuickStrawpollContext(DbContextOptions<QuickStrawpollContext> options) : base(options) { }

        public DbSet<Polls> Polls { get; set; }
        public DbSet<OptionDatas> OptionDatas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Set the default schema for the entire context
            modelBuilder.HasDefaultSchema("QuickStrawpoll");

            modelBuilder.Entity<OptionDatas>()
                .HasOne(p => p.Polls)
                .WithMany(b => b.Options)
                .HasForeignKey(p => p.PollId);
        }
    }
}
