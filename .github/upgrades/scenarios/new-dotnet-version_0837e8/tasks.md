# BilbiotecaDinamica .NET 10.0 Upgrade Tasks

## Overview

This document tracks the upgrade of `BilbiotecaDinamica` from `net9.0` to `net10.0`. The upgrade is executed as a single atomic code change (project files and package updates), followed by automated testing and a final commit.

**Progress**: 3/4 tasks complete (75%) ![75%](https://progress-bar.xyz/75)

---

## Tasks

### [✓] TASK-001: Verify prerequisites *(Completed: 2026-02-24 23:22)*
**References**: Plan §Project-by-Project Plans, Plan §Migration Strategy

- [✓] (1) Verify .NET 10.0 SDK is installed on the execution environment per Plan §Project-by-Project Plans
- [✓] (2) Runtime/SDK version meets minimum requirements (**Verify**)
- [✓] (3) If present, update or create `global.json` to pin SDK `10.0` per Plan §Project-by-Project Plans
- [✓] (4) `global.json` updated or absent (**Verify**)

### [✓] TASK-002: Atomic framework and package upgrade with compilation fixes *(Completed: 2026-02-24 23:23)*
**References**: Plan §Project-by-Project Plans, Plan §Package Update Reference, Plan §Breaking Changes Catalog

- [✓] (1) Update `<TargetFramework>` to `net10.0` in `BilbiotecaDinamica\BilbiotecaDinamica.csproj` per Plan §Project-by-Project Plans
- [✓] (2) All project files updated to `net10.0` (**Verify**)
- [✓] (3) Update NuGet package references to the exact target versions listed in Plan §Package Update Reference
- [✓] (4) All package references updated to the target versions (**Verify**)
- [✓] (5) Restore dependencies (e.g., `dotnet restore`) per Plan §Package Update Reference
- [✓] (6) Dependencies restored successfully (**Verify**)
- [✓] (7) Build the solution to identify compilation errors and fix them, applying guidance from Plan §Breaking Changes Catalog
- [✓] (8) Rebuild solution to verify fixes applied
- [✓] (9) Solution builds with 0 errors (**Verify**)

### [✓] TASK-003: Run automated tests and validate upgrade *(Completed: 2026-02-24 23:24)*
**References**: Plan §Testing Strategy, Plan §Breaking Changes Catalog

- [✓] (1) Run unit and integration test projects as specified in Plan §Testing Strategy
- [✓] (2) Fix any test failures (reference Plan §Breaking Changes Catalog for common issues)
- [✓] (3) Re-run tests after fixes
- [✓] (4) All tests pass with 0 failures (**Verify**)

### [▶] TASK-004: Final commit
**References**: Plan §Source Control Strategy

- [▶] (1) Commit all remaining changes with message: "TASK-004: Complete upgrade to net10.0"
