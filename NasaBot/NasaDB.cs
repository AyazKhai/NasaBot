using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NasaBot.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NasaBot
{
    public class AppDbContext : DbContext
    {
        public DbSet<Nasa> Nasas { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
            Database.EnsureCreated();
        }
    }
}
