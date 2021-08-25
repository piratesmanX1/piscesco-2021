using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Piscesco.Models;

namespace Piscesco.Data
{
    public class PiscescoModelContext : DbContext
    {
        public PiscescoModelContext (DbContextOptions<PiscescoModelContext> options)
            : base(options)
        {
        }

        public DbSet<Piscesco.Models.Stall> Stall { get; set; }

        public DbSet<Piscesco.Models.Product> Product { get; set; }

        public DbSet<Piscesco.Models.FeaturedProduct> FeaturedProduct { get; set; }

        public DbSet<Piscesco.Models.Order> Order { get; set; }

        public DbSet<Piscesco.Models.FeedbackEntity> FeedbackEntity { get; set; }
    }
}
