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
        public DbSet<VwOperacionesMaterial> VwOperacionesMateriales { get; set; }
        public DbSet<VwOperacionesMotivo> VwOperacionesMotivos { get; set; }

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

            // La columna real en la BD tiene un typo: "observaciond" (con "d" extra al final).
            // Se mapea explícitamente para mantener el nombre de propiedad limpio en C#.
            modelBuilder.Entity<ParteParadaDetalle>()
                .Property(d => d.Observacion)
                .HasColumnName("observaciond");

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

            // Vista de solo lectura: sin clave primaria (Keyless)
            modelBuilder.Entity<VwOperacionesMaterial>(entity =>
            {
                entity.HasNoKey();
                entity.ToView("VW_Operaciones_material");

                entity.Property(e => e.Codigom).HasColumnName("codigom");
                entity.Property(e => e.Nombrem).HasColumnName("nombrem");
                entity.Property(e => e.Abreviatura).HasColumnName("abreviatura");
            });

            // Vista de solo lectura: sin clave primaria (Keyless)
            modelBuilder.Entity<VwOperacionesMotivo>(entity =>
            {
                entity.HasNoKey();
                entity.ToView("VW_Operaciones_motivo");

                entity.Property(e => e.Codigo).HasColumnName("Codigo");
                entity.Property(e => e.Motivo).HasColumnName("Motivo");
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}