using Microsoft.EntityFrameworkCore;
using EnerGym.Api.Models;

namespace EnerGym.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
            
        }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Cesta>()
            .HasOne(c => c.Usuario)
            .WithOne(u => u.Cesta)
            .HasForeignKey<Cesta>(c => c.IdUsuario);

            modelBuilder.Entity<Categoria>()
            .HasKey(c => c.IdCategoria);

            modelBuilder.Entity<Usuario>()
            .HasKey(u => u.IdUsuario);

            modelBuilder.Entity<Pedido>()
                .HasKey(p => p.IdPedido);

            modelBuilder.Entity<Producto>()
                .HasKey(p => p.IdProducto);

            modelBuilder.Entity<Categoria>()
                .HasKey(c => c.IdCategoria);

            modelBuilder.Entity<Cupon>()
                .HasKey(c => c.IdCupon);

            modelBuilder.Entity<Cesta>()
                .HasKey(c => c.IdCesta);

            modelBuilder.Entity<Resenya>()
                .HasKey(r => r.IdResena);

            modelBuilder.Entity<Producto>()
            .Property(p => p.Precio)
            .HasPrecision(18, 2);

            modelBuilder.Entity<Pedido>()
                .Property(p => p.Total)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Cupon>()
                .Property(c => c.Descuento)
                .HasPrecision(5, 2);    

        }   

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Pedido> Pedidos { get; set; }
        public DbSet<Producto> Productos { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Cupon> Cupones { get; set; }
        public DbSet<Cesta> Cestas { get; set; }
        public DbSet<Resenya> Resenas { get; set; }
    }
}
