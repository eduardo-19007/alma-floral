# Publicación de Alma Floral en IIS con SQL Server

Estado: preparación local. Este documento no acredita un despliegue realizado.

## Preparar el alojamiento

Crear una cuenta gratuita, un sitio ASP.NET Core .NET 10 y una base SQL Server en MonsterASP.NET. Confirmar HTTPS con soporte antes de usar el acceso administrativo: la oferta gratuita publica condiciones distintas entre sus páginas de planes. No introducir datos personales reales durante la demostración.

## Generar los archivos

Desde la raíz ejecutar `./scripts/Preparar-Despliegue.ps1` en PowerShell con SDK y herramienta dotnet-ef 10 instalados. El proceso ejecuta todas las pruebas, publica una aplicación dependiente del runtime .NET 10 y genera un SQL idempotente. No incluye la configuración Development ni secretos de usuario. El hosting debe tener el runtime ASP.NET Core 10 para Windows/IIS instalado.

## Base remota

Conectar SSMS con las credenciales del proveedor y seleccionar expresamente la base remota asignada. Respaldarla antes de modificarla. Revisar y ejecutar `base-datos.sql` allí; nunca en otras bases. El script registra las migraciones y crea las tablas y los productos iniciales definidos por el proyecto. No exporta pedidos, clientes ni contraseñas locales. No se aplican migraciones automáticamente al iniciar la web.

## Configuración privada del servidor

Configurar mediante el panel o variables del proceso:

| Variable | Valor |
| --- | --- |
| ASPNETCORE_ENVIRONMENT | Production |
| ConnectionStrings__AlmaFloral | Conexión SQL proporcionada por el hosting, con base, usuario, contraseña y cifrado según sus instrucciones |
| AllowedHosts | Nombre del sitio asignado, sin protocolo ni ruta |

No usar Windows Authentication ni la dirección SQLEXPRESS del equipo local en el servidor. No publicar contraseñas en GitHub. Si el alojamiento requiere web.config para variables, editar únicamente la copia del servidor y conservarla fuera del repositorio. Mantener HTTPS y las cookies seguras de producción; no cambiar a Development para eludir errores. El paquete contiene web.config generado para IIS. No desactivar la comprobación de certificados de SQL sin confirmar la configuración del proveedor.

## Administrador inicial

La inicialización automática continúa limitada a Development. En Production se dispone de un comando explícito de mantenimiento, que termina sin abrir el servidor:

`dotnet AlmaFloral.Web.dll --crear-administrador`

Antes de ejecutarlo, aplicar las migraciones y establecer de forma privada `Administracion__Correo` y `Administracion__ContrasenaInicial` (al menos 12 caracteres), además de la conexión SQL remota y el entorno Production. El comando puede ejecutarse desde una consola privada del servidor o desde una copia local de la publicación conectada expresamente a la base remota. En este último caso cambiar al directorio web publicado y ejecutar con esas variables solo en ese proceso; no modifica la conexión de desarrollo ni necesita User Secrets. No pasar la contraseña como argumento. Retirar las variables iniciales al finalizar. Si el correo existe, no reemplaza su contraseña ni cambia su rol. No ejecutar desde una página pública ni crear una ruta HTTP de inicialización.

## Subir y comprobar

1. Subir el contenido de `web` o descomprimir `alma-floral-web.zip` en la raíz del sitio IIS. No subir la carpeta contenedora, SQL, guía, fuentes ni secretos.
2. Configurar HTTPS y Production; reiniciar desde el panel.
3. Comprobar inicio y catálogo con los tres productos iniciales.
4. Ingresar como administrador y crear un producto de demostración.
5. Registrar un pedido con datos ficticios y verificar reserva, consulta con código/teléfono y rechazo de un teléfono incorrecto.
6. Probar entrega y cancelación en pedidos distintos; comprobar el inventario y la postventa.
7. Cerrar sesión, comprobar que Administración requiere ingreso y revisar la visualización móvil.
8. Respaldar la base remota, guardar la versión publicada y conservar un paquete anterior para recuperación. Las restauraciones deben ser compatibles con el esquema; no borrar registros para repetir pruebas.

## Evidencias al terminar

Capturar la dirección pública con HTTPS y el catálogo, un pedido de demostración, inventario, postventa y el flujo de publicación. Ocultar credenciales. Registrar fecha, commit y resultado real de cada comprobación. Hasta completar estos pasos, el informe debe seguir describiendo el alojamiento como propuesta y el paquete como preparación de entrega.

El plan gratuito tiene recursos limitados; verificar disponibilidad y condiciones en el panel. La primera publicación puede ser manual. El flujo actual de GitHub Actions prepara el paquete, pero no instala por sí mismo en el hosting.

## Referencias de configuración

- https://help.monsterasp.net/websites/configuration/environment-variables
- https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/applying
