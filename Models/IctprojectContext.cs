using System;
using System.Collections.Generic;
using ICTCapstoneProject.Models;
using Microsoft.EntityFrameworkCore;

namespace ICTCapstoneProject.Data;

public partial class IctprojectContext : DbContext
{
    public IctprojectContext()
    {
    }

    public IctprojectContext(DbContextOptions<IctprojectContext> options)
        : base(options)
    {
    }

    public virtual DbSet<User> Users { get; set; }
    /*
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=LAPTOP-2A9L5TJJ\\SQLEXPRESS;Initial Catalog=ICTProject;Integrated Security=True;Encrypt=True;Trust Server Certificate=True");
*/
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CCAC534A94E5");

            entity.HasIndex(e => e.Email, "UQ__Users__A9D10534D63F3A8A").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.Password).HasMaxLength(255);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
