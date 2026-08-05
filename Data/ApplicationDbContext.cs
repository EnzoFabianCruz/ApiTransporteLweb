using Microsoft.EntityFrameworkCore;
using ApiTransporteLweb.Models;

namespace ApiTransporteLweb.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<ParteTrabajo> ParteTrabajos { get; set; }
        public DbSet<ParteTrabajoDetalle> ParteTrabajoDetalles { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Personal> Personal { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ParteTrabajo>().ToTable("PARTETRABAJO");
            modelBuilder.Entity<ParteTrabajoDetalle>().ToTable("PARTETRABAJO_DETALLE");
            modelBuilder.Entity<Usuario>().ToTable("USUARIOS");
            modelBuilder.Entity<Personal>().ToTable("PERSONAL");

            modelBuilder.Entity<ParteTrabajo>()
                .HasKey(p => new { p.CodigoEmp, p.NumeroParte });

            modelBuilder.Entity<ParteTrabajoDetalle>()
                .HasKey(d => new { d.CodigoEmp, d.NumeroParte, d.NumeroLinea });

            modelBuilder.Entity<ParteTrabajoDetalle>()
                .HasOne(d => d.ParteTrabajo)
                .WithMany(p => p.Detalles)
                .HasForeignKey(d => new { d.CodigoEmp, d.NumeroParte });

            modelBuilder.Entity<Personal>()
                .HasKey(p => p.CodigoPersonal);

            base.OnModelCreating(modelBuilder);
        }
    }
}