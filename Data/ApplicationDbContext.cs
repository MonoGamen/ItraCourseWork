using CourseWork.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace CourseWork.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public DbSet<FanficModel> Fanfics { get; set; }
        public DbSet<ChapterModel> Chapters { get; set; }
        public DbSet<TagModel> Tags { get; set; }
        public DbSet<FandomModel> Fandoms { get; set; }
        public DbSet<MarkModel> Marks { get; set; }
        public DbSet<LikeModel> Likes { get; set; }
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
            Database.EnsureCreated();
        }
    }
}
