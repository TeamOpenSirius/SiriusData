# SiriusData

English / [中文](README_ZH.md)

Shared protocol and master data library for the Sirius server, the desktop tooling,
and the standalone calculators.

This repository exists so that the wire protocol, the MasterMemory table models, and
the generated MasterMemory database live in exactly one place. Projects reference this
repository instead of keeping their own copy of the model classes.

## Why the whole protocol lives here

The MasterMemory table models cannot be separated from the rest of the protocol.

Every table model implements `Sirius.Protocol.Shared.IDataObject`, and `IDataObject`
is a MessagePack `[Union]` interface whose member list spans the whole shared data
model: master tables, player state, parties, characters, live results, events, photos,
shops, and leagues. The dependency runs in both directions, so the table models and
the rest of `Sirius.Protocol.Shared` must be compiled into the same assembly.

`Sirius.Protocol` is also a leaf project: it depends only on NuGet packages and on
nothing else in the server repository. Moving it whole keeps every namespace,
type name, and assembly name unchanged, so consuming projects need no source edits.

The retired `Sirius.MasterIndexer` PostgreSQL import/index pipeline was deleted rather
than migrated. `Sirius.MasterData` contains only the typed MasterMemory
edit/export/rebuild layer and reuses the table models in `Sirius.Protocol`.

## Layout

```
SiriusData.sln
src/
  Sirius.MasterData/
    Sirius.MasterData.csproj      net8.0;net10.0 typed MasterMemory editor library
  Sirius.Protocol/
    Sirius.Protocol.csproj        net8.0;net10.0 class library
    Common/                       MessagePack helpers and API result types
    Compatibility/                IDataObject partial fixes
    Models/InternalApi/           internal platform API contracts
    Models/LiveEngine/            live scoring engine models
    Models/Realtime/              MagicOnion realtime hub contracts
    Models/Shared/                shared DTOs, enums, and the IDataObject union
      Models/Master/              MasterMemory table models (229 tables, 320 files)
    MasterMemoryGeneratorOptions.cs
    tools/Wds.MasterMemory.Tool/  offline unpack/pack/localize/verify tool
```

The generated `MemoryDatabase`, `DatabaseBuilder`, `ImmutableBuilder`, and
`Tables.*Table` types are emitted by the Cysharp MasterMemory 3.0.4 source generator
into the `Sirius.Protocol.Shared` namespace during compilation. They are not checked
in. See `src/Sirius.Protocol/README.MasterMemory.md`.

## Consuming this repository

The supported layout keeps the repositories side by side:

```
E:\Ymst\Projects\SiriusData
E:\Ymst\Projects\SiriusServer
E:\Ymst\Projects\SiriusToolbox
```

Consumers should resolve the reference in this order:

1. `ProjectReference` to `..\..\SiriusData\src\Sirius.Protocol\Sirius.Protocol.csproj`
   when the sibling checkout exists.
2. `PackageReference` to the packed `Sirius.Protocol` package produced by this
   repository when the sibling checkout is absent.

The server repository implements this with a conditional item group driven by
`Directory.Build.props`, so a normal developer machine uses the live source and a
build machine without a sibling checkout can fall back to the prebuilt package.

## Build

```powershell
dotnet build .\SiriusData.sln -c Release
```

The libraries target `net8.0` and `net10.0`. The offline verification tool targets `net8.0`.

## Produce the prebuilt package

```powershell
dotnet pack .\src\Sirius.Protocol\Sirius.Protocol.csproj -c Release -o .\artifacts\packages
```

Point consumers at `artifacts\packages` through a `nuget.config` feed entry when they
cannot use a sibling checkout.

## `Sirius.MasterData` library API

`src/Sirius.MasterData` exposes the typed layer the desktop tooling uses, so no caller
needs to reimplement MessagePack parsing or MasterMemory layout handling:

```csharp
var report = MasterMemoryDatabaseService.Verify(databasePath);
var tables = MasterMemoryDatabaseService.GetTables(databasePath);
var rows   = MasterMemoryDatabaseService.ListRecords(databasePath, tableName, offset, limit);
await MasterMemoryDatabaseService.ExportAllJsonAsync(databasePath, jsonDirectory);
var pack = MasterMemoryDatabaseService.PackFromJson(
    sourceDatabase, jsonDirectory, baselineJsonDirectory, outputDatabase, requireExact: true);
```

`PackFromJson` rebuilds a database from an exported JSON directory. A table whose JSON
is byte-identical to the baseline keeps its original payload block; a table whose JSON
still decodes to the original values also keeps its original block. Only tables with a
real value change are re-encoded through the generated `DatabaseBuilder`, and
`requireExact` fails the run when a no-op export would not be byte-identical. Edits are
applied with the same typed converters as `AddRecord`/`UpdateRecord`, so the strict
round trip doubles as a model check.

The `Wds.MasterMemory.Tool` console entry point remains available for scripted offline
work; both surfaces share the same typed converters.

## `Wds.MasterMemory.Tool`

Offline unpack, edit, repack, translate, and verify workflow for `mastermemory.db`.
The runtime server never depends on the exported JSON or the localization CSV.

```powershell
dotnet run --project .\src\Sirius.Protocol\tools\Wds.MasterMemory.Tool -- --help
```

See `src/Sirius.Protocol/tools/Wds.MasterMemory.Tool/README.md`.
