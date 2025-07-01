using LibSystem.Identity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibSystem.Identity.Context
{
    public class IdentityDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, int>
    {
        public IdentityDbContext(DbContextOptions<IdentityDbContext> options)
            : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            //Conigure table names to match the existing database schema
            builder.Entity<ApplicationUser>().ToTable("Users");
            builder.Entity<ApplicationRole>().ToTable("Roles");
            builder.Entity<IdentityUserRole<int>>().ToTable("UserRoles");
            builder.Entity<IdentityUserClaim<int>>().ToTable("UserClaims");
            builder.Entity<IdentityUserLogin<int>>().ToTable("UserLogins");
            builder.Entity<IdentityRoleClaim<int>>().ToTable("RoleClaims");
            builder.Entity<IdentityUserToken<int>>().ToTable("UserTokens");

            // Configure ApplicationUser properties
            builder.Entity<ApplicationUser>(entity =>
            {
                entity.Property(e => e.FirstName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.LastName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");

                entity.Property(e => e.UpdatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");

                entity.Property(e => e.IsActive)
                    .HasDefaultValue(true);

                //Index on MemberId for linking with domain entities
                entity.HasIndex(u => u.MemberId)
                    .IsUnique()
                    .HasFilter("[MemberId] IS NOT NULL");

                // Computed column for FullName (optional)
                entity.Property(u => u.FullName)
                    .HasComputedColumnSql("[FirstName] + ' ' + [LastName]", stored: true);
            });

            // Configure ApplicationRole
            builder.Entity<ApplicationRole>(entity =>
            {
                entity.Property(r => r.Description)
                    .HasMaxLength(500);

                entity.Property(r => r.CreatedAt)
                    .IsRequired()
                    .HasDefaultValueSql("GETUTCDATE()");

                entity.Property(r => r.IsActive)
                    .IsRequired()
                    .HasDefaultValue(true);
            });

            // Seed default roles
            SeedRoles(builder);
        }
        private static void SeedRoles(ModelBuilder builder)
        {
            var roles = new[]
            {
                new ApplicationRole
                {
                    Id = 1,
                    Name = Constants.ApplicationRoles.Member,
                    NormalizedName = Constants.ApplicationRoles.Member.ToUpper(),
                    Description = Constants.ApplicationRoles.RoleDescriptions[Constants.ApplicationRoles.Member],
                    ConcurrencyStamp = Guid.NewGuid().ToString()
                },
                new ApplicationRole
                {
                    Id = 2,
                    Name = Constants.ApplicationRoles.MinorStaff,
                    NormalizedName = Constants.ApplicationRoles.MinorStaff.ToUpper(),
                    Description = Constants.ApplicationRoles.RoleDescriptions[Constants.ApplicationRoles.MinorStaff],
                    ConcurrencyStamp = Guid.NewGuid().ToString()
                },
                new ApplicationRole
                {
                    Id = 3,
                    Name = Constants.ApplicationRoles.ManagementStaff,
                    NormalizedName = Constants.ApplicationRoles.ManagementStaff.ToUpper(),
                    Description = Constants.ApplicationRoles.RoleDescriptions[Constants.ApplicationRoles.ManagementStaff],
                    ConcurrencyStamp = Guid.NewGuid().ToString()
                },
                new ApplicationRole
                {
                    Id = 4,
                    Name = Constants.ApplicationRoles.Administrator,
                    NormalizedName = Constants.ApplicationRoles.Administrator.ToUpper(),
                    Description = Constants.ApplicationRoles.RoleDescriptions[Constants.ApplicationRoles.Administrator],
                    ConcurrencyStamp = Guid.NewGuid().ToString()
                }
            };

            builder.Entity<ApplicationRole>().HasData(roles);
        }
    }
}