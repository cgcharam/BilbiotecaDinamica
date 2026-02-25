# BilbiotecaDinamica

Este proyecto es una aplicación ASP.NET Core MVC diseñada para gestionar una biblioteca dinámica. Incluye autenticación y autorización de usuarios utilizando ASP.NET Core Identity, y funcionalidades para buscar y gestionar libros.

## Características

-   **Autenticación de Usuarios:** Registro seguro de usuarios, inicio de sesión y gestión de identidad.
-   **Búsqueda de Libros:** Funcionalidad para buscar libros, con posible integración con APIs externas como Open Library.
-   **Gestión Personal de Libros:** Los usuarios pueden gestionar su propia colección de libros, incluyendo la adición de favoritos.
-   **Arquitectura MVC:** Sigue el patrón Modelo-Vista-Controlador para una clara separación de responsabilidades.

## Tecnologías Utilizadas

-   **ASP.NET Core 9.0:** El framework web para construir la aplicación.
-   **Entity Framework Core:** ORM para la interacción con la base de datos.
-   **ASP.NET Core Identity:** Para la autenticación y autorización de usuarios.
-   **SQL Server:** Base de datos para almacenar los datos de la aplicación.
-   **HTML, CSS, JavaScript:** Para el desarrollo front-end.
-   **Bootstrap:** Para diseño responsivo y estilos.

## Primeros Pasos

### Prerrequisitos

-   .NET 9.0 SDK
-   SQL Server (o SQL Server Express LocalDB, que a menudo se incluye con Visual Studio)
-   Visual Studio 2022 (recomendado) o Visual Studio Code

### Instrucciones de Configuración

1.  **Clonar el repositorio:**
    ```bash
    git clone https://github.com/tu-usuario/BilbiotecaDinamica.git
    cd BilbiotecaDinamica
    ```
    *(Nota: Reemplaza `https://github.com/tu-usuario/BilbiotecaDinamica.git` con la URL real del repositorio si está alojado.)*

2.  **Restaurar paquetes NuGet:**
    ```bash
    dotnet restore
    ```

3.  **Actualizar Migraciones de Base de Datos:**
    Este proyecto utiliza Migraciones de Entity Framework Core. Necesitas aplicar las migraciones para crear y actualizar el esquema de tu base de datos.

    Abre una terminal en el directorio `BilbiotecaDinamica` (el que contiene `BilbiotecaDinamica.csproj`) y ejecuta:
    ```bash
    dotnet ef database update
    ```
    Este comando creará la base de datos y aplicará todas las migraciones pendientes.

4.  **Configurar Cadena de Conexión:**
    Asegúrate de que tu cadena de conexión a la base de datos en `appsettings.json` (y `appsettings.Development.json`) esté configurada correctamente para tu instancia de SQL Server.

    Ejemplo `appsettings.json` (para LocalDB):
    ```json
    "ConnectionStrings": {
      "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=BilbiotecaDinamicaDb;Trusted_Connection=True;MultipleActiveResultSets=true"
    }
    ```

### Ejecutar la Aplicación

Puedes ejecutar la aplicación usando Visual Studio o la CLI de .NET.

#### Usando Visual Studio

1.  Abre el archivo de solución `BilbiotecaDinamica.slnx` en Visual Studio.
2.  Presiona `F5` o haz clic en el botón "Ejecutar" para iniciar la aplicación.

#### Usando la CLI de .NET

Abre una terminal en el directorio `BilbiotecaDinamica` (el que contiene `BilbiotecaDinamica.csproj`) y ejecuta:
```bash
dotnet run
```
La aplicación normalmente se ejecutará en `https://localhost:7000` (o un puerto similar). Revisa la salida de la consola para la URL exacta.

## Estructura del Proyecto

-   `BilbiotecaDinamica.slnx`: Archivo de solución de Visual Studio.
-   `BilbiotecaDinamica/`: El proyecto principal de ASP.NET Core.
    -   `Areas/Identity/`: Contiene archivos relacionados con ASP.NET Core Identity para la gestión de usuarios.
    -   `Controllers/`: Maneja las solicitudes y respuestas entrantes.
    -   `Models/`: Define las estructuras de datos y la lógica de negocio.
    -   `Migrations/`: Archivos de migración de base de datos generados por Entity Framework Core.
    -   `Views/`: Contiene las plantillas de la interfaz de usuario (vistas Razor).
    -   `wwwroot/`: Archivos estáticos como CSS, JavaScript e imágenes.
    -   `appsettings.json`: Configuración de la aplicación.
    -   `Program.cs`: Configuración de inicio de la aplicación.

## BilbiotecaDinamica — Cambios recientes

Resumen breve

- Se ha habilitado tolerancia a fallos transitorios en la conexión a la base de datos añadiendo `EnableRetryOnFailure()` al registro del `DbContext`.
- Se añadió ejecución automática de migraciones al arranque de la aplicación (`context.Database.Migrate()`) para crear la base de datos y aplicar migraciones pendientes en entornos de desarrollo.
- Se añadió la directiva necesaria `using Microsoft.Extensions.DependencyInjection;` en `Program.cs`.

Motivación

Al crear usuarios con ASP.NET Core Identity la aplicación fallaba con:

```
Cannot open database "BilbiotecaDinamica" requested by the login. The login failed.
```

Causa raíz

- La base de datos `BilbiotecaDinamica` no existía o no tenía las tablas de Identity aplicadas. El intento de crear un usuario falla al usar EF Core porque no puede abrir la base de datos.
- En algunos escenarios la conexión puede sufrir errores transitorios; `EnableRetryOnFailure()` mitiga esos casos.

Cambios aplicados (ubicación)

- `Program.cs`:
  - `builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString, sqlOptions => sqlOptions.EnableRetryOnFailure()));`
  - Bloque de arranque que crea un scope y ejecuta `context.Database.Migrate()`.
  - `using Microsoft.Extensions.DependencyInjection;`

Advertencias y buenas prácticas

- No es recomendable ejecutar `Database.Migrate()` automáticamente en entornos de producción sin control: prefiera aplicar migraciones mediante su pipeline CI/CD o un paso manual controlado.
- Si prefiere no aplicar migraciones en runtime, elimine o comente el bloque que llama a `context.Database.Migrate()` y use:

```bash
# Crear migración (si no existe aún)
dotnet ef migrations add InitialIdentitySetup

# Aplicar migraciones a la BD
dotnet ef database update
```

- Asegúrese de que la cadena de conexión en `appsettings.json` sea la adecuada y que el usuario/servicio que ejecuta la aplicación tenga permisos sobre la base de datos. Para LocalDB con autenticación integrada la cadena por defecto es:

```json
"ConnectionStrings": {
  "ApplicationDbContextConnection": "Server=(localdb)\\\\mssqllocaldb;Database=BilbiotecaDinamica;Trusted_Connection=True;MultipleActiveResultSets=true"
}
```

Qué hacer si el error persiste

1. Verifique que el servidor SQL (LocalDB) esté en ejecución.
2. En SQL Server Management Studio (o `sqllocaldb`), confirme que la base de datos `BilbiotecaDinamica` exista.
3. Ejecute `dotnet ef database update` manualmente para crear la BD y tablas.
4. Revise los permisos del usuario que ejecuta la app.

Contacto

Este README documenta las correcciones aplicadas en el repositorio. Si desea, puedo:

- Revertir la llamada automática a `Database.Migrate()` y documentar un flujo seguro para producción.
- Añadir scripts de migración inicial y ejemplos de CI/CD para desplegar migraciones.

## Contribución

(Opcional: Añade guías para contribuciones si este es un proyecto de código abierto.)

## Licencia

(Opcional: Especifica la licencia bajo la cual se distribuye este proyecto.)