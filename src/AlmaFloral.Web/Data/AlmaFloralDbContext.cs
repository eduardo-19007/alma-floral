using AlmaFloral.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace AlmaFloral.Web.Data;

public class AlmaFloralDbContext : DbContext
{
    public AlmaFloralDbContext(
        DbContextOptions<AlmaFloralDbContext> options)
        : base(options)
    {
    }

    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<Pedido> Pedidos => Set<Pedido>();
    public DbSet<DetallePedido> DetallesPedido => Set<DetallePedido>();

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<CasoPostventa> CasosPostventa => Set<CasoPostventa>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Usuario>(e => {
            e.ToTable("Usuario"); e.HasKey(u => u.UsuarioId);
            e.HasIndex(u => u.Correo).IsUnique();
        });
        modelBuilder.Entity<CasoPostventa>(e => {
            e.ToTable("CasoPostventa", t => {
                t.HasCheckConstraint("CK_Caso_Estado", "[Estado] IN ('Abierto', 'En atención', 'Cerrado')");
                t.HasCheckConstraint("CK_Caso_Tipo", "[Tipo] IN ('Consulta', 'Reclamo', 'Solicitud')");
                t.HasCheckConstraint("CK_Caso_Cierre", "[Estado] <> 'Cerrado' OR ([FechaCierre] IS NOT NULL AND [UsuarioId] IS NOT NULL AND [Respuesta] IS NOT NULL AND LEN(LTRIM(RTRIM([Respuesta]))) > 0 AND [Acuerdo] IS NOT NULL AND LEN(LTRIM(RTRIM([Acuerdo]))) > 0)");
            });
            e.HasKey(c => c.CasoPostventaId);
            e.Property(c => c.FechaApertura).HasColumnType("datetime2(0)");
            e.Property(c => c.FechaCierre).HasColumnType("datetime2(0)");
            e.HasOne(c => c.Pedido).WithMany().HasForeignKey(c => c.PedidoId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(c => c.Usuario).WithMany().HasForeignKey(c => c.UsuarioId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.ToTable("Cliente");
            entity.HasKey(cliente => cliente.ClienteId);
        });

        modelBuilder.Entity<Producto>(entity =>
        {
            entity.ToTable("Producto", tabla =>
            {
                tabla.HasCheckConstraint(
                    "CK_Producto_Precio",
                    "[Precio] > 0 AND [Precio] <= 100000");

                tabla.HasCheckConstraint(
                    "CK_Producto_Existencias",
                    "[StockFisico] >= 0 AND [StockReservado] >= 0 " +
                    "AND [StockReservado] <= [StockFisico]");
            });

            entity.HasKey(producto => producto.Id);

            entity.HasData(
                new Producto
                {
                    Id = 1,
                    Nombre = "Ramo de rosas",
                    Descripcion = "Arreglo de rosas para ocasiones especiales.",
                    Precio = 85.00m,
                    StockFisico = 10,
                    StockReservado = 0,
                    Activo = true
                },
                new Producto
                {
                    Id = 2,
                    Nombre = "Orquídea en maceta",
                    Descripcion = "Planta decorativa presentada en maceta.",
                    Precio = 120.00m,
                    StockFisico = 5,
                    StockReservado = 0,
                    Activo = true
                },
                new Producto
                {
                    Id = 3,
                    Nombre = "Arreglo floral mixto",
                    Descripcion = "Composición de flores de distintos colores.",
                    Precio = 95.00m,
                    StockFisico = 0,
                    StockReservado = 0,
                    Activo = true
                });
        });

        modelBuilder.Entity<Pedido>(entity =>
        {
            entity.ToTable("Pedido", tabla =>
            {
                tabla.HasCheckConstraint(
                    "CK_Pedido_Estado",
                    "[Estado] IN ('Pendiente', 'En preparación', " +
                    "'Listo', 'Entregado', 'Cancelado')");

                tabla.HasCheckConstraint(
                    "CK_Pedido_Modalidad",
                    "[ModalidadEntrega] IN ('Delivery', 'Recojo')");

                tabla.HasCheckConstraint(
                    "CK_Pedido_Direccion",
                    "[ModalidadEntrega] <> 'Delivery' OR " +
                    "([DireccionEntrega] IS NOT NULL AND " +
                    "LEN(LTRIM(RTRIM([DireccionEntrega]))) > 0)");
            });

            entity.HasKey(pedido => pedido.PedidoId);

            entity.HasIndex(pedido => pedido.Codigo)
                .IsUnique();

            entity.Property(pedido => pedido.FechaRegistro)
                .HasColumnType("datetime2(0)");

            entity.Property(pedido => pedido.FechaEntregaProgramada)
                .HasColumnType("datetime2(0)");

            entity.HasOne(pedido => pedido.Cliente)
                .WithMany()
                .HasForeignKey(pedido => pedido.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(pedido => pedido.Detalles)
                .WithOne(detalle => detalle.Pedido)
                .HasForeignKey(detalle => detalle.PedidoId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<DetallePedido>(entity =>
        {
            entity.ToTable("DetallePedido", tabla =>
            {
                tabla.HasCheckConstraint(
                    "CK_DetallePedido_Cantidad",
                    "[Cantidad] > 0");

                tabla.HasCheckConstraint(
                    "CK_DetallePedido_Precio",
                    "[PrecioUnitario] > 0 AND [PrecioUnitario] <= 100000");
            });

            entity.HasKey(detalle => detalle.DetallePedidoId);

            entity.HasIndex(detalle => new
            {
                detalle.PedidoId,
                detalle.ProductoId
            }).IsUnique();

            entity.HasOne(detalle => detalle.Producto)
                .WithMany()
                .HasForeignKey(detalle => detalle.ProductoId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}