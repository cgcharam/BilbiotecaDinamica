# Plan de actualización a .NET 10.0

Tabla de contenidos

- Executive Summary
- Migration Strategy
- Detailed Dependency Analysis
- Project-by-Project Plans
- Package Update Reference
- Breaking Changes Catalog
- Testing Strategy
- Risk Management
- Complexity & Effort Assessment
- Source Control Strategy
- Success Criteria

---

## Executive Summary

Resumen: Actualizar la solución `BilbiotecaDinamica` de `net9.0` a `net10.0` usando la estrategia All-At-Once (actualización atómica de todos los proyectos simultáneamente).

High-level metrics (extraídos de `assessment.md`):

- Total Projects: 1 (todas requieren actualización)
- Total NuGet Packages: 5 (todas requieren actualización)
- Total Code Files: 43
- Files with incidents: 3
- Total LOC: 3408
- Total Issues detectadas: 16

Rationale: Solución pequeña (1 proyecto), baja complejidad y cobertura de paquetes conocida → All-At-Once es apropiada.

## Migration Strategy

Selected Strategy: **All-At-Once Strategy** — Actualizar todos los proyectos simultáneamente en una sola operación atómica.

Justificación:
- Solución con 1 proyecto
- Paquetes NuGet con versiones objetivo conocidas (10.0.x)
- Riesgo manejable dada la pequeña superficie de código

Operaciones clave (como una sola operación atómica):
- Actualizar TargetFramework de todos los proyectos a `net10.0`
- Actualizar referencias de paquetes a las versiones sugeridas
- Restaurar dependencias y compilar solución para identificar errores
- Resolver errores de compilación resultantes
- Ejecutar suite de pruebas (si aplica)

Nota: Este documento prescribe las acciones y criterios; no ejecuta cambios.

## Detailed Dependency Analysis

Resumen de dependencias:

- Proyectos analizados: `BilbiotecaDinamica\BilbiotecaDinamica.csproj` (SDK-style, AspNetCore)
- Dependencias entre proyectos: Ninguna (solución de un solo proyecto)
- Dependientes: Ninguno

Conclusión: No hay ordenamiento complejo por dependencias; la actualización atómica cubre el único proyecto.

## Project-by-Project Plans

### Proyecto: `BilbiotecaDinamica\BilbiotecaDinamica.csproj`

Current State:
- TargetFramework: `net9.0`
- SDK-style: True
- Project type: AspNetCore (Razor Pages + Identity + EF Core)
- Files: 49
- LOC: 3408
- Incidents: 3

Proposed Target:
- TargetFramework: `net10.0`

Migration Steps (especificación, para el ejecutor):
1. Verificar prerrequisitos: instalar SDK .NET 10.0 en el entorno y actualizar `global.json` si aplica.
2. Actualizar `<TargetFramework>` a `net10.0` en el archivo `.csproj`.
3. Actualizar las referencias de paquetes a las versiones objetivo indicadas en la sección "Package Update Reference".
4. Restaurar paquetes y compilar la solución para identificar errores de compilación y breaking changes.
5. Aplicar cambios de código necesarios (según catálogo de breaking changes).
6. Ejecutar pruebas unitarias y de integración disponibles.
7. Verificar comportamiento de localización y autenticación (Identity) en entorno de staging.

Validación (criterios):
- La solución compila sin errores
- Todas las pruebas automatizadas pasan
- No quedan vulnerabilidades de paquetes identificadas en assessment

## Package Update Reference

Packages identificadas (de `assessment.md`):

- `Microsoft.AspNetCore.Identity.EntityFrameworkCore` 9.0.8 → 10.0.3
- `Microsoft.AspNetCore.Identity.UI` 9.0.8 → 10.0.3
- `Microsoft.EntityFrameworkCore.SqlServer` 9.0.8 → 10.0.3
- `Microsoft.EntityFrameworkCore.Tools` 9.0.8 → 10.0.3
- `Microsoft.VisualStudio.Web.CodeGeneration.Design` 9.0.0 → 10.0.2

Para cada paquete: el ejecutor debe aplicar la versión exacta sugerida por el assessment.

## Breaking Changes Catalog

Resumen de categorías detectadas en assessment:

- Source incompatible (4 incidencias): APIs de Identity DI y AddEntityFrameworkStores/AddDefaultIdentity pueden requerir adaptación en llamadas o firmas.
- Behavioral changes (6 incidencias): `HttpContent`, `Uri`, `UseExceptionHandler` y `AddHttpClient` pueden exhibir cambios de comportamiento; testear rutas afectadas.

Acción propuesta: registrar errores de compilación y mapearlos a entradas de este catálogo para guiar correcciones de código.

## Testing Strategy

Niveles de pruebas:
- Project-level: compilar y ejecutar pruebas unitarias (si existen)
- Integration: pruebas que involucren EF Core/SQL Server y Identity
- Full-solution: verificación de páginas Razor y flujos de autenticación

Checklist de verificación automatizable:
- [ ] Compila sin errores
- [ ] No quedan advertencias críticas relacionadas con APIs obsoletas
- [ ] Pruebas unitarias pasan
- [ ] Pruebas de integración críticas pasan

## Risk Management

Principales riesgos:
- Cambios en APIs de Identity y EF Core → pueden requerir ajustes en Startup/Program o en llamadas a IdentityBuilder
- Cambios de comportamiento en `HttpContent`/`Uri` → pruebas de internacionalización/localización

Mitigaciones:
- Ejecutar pruebas de integración que cubran autenticación y acceso a BD
- Revisar y aplicar actualizaciones sugeridas en assessment.md para paquetes con breaking changes

## Complexity & Effort Assessment

Clasificación general: **Simple**

Per-project rating:
- `BilbiotecaDinamica` — Complejidad: Low (Superficie de cambio pequeña, 10+ LOC estimadas)

## Source Control Strategy

- Source branch: `master` (actual)
- Upgrade branch: `upgrade-to-NET10` (recomendado)
- Acciones pendientes: Commit de cambios actuales antes de crear la rama de upgrade (acción: `commit`)
- Recomendación: Un único commit atómico que contenga todos los cambios de TargetFramework y las actualizaciones de paquetes. PR con checklist de validación y revisión de breaking changes.

## Success Criteria

La migración se considera completa cuando se cumplen los siguientes criterios:
- Todos los proyectos apuntan a `net10.0`.
- Todas las actualizaciones de paquetes indicadas en assessment.md se aplicaron con las versiones exactas.
- La solución compila sin errores.
- Todas las pruebas automatizadas pasan.
- No quedan vulnerabilidades de paquetes listadas en assessment.md.

---

[To be filled: detalles adicionales, catálogo de cambios por archivo y referencias a líneas de código específicas]
