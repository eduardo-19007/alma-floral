# Funcionamiento del sistema Alma Floral

## Módulos implementados

- Catálogo persistente de productos activos, precios y disponibilidad.
- Solicitud de pedidos y reserva transaccional; precio unitario conservado en el detalle.
- Consulta por código aleatorio y teléfono, sin exponer datos personales de contacto.
- Acceso administrativo con cookie, contraseñas con hash, rol y verificación de cuenta activa.
- Registro y edición de productos; desactivación que conserva las relaciones históricas.
- Atención de pedidos: Pendiente → En preparación → Listo → Entregado. Solo Pendiente permite Cancelado.
- Cancelación: libera reserva sin reducir stock físico. Entrega: reduce stock físico y reservado una sola vez.
- Postventa: apertura por código y teléfono, atención administrativa y cierre con respuesta, acuerdo, responsable y fecha.
- Seguimiento del caso por el cliente desde Mi pedido.

## Preparación y ejecución

Requiere SDK .NET 10 y SQL Server. Configurar ConnectionStrings:AlmaFloral para la instancia local. En desarrollo se usa autenticación de Windows; no se guarda contraseña de SQL Server en el repositorio. TrustServerCertificate=True es una configuración local.

```powershell
dotnet tool restore
dotnet ef database update --project .\src\AlmaFloral.Web\AlmaFloral.Web.csproj -- --environment Development
dotnet user-secrets set "Administracion:Correo" "correo-del-administrador" --project .\src\AlmaFloral.Web\AlmaFloral.Web.csproj
dotnet user-secrets set "Administracion:ContrasenaInicial" "una-contraseña-privada-de-al-menos-12-caracteres" --project .\src\AlmaFloral.Web\AlmaFloral.Web.csproj
dotnet run --project .\src\AlmaFloral.Web\AlmaFloral.Web.csproj --launch-profile http
```

La cuenta inicial se crea en Development si el correo no existe. La clave se almacena como hash en Usuario; la configuración privada permanece fuera de Git. No hay un usuario o contraseña universal incorporados al código. Las migraciones se aplican explícitamente, no durante cada arranque.

Rutas: `/Catalogo`, `/Pedidos/Consultar`, `/Postventa/Nueva`, `/Cuenta/Ingresar` y `/Administracion`.

## Pruebas y simulaciones

```powershell
# Detener antes la aplicación local con Ctrl+C para evitar bloquear el ejecutable.
dotnet test .\tests\AlmaFloral.Web.Tests\AlmaFloral.Web.Tests.csproj
```

La suite contiene 33 casos aprobados. Usa SQLite en memoria para aislar los datos de prueba. Las simulaciones documentadas en `simulaciones.md` se ejecutaron además sobre SQL Server Express, con datos ficticios. Este contraste no equivale a pruebas de carga ni a una certificación de seguridad.

## Reglas y configuración

La disponibilidad es StockFisico − StockReservado. Las actualizaciones condicionales y transacciones protegen las reservas y el cambio de estado. La interfaz de solicitud registra un producto por pedido; el esquema admite varios detalles. El catálogo no permite solicitar productos agotados o inactivos. Los estados finales no pueden volver a descontar o liberar existencias.

Los formularios de escritura usan antifalsificación; los controladores administrativos exigen rol Administrador. La sesión dura 30 minutos con renovación por actividad. En producción la cookie exige HTTPS. El ingreso y el registro de postventa limitan solicitudes por IP. Las fechas se guardan en UTC y la entrega se muestra en hora de Lima.

## Alcance operativo

El circuito funcional de catálogo, pedido, inventario y postventa se encuentra implementado. No se procesan pagos bancarios ni reembolsos automáticos; una respuesta de postventa registra la resolución y no altera el inventario. La tarifa de delivery se coordina aparte. No se ha publicado una instancia de producción ni ejecutado pruebas de carga. Las credenciales, copias de seguridad y el alojamiento de producción requieren su propia configuración.
