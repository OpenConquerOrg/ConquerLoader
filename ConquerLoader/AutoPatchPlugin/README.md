# AutoPatch Plugin

## Que hace

`AutoPatchPlugin` es un plugin opcional para `ConquerLoader` que ejecuta un autoparche **antes** de abrir el cliente.

Su objetivo es sencillo:

1. leer una configuracion propia del plugin
2. cargar un manifiesto de parche
3. comparar archivos locales con lo que pide el manifiesto
4. descargar o copiar solo los archivos necesarios
5. continuar con el launch normal del cliente

Si el plugin esta activado y configurado para bloquear el arranque en caso de error, puede cancelar el `launch` y mostrar el motivo al usuario.

## Como se integra con el loader

El loader ahora tiene un punto de extension de pre-lanzamiento:

- `ConquerLoader/Core.cs`
- `CLCore/PluginSystem/IPreLaunchPlugin.cs`
- `CLCore/PluginSystem/PluginPreLaunchContext.cs`
- `CLCore/PluginSystem/PluginPreLaunchResult.cs`

Flujo real:

1. `ConquerLoader` carga los DLL de `Plugins`
2. crea las instancias que implementan `IPlugin`
3. ejecuta `Init()` como siempre
4. justo antes de `Process.Start(...)`, el loader llama a `Core.RunPreLaunchPlugins(...)`
5. solo los plugins que implementan `IPreLaunchPlugin` participan en ese paso
6. si alguno devuelve `ContinueLaunch = false`, el cliente no se abre

Esto mantiene compatibilidad con plugins antiguos: si solo implementan `IPlugin`, siguen funcionando igual.

## Configuracion del plugin

El plugin guarda su configuracion en:

`Plugins/AutoPatchPlugin.settings.json`

Campos:

- `Enabled`: activa o desactiva el autoparche
- `ManifestLocation`: URL HTTP/HTTPS o ruta local a un JSON de manifiesto
- `FailLaunchOnError`: si es `true`, cancela el launch cuando el parche falla
- `RelativeTargetFolder`: carpeta relativa opcional dentro del `working directory` del cliente

La configuracion se edita desde la ventana WPF del plugin:

- `AutoPatchPlugin/AutoPatchConfigurationWindow.xaml`
- `AutoPatchPlugin/AutoPatchConfigurationWindow.xaml.cs`

## Manifiesto de parche

El plugin espera un JSON con este formato:

```json
{
  "version": "1.0.0",
  "baseUrl": "https://cdn.example.com/patch/",
  "files": [
    {
      "path": "ini/server.dat",
      "url": "ini/server.dat",
      "size": 4096,
      "sha256": "HEX_O_BASE64_SHA256"
    },
    {
      "path": "patch/loader.dat",
      "url": "files/loader.dat"
    }
  ]
}
```

Reglas:

- `path` es obligatorio y define donde se escribira el archivo dentro del cliente
- `url` es opcional
- si `url` no existe, se usa el propio `path`
- `baseUrl` puede ser:
  - una URL remota
  - una ruta local
  - o puede omitirse y entonces el plugin resuelve relativo al manifiesto
- `sha256` acepta hash en hexadecimal o en base64
- `size` es opcional, pero si existe ayuda a detectar cambios mas rapido

## Como resuelve rutas

Destino:

- el destino base es `WorkingDirectory` del cliente
- si `RelativeTargetFolder` tiene valor, se combina con ese `WorkingDirectory`
- el plugin bloquea rutas peligrosas con `..` que intenten salir de la carpeta objetivo

Origen:

- si el manifiesto esta en HTTP/HTTPS, los archivos se descargan con `HttpClient`
- si el manifiesto es local, tambien puede apuntar a archivos locales
- `baseUrl` y `url` se combinan de forma relativa cuando toca

## Orden exacto durante el launch

En `MainLite` y en `Main` el flujo queda asi:

1. preparar entorno DX8/DX9 si aplica
2. regenerar `server.dat` o DLLs previos si hace falta
3. construir `PluginPreLaunchContext`
4. ejecutar `Core.RunPreLaunchPlugins(...)`
5. si todo sale bien, abrir el cliente
6. continuar con hooks/inyeccion/CLServer como antes

Eso significa que el autoparche trabaja sobre la carpeta final real que se va a usar al lanzar el cliente.

## Progreso y logging

El contexto del plugin incluye:

- `ReportProgress`
- `Log`

`AutoPatchPlugin` usa ambos:

- reporta progreso en la franja `1..8` antes del launch real
- escribe trazas con prefijo `[Plugin]` en el log del loader

## Archivos importantes

- `AutoPatchPlugin/AutoPatchPlugin.cs`: logica del plugin
- `AutoPatchPlugin/AutoPatchManifest.cs`: clases del manifiesto
- `AutoPatchPlugin/AutoPatchSettings.cs`: modelo de configuracion
- `AutoPatchPlugin/AutoPatchSettingsStore.cs`: persistencia JSON
- `AutoPatchPlugin/AutoPatchConfigurationWindow.xaml`: UI WPF del plugin
- `ConquerLoader/Core.cs`: ejecucion de plugins de pre-launch
- `ConquerLoader/Forms/WPF/MainLite.xaml.cs`: integracion WPF en el launch
- `ConquerLoader/Forms/Main.cs`: integracion WinForms antigua en el launch

## Build y despliegue

El proyecto:

- esta añadido a `ConquerLoader.sln`
- compila como libreria .NET Framework 4.6.2
- copia automaticamente `AutoPatchPlugin.dll` a:

`ConquerLoader/bin/<Configuration>/Plugins/`

Asi el loader ya lo descubre usando el sistema de plugins existente.

## Limitaciones actuales

- no hay UI dentro de `Settings` para el autoparche; la configuracion va desde la pantalla de plugins
- no hay planificador de versiones ni delta patching; el manifiesto es por archivos
- el plugin no implementa reintentos, mirrors ni rollback automatico

## Recomendacion de uso

Para una primera integracion estable:

1. activa el plugin
2. usa un manifiesto local primero
3. comprueba hashes
4. cuando funcione bien, cambia a CDN o HTTP
5. deja `FailLaunchOnError = true` en produccion
