# Repository Agent Instructions

## What this repository is

`SiriusData` is the single source of truth for the Sirius wire protocol, the
MasterMemory table models, and the generated MasterMemory database. It was extracted
from `E:\Ymst\Projects\SiriusServer` so that the desktop tooling and the standalone
calculators stop keeping private copies of the model classes.

## Layout rules

- All libraries live under `src/`.
- The library project name, assembly name, and namespaces stay as `Sirius.Protocol`
  and `Sirius.Protocol.*`. Do not rename them: consuming repositories reference these
  names directly and a rename would force changes there.
- Repositories that consume this library must sit next to it under `E:\Ymst\Projects`:

  ```
  E:\Ymst\Projects\SiriusData
  E:\Ymst\Projects\SiriusServer
  E:\Ymst\Projects\SiriusToolbox
  ```

  This is a hard requirement. When a sibling checkout is not present, consumers must
  fall back to the prebuilt `Sirius.Protocol` package packed from this repository
  (`artifacts/packages`), never to a copied source tree.

## Model and code generation rules

- `src/Sirius.Protocol/Models/Shared/Models/Master/` holds the MasterMemory table
  models. They are generated from the client protocol dump and are checked in.
- The table models implement `Sirius.Protocol.Shared.IDataObject`, which is a
  MessagePack `[Union]` covering the whole shared data model. Do not try to move the
  master tables into a separate assembly; the dependency is bidirectional.
- `MemoryDatabase`, `DatabaseBuilder`, `ImmutableBuilder`, and `Tables.*Table` are
  produced by the MasterMemory source generator at compile time. Never check in
  hand-written replacements for those types; they conflict with the generator.
- Changing a table model changes the generated database layout. Re-run the
  round-trip verification in `src/Sirius.Protocol/tools/Wds.MasterMemory.Tool`
  before trusting the result.

## Coordination with SiriusServer

- The server repository no longer contains `Sirius.Protocol`. Its `Directory.Build.props`
  resolves the sibling checkout first and the prebuilt package second.
- The retired `Sirius.MasterIndexer` PostgreSQL pipeline must not be recreated.
  `src/Sirius.MasterData` may contain only MasterMemory editing/export/rebuild code and
  must reuse the table models in `Sirius.Protocol`.
- Treat `E:\Ymst\Projects\SiriusServer\episode\Adv-Resource` as off limits unless the
  user explicitly asks otherwise.

## Working style

- Communicate with the user in concise Chinese unless they request another language.
- Keep repository documentation English-first with a Chinese counterpart.
- Use `git` directly; do not push or perform remote branch operations.
- Group related changes into one coherent commit and stage only files that belong to
  the current task.
- After modifying source files or project metadata, update CodeGraph with
  `codegraph sync` when a `.codegraph/` directory exists.

## Commit attribution

- Commits made while assisting the maintainer must use the maintainer's Git identity
  as the author and add a `Co-authored-by` trailer naming each agent that materially
  contributed. For Codex use exactly:

  `Co-authored-by: Codex <codex@openai.com>`
