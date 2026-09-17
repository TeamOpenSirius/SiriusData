# Sirius.Protocol + MasterMemory v3

This tree replaces the previous compatibility/fake MasterMemory layer with Cysharp MasterMemory 3.0.4's real source-generator pipeline.

## Runtime usage

`mastermemory.db` is not deserialized as a normal MessagePack object. Load it through the generated database constructor:

```csharp
using Sirius.Protocol.Shared;

byte[] bytes = await File.ReadAllBytesAsync("mastermemory.db");
var db = new MemoryDatabase(
    bytes,
    maxDegreeOfParallelism: Environment.ProcessorCount);

var music = db.MusicMasterTable.FindById(1);
```

The following are generated at compile time from the annotated models:

- `Sirius.Protocol.Shared.MemoryDatabase`
- `Sirius.Protocol.Shared.DatabaseBuilder`
- `Sirius.Protocol.Shared.ImmutableBuilder`
- `Sirius.Protocol.Shared.Tables.*Table`
- MasterMemory MessagePack resolver support

Do not restore the deleted hand-written `MemoryDatabase.cs`, `DatabaseBuilder.cs`, `MasterMemoryResolver*.cs`, or `Models/Shared/Tables/*.cs`; those were compatibility shells and conflict with the official generator.

## Model coverage for the supplied DB

The supplied `mastermemory.db` contains 229 tables. The patched protocol contains exactly 229 matching `[MemoryTable("...")]` model definitions and a primary key definition for every table.

Two tables use composite primary keys recovered from the previous table shells:

- `CharacterBloomItemMaster`: `(Rarity, CurrentStage)`
- `StarRankRewardMaster`: `(Rank, CharacterBaseMasterId)`

No secondary indexes were invented because the supplied source did not contain reliable secondary-index metadata.

The protocol snapshot was slightly older than the DB. Observable newer MessagePack fields and the nine DB tables missing from the old table shells were added. Fields whose semantics cannot be proven from the supplied artifacts use conservative names such as `Reserved25` instead of fabricated domain names.

`EventCampClassMaster` is empty in this DB sample. Only its `Id`/primary-key shape can currently be established; a newer DB containing rows may require extending that class.

## Offline DB/localization tool

See `tools/Wds.MasterMemory.Tool/README.md`.

The tool uses JSON/CSV only as an offline editable representation. Runtime/server loading does not depend on `table.json`, exported JSON, or the localization CSV.
