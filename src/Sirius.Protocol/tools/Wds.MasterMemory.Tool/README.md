# WDS MasterMemory Tool (.NET)

Uses the **official Cysharp MasterMemory 3.0.4 source generator and DatabaseBuilder** through the patched `Sirius.Protocol` models.

## Build

```powershell
dotnet build .\tools\Wds.MasterMemory.Tool\Wds.MasterMemory.Tool.csproj -c Release
```

## Unpack / edit / repack

```powershell
dotnet run --project .\tools\Wds.MasterMemory.Tool -- unpack .\mastermemory.db .\dump
# edit dump\tables\*.json
dotnet run --project .\tools\Wds.MasterMemory.Tool -- pack .\dump .\mastermemory_zh.db
dotnet run --project .\tools\Wds.MasterMemory.Tool -- verify .\mastermemory_zh.db
```

`unpack` stores an exact copy of the input in `dump/.wdsmm/original.db` and hashes every exported table JSON.
If `pack` sees that **no JSON file changed**, it copies that original DB directly and verifies SHA-256. This makes a no-edit unpack/pack **byte-for-byte identical**, not merely semantically equivalent.

If tables changed, the tool deserializes the JSON back into the generated typed models and rebuilds the database using MasterMemory's generated `DatabaseBuilder` (LZ4Block, normal MasterMemory header/table layout).

## Exact round-trip test

```powershell
dotnet run --project .\tools\Wds.MasterMemory.Tool -- roundtrip .\mastermemory.db .\rt
```

Expected output contains:

```text
BYTE-EXACT ROUNDTRIP: PASS
```

## Localization CSV

Only Japanese/CJK strings are extracted by default:

```powershell
dotnet run --project .\tools\Wds.MasterMemory.Tool -- extract-text .\dump .\translation.csv
dotnet run --project .\tools\Wds.MasterMemory.Tool -- apply-text .\dump .\translation.csv
dotnet run --project .\tools\Wds.MasterMemory.Tool -- pack .\dump .\mastermemory_zh.db
```

Use `--all` on `extract-text` if every string (including paths/IDs) is desired.

## Model notes

- The old compatibility `MasterMemory` attributes and hand-written `MemoryDatabase` / `DatabaseBuilder` / `*Table` shells were removed. They conflict with the v3 source generator.
- Current DB table set is 229 tables. All current table model classes are annotated with `[MemoryTable]` and a primary key.
- Existing stub `PrimaryKeySelector` signatures were used to recover the composite keys (`CharacterBloomItemMaster`, `StarRankRewardMaster`). No secondary indexes were invented because the supplied code contains no evidence for them.
- The current DB contains schema fields newer than the supplied protocol snapshot. These were added where observable from the binary. Unknown semantics are intentionally named `ReservedXX` instead of assigning a false meaning.
- `EventCampClassMaster` is empty in the supplied DB, so only its primary key can be established from this sample. A later DB with actual rows may require extending that model.
