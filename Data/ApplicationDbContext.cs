using CourseWork.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Npgsql;
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
        public DbSet<BookmarkModel> Bookmarks { get; set; }
        public DbSet<UserSettingsModel> Settings { get; set; }
        public DbSet<CommentModel> Comments { get; set; }
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
            Database.EnsureCreated();
        }

        public static string GetNpgsqlConnectionString(string databaseUrl, bool dev)
        {
            var databaseUri = new Uri(databaseUrl);
            var userInfo = databaseUri.UserInfo.Split(':');
            var builder = new NpgsqlConnectionStringBuilder
            {
                Host = databaseUri.Host,
                Port = databaseUri.Port,
                Username = userInfo[0],
                Password = userInfo[1],
                Database = databaseUri.LocalPath.TrimStart('/')
            };

            if (!dev)
            {
                builder.Pooling = true;
                builder.SslMode = SslMode.Require;
                builder.TrustServerCertificate = true;
            }

            return builder.ToString();
        }
    }
}
