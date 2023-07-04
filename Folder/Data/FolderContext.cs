using Folder.Models;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using static System.Net.WebRequestMethods;

namespace Folder.Data
{
    public class FolderContext : DbContext
    {
        public FolderContext(DbContextOptions<FolderContext> options)
            : base(options)
        {

        }

        public DbSet<Folders> Folders { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Folders>()
           .Property(f => f.Id)
           .ValueGeneratedOnAdd();

            // Seed data for Folders
            modelBuilder.Entity<Folders>().HasData(
                new Folders { Id = 4, FolderName = "Creating Digital Images", ParrentFolderId = null },
                new Folders { Id = 5, FolderName = "Resources", ParrentFolderId = 4 },
                new Folders { Id = 6, FolderName = "Evidence", ParrentFolderId = 4 },
                new Folders { Id = 7, FolderName = "Graphic Products", ParrentFolderId = 4 },
                new Folders { Id = 8, FolderName = "Primary Sources", ParrentFolderId = 5 },
                new Folders { Id = 9, FolderName = "Secondary Sources", ParrentFolderId = 5 },
                new Folders { Id = 10, FolderName = "Process", ParrentFolderId = 7 },
                new Folders { Id = 11, FolderName = "Final Product", ParrentFolderId = 7 }
            );
        }
    }
}
