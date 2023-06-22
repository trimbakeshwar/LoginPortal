
using LoginMVCProject.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace AppDbContext
{
    public class LoginContext : DbContext
    {
        public LoginContext(DbContextOptions options) : base(options)
        {

        }
        public DbSet<Registration> Registrations { get; set; }
        public DbSet<PasswordLogHistory> PasswordLogHistorys { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            base.OnModelCreating(modelBuilder);

        }
    }
}
