using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NGCore_Blog.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NGCore_Blog.Data
{
    public class ApplicationDbContext :IdentityDbContext<IdentityUser> 
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }
        //Create Roles to Database by overriding the ApplicationDbContext
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            //By Default Our IdentityRole Table(Created By Default by Microsoft AspNetCore Identity Package)
            builder.Entity<IdentityRole>().HasData(
                new { Id = "1", Name = "Admin", NormalizedName = "ADMIN" },
                  new { Id = "2", Name = "Customer", NormalizedName = "CUSTOMER" },
                    new { Id = "3", Name = "Moderator", NormalizedName = "MODERATOR" }
                );
        }

        //for crud Operation 
        public DbSet<ProductModel> Products { get; set; }

        //commands
        //dotnet ef migrations add InitialCreate
        //add migration
        //Enable-Migrations -StartUpProjectName NGCore_Blog -ContextTypeName NGCore_Blog.Data.ApplicationDbContext -Verbose

    }
}
