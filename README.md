# SiriusData

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

Note that `Sirius.MasterIndexer` in the server repository is a separate, checked-in
generated pipeline. It does not reference `Sirius.Protocol` and is unaffected by this
repository.

## Layout

```
SiriusData.sln
src/
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

The library targets `net8.0` and `net10.0`. The offline tool targets `net8.0`.

## Produce the prebuilt package

```powershell
dotnet pack .\src\Sirius.Protocol\Sirius.Protocol.csproj -c Release -o .\artifacts\packages
```

Point consumers at `artifacts\packages` through a `nuget.config` feed entry when they
cannot use a sibling checkout.

## `Wds.MasterMemory.Tool`

Offline unpack, edit, repack, translate, and verify workflow for `mastermemory.db`.
The runtime server never depends on the exported JSON or the localization CSV.

```powershell
dotnet run --project .\src\Sirius.Protocol\tools\Wds.MasterMemory.Tool -- --help
```

See `src/Sirius.Protocol/tools/Wds.MasterMemory.Tool/README.md`.

---

# SiriusData（中文）

Sirius 服务端、桌面工具链和独立计算器共用的协议与主数据库。

本仓库存在的意义，是让通信协议、MasterMemory 表模型和生成的 MasterMemory 数据
库只有一份定义。其他项目引用本仓库，而不是各自维护一份模型代码。

## 为什么整个协议层都放在这里

MasterMemory 的表模型无法从协议其余部分单独拆出。

每个表模型都实现 `Sirius.Protocol.Shared.IDataObject`，而 `IDataObject` 是一个
MessagePack `[Union]` 接口，其成员列表覆盖整套共享数据模型：主数据表、玩家状态、
队伍、角色、Live 结算、活动、相册、商店和联赛。依赖是双向的，因此表模型与
`Sirius.Protocol.Shared` 其余部分必须编译进同一个程序集。

同时 `Sirius.Protocol` 本身就是叶子项目：它只依赖 NuGet 包，不依赖服务端仓库中
的任何其他项目。整体搬迁可以保持所有命名空间、类型名和程序集名不变，消费方无需
改动任何源码。

服务端仓库中的 `Sirius.MasterIndexer` 是另一条已签入的生成式流水线，它不引用
`Sirius.Protocol`，不受本次拆分影响。

## 目录结构

```
SiriusData.sln
src/
  Sirius.Protocol/
    Sirius.Protocol.csproj        net8.0;net10.0 类库
    Common/                       MessagePack 辅助与 API 结果类型
    Compatibility/                IDataObject 局部修补
    Models/InternalApi/           内部平台 API 契约
    Models/LiveEngine/            Live 计分引擎模型
    Models/Realtime/              MagicOnion 实时 Hub 契约
    Models/Shared/                共享 DTO、枚举与 IDataObject 联合
      Models/Master/              MasterMemory 表模型（229 表，320 个文件）
    MasterMemoryGeneratorOptions.cs
    tools/Wds.MasterMemory.Tool/  离线解包/打包/翻译/校验工具
```

`MemoryDatabase`、`DatabaseBuilder`、`ImmutableBuilder` 和 `Tables.*Table` 由
Cysharp MasterMemory 3.0.4 源生成器在编译期生成到 `Sirius.Protocol.Shared`
命名空间，不签入仓库。详见 `src/Sirius.Protocol/README.MasterMemory.md`。

## 如何被引用

约定的目录布局是把各仓库放在同一层级：

```
E:\Ymst\Projects\SiriusData
E:\Ymst\Projects\SiriusServer
E:\Ymst\Projects\SiriusToolbox
```

消费方按以下顺序解析引用：

1. 同级检出存在时，使用指向
   `..\..\SiriusData\src\Sirius.Protocol\Sirius.Protocol.csproj` 的 `ProjectReference`。
2. 同级检出不存在时，使用本仓库打包出的 `Sirius.Protocol` 预编译库
   （`PackageReference`）。

服务端仓库通过 `Directory.Build.props` 控制的条件引用组实现了这一优先级：日常开发
直接编译最新源码，缺少同级检出时退回预编译库。

## 构建

```powershell
dotnet build .\SiriusData.sln -c Release
```

类库目标框架为 `net8.0` 与 `net10.0`，离线工具目标框架为 `net8.0`。

## 生成预编译库

```powershell
dotnet pack .\src\Sirius.Protocol\Sirius.Protocol.csproj -c Release -o .\artifacts\packages
```

无法使用同级检出时，在消费方 `nuget.config` 中把 `artifacts\packages` 添加为源即可。

## `Wds.MasterMemory.Tool`

面向 `mastermemory.db` 的离线解包、编辑、重打包、翻译与校验工具。运行时服务端不依赖
导出的 JSON 或翻译 CSV。

```powershell
dotnet run --project .\src\Sirius.Protocol\tools\Wds.MasterMemory.Tool -- --help
```

详见 `src/Sirius.Protocol/tools/Wds.MasterMemory.Tool/README.md`。
