using TaskMangment.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskMangment.Infrastructure.DataContext
{
    public class AppDbContext : IdentityDbContext<SystemUser, SystemRole, string>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
           : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<SystemUser>(b =>
            {
                b.Property(u => u.FullName).HasMaxLength(100);
            });
        }
    }
}
