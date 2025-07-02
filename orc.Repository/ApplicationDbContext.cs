using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using orc.core.Models;
using orc.Infrastructure;

namespace orc.Infrastructure;

public partial class ApplicationDbContext : DbContext
{
    public ApplicationDbContext()
    {
    }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<National> Nationals { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server =.;Database=orc;Trusted_Connection=True; TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<National>(entity =>
        {
            entity.ToTable("National");

            entity.Property(e => e.Id).HasMaxLength(5);
            entity.Property(e => e.Address).HasMaxLength(200);
            entity.Property(e => e.Government).HasMaxLength(100);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.NationalNumber)
                .HasMaxLength(14)
                .IsFixedLength()
                .HasColumnName("National_Number");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
