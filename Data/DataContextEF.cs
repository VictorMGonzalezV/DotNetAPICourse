using DotnetAPI.Models;
using Microsoft.EntityFrameworkCore;

    namespace DotNetAPI.Data
{
    public class DataContextEF: DbContext
    {
         private readonly IConfiguration _config;

         public DataContextEF(IConfiguration config)
         {
            _config=config;
         }

         public virtual DbSet<User> Users{get; set;}

         public virtual DbSet<UserSalary> UserSalary{get;set;}

         public virtual DbSet<UserJobInfo> UserJobInfo{get;set;}

         protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
         {
            if(!optionsBuilder.IsConfigured)
            {
                //For this to work, we need to install the EntityFramework.SqlServer package
                optionsBuilder.UseSqlServer(_config.GetConnectionString("DefaultConnection"),optionsBuilder=>optionsBuilder.EnableRetryOnFailure());
            }
         }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //For HasDefaultSchema to work, it's necsssary to add the EntityFrameworkCore.Relational package as well
            modelBuilder.HasDefaultSchema("TutorialAppSchema");

            modelBuilder.Entity<User>()
                .ToTable("Users","tutorialAppSchema")
                .HasKey(u=>u.UserId);

            modelBuilder.Entity<UserSalary>()
                .HasKey(u=>u.UserId);

            modelBuilder.Entity<UserJobInfo>()
                .HasKey(u=>u.UserId);
        }

    }   
}