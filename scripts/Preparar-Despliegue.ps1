[CmdletBinding()]
param()
$ErrorActionPreference = 'Stop'
$raiz = Split-Path $PSScriptRoot -Parent
$proyecto = Join-Path $raiz 'src/AlmaFloral.Web/AlmaFloral.Web.csproj'
$pruebas = Join-Path $raiz 'tests/AlmaFloral.Web.Tests/AlmaFloral.Web.Tests.csproj'
$destino = Join-Path $raiz ('artifacts/despliegue-' + (Get-Date -Format 'yyyyMMdd-HHmmss'))
$web = Join-Path $destino 'web'
New-Item -ItemType Directory -Path $web -Force | Out-Null
dotnet test $pruebas --configuration Release -p:UseSharedCompilation=false --disable-build-servers
if ($LASTEXITCODE -ne 0) { throw 'Las pruebas fallaron. No se genera el paquete.' }
dotnet publish $proyecto --configuration Release --no-restore --output $web -p:UseAppHost=false -p:UseSharedCompilation=false --disable-build-servers
if ($LASTEXITCODE -ne 0) { throw 'La publicación falló.' }
# La configuración local no debe distribuirse al servidor.
$desarrollo = Join-Path $web 'appsettings.Development.json'
if (Test-Path -LiteralPath $desarrollo) { Remove-Item -LiteralPath $desarrollo }
dotnet ef migrations script --idempotent --no-build --configuration Release --project $proyecto --output (Join-Path $destino 'base-datos.sql') -- --environment Production --ConnectionStrings:AlmaFloral 'Server=localhost;Database=AlmaFloralDb;Integrated Security=True;Encrypt=True'
if ($LASTEXITCODE -ne 0) { throw 'No se pudo generar el script de migración.' }
Copy-Item -LiteralPath (Join-Path $raiz 'documentacion/despliegue/GUIA.md') -Destination $destino
Compress-Archive -Path (Join-Path $web '*') -DestinationPath (Join-Path $destino 'alma-floral-web.zip')
Write-Host "Paquete preparado en: $destino"
Write-Host 'Subir solo el contenido de web al sitio. El SQL y la guía quedan fuera de la raíz pública.'
