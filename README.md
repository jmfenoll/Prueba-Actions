# estate.service.public

Servicio público para recuperar datos de inmuebles.


## Configuración local
Url: http://localhost:9400

Url localhost: http://localhost:9400

Database: WayToCol


## Configuración desarrollo
Url: https://devapi.waytocol.com/estate

Url localhost: http://localhost:9400

Database: WayToCol


## Configuración producción
Url: https://api.waytocol.com/estate

Url localhost: http://localhost:9400

Database: WayToCol


## Despliegue
Framework: .NET Core 2.1
Desplegar como servicio de linux.

```bash
dotnet restore -s \\172.16.201.90\w2c\Nuget
dotnet publish
```
