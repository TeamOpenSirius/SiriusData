# SiriusData

[English](README.md) / 中文

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
