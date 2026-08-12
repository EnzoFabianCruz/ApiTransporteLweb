using Microsoft.EntityFrameworkCore;
using ApiTransporteLweb.Models;

namespace ApiTransporteLweb.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<ParteTrabajo> ParteTrabajos { get; set; }
        public DbSet<ParteTrabajoDetalle> ParteTrabajoDetalles { get; set; }
        public DbSet<ParteParada> ParteParadas { get; set; }
        public DbSet<ParteParadaDetalle> ParteParadaDetalles { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Personal> Personal { get; set; }
        public DbSet<PuestoTrabajo> PuestosTrabajo { get; set; }
        public DbSet<UnidadesTransporte> UnidadesTransporte { get; set; }
        public DbSet<UnidadesCiclos> UnidadesCiclos { get; set; }
        public DbSet<CiclosPuntos> CiclosPuntos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ParteTrabajo>().ToTable("PARTETRABAJO");
            modelBuilder.Entity<ParteTrabajoDetalle>().ToTable("PARTETRABAJO_DETALLE");
            modelBuilder.Entity<ParteParada>().ToTable("PARTEPARADA");
            modelBuilder.Entity<ParteParadaDetalle>().ToTable("PARTEPARADA_DETALLE");
            modelBuilder.Entity<Usuario>().ToTable("USUARIOS");
            modelBuilder.Entity<Personal>().ToTable("PERSONAL");
            modelBuilder.Entity<PuestoTrabajo>().ToTable("PUESTOTRABAJO");
            modelBuilder.Entity<UnidadesTransporte>().ToTable("UNIDADESTRANSPORTE");
            modelBuilder.Entity<UnidadesCiclos>().ToTable("UNIDADESCICLOS");
            modelBuilder.Entity<CiclosPuntos>().ToTable("CICLOSPUNTOS");

            modelBuilder.Entity<ParteTrabajo>()
                .HasKey(p => new { p.CodigoEmp, p.NumeroParte });

            modelBuilder.Entity<ParteTrabajoDetalle>()
                .HasKey(d => new { d.CodigoEmp, d.NumeroParte, d.NumeroLinea });

            modelBuilder.Entity<ParteTrabajoDetalle>()
                .HasOne(d => d.ParteTrabajo)
                .WithMany(p => p.Detalles)
                .HasForeignKey(d => new { d.CodigoEmp, d.NumeroParte });

            modelBuilder.Entity<ParteParada>()
                .HasKey(p => new { p.CodigoEmp, p.NumeroParada });

            modelBuilder.Entity<ParteParadaDetalle>()
                .HasKey(d => new { d.CodigoEmp, d.NumeroParada, d.NumeroLinea });

            modelBuilder.Entity<ParteParadaDetalle>()
                .HasOne(d => d.ParteParada)
                .WithMany(p => p.Detalles)
                .HasForeignKey(d => new { d.CodigoEmp, d.NumeroParada });

            modelBuilder.Entity<Personal>()
                .HasKey(p => p.CodigoPersonal);

            modelBuilder.Entity<PuestoTrabajo>()
                .HasKey(p => new { p.CodigoEmp, p.CodigoPuesto });

            modelBuilder.Entity<UnidadesTransporte>()
                .HasKey(u => new { u.CodigoEmp, u.CodigoUnidad });

            modelBuilder.Entity<UnidadesCiclos>()
                .HasKey(u => new { u.CodigoEmp, u.CodigoCiclo });

            modelBuilder.Entity<CiclosPuntos>()
                .HasKey(c => new { c.CodigoEmp, c.CodigoPunto });

            base.OnModelCreating(modelBuilder);
        }
    }
}