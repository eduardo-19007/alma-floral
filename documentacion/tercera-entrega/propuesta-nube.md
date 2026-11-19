# Propuesta de alojamiento en la nube — Alma Floral

## Alcance

Se presenta una alternativa de alojamiento para una etapa posterior.
En este avance no se han contratado servicios ni desplegado
la aplicación en internet.

## Arquitectura propuesta

Navegador del cliente
→ conexión HTTPS
→ aplicación ASP.NET Core MVC en Azure App Service
→ base de datos en Azure SQL Database.

GitHub conserva el código y GitHub Actions realiza
la restauración, compilación, pruebas y generación
del paquete publicable.

## Componentes

| Componente | Servicio propuesto | Función |
|---|---|---|
| Aplicación web | Azure App Service | Alojar la aplicación ASP.NET Core MVC. |
| Base de datos | Azure SQL Database | Almacenar clientes, productos, pedidos y postventa. |
| Código fuente | GitHub | Conservar versiones y gestionar cambios. |
| Validación y paquete | GitHub Actions | Ejecutar las comprobaciones y preparar la entrega. |

## Relación con el diseño anterior

SQL Server Express se mantiene como alternativa prevista
para desarrollo local.

Azure SQL Database se propone para el futuro entorno en la nube.
No corresponde a una instalación de SQL Server Express.
Antes del despliegue se deberá validar la compatibilidad
del esquema y de las consultas.

## Pasos pendientes para un despliegue

1. Implementar y comprobar la conexión con la base de datos.
2. Crear el esquema y validar las reglas de integridad.
3. Implementar autenticación y autorización.
4. Configurar los servicios y el entorno de producción.
5. Guardar la configuración sensible fuera del repositorio.
6. Desplegar una versión validada y comprobar su funcionamiento.
7. Establecer copias de seguridad, registros y recuperación.
8. Revisar los costos antes de contratar recursos.

## Estado demostrado en el avance 03

- Seguimiento de una tarea mediante GitHub Issues.
- Compilación y tres pruebas de integración automatizadas.
- Generación de un paquete descargable tras superar las pruebas.
- Modelo relacional y diccionario de datos documentados.

El paquete generado constituye la preparación automatizada
para la entrega. No acredita un despliegue en Azure
ni una aplicación accesible públicamente.

## Referencia

Microsoft. Tutorial: Deploy an ASP.NET Core and Azure SQL Database app.
https://learn.microsoft.com/azure/app-service/tutorial-dotnetcore-sqldb-app
