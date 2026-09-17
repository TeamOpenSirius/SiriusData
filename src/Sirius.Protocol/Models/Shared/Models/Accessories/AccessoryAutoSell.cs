using MessagePack;

namespace Sirius.Protocol.Shared.Models.Accessories;

[MessagePackObject]
public sealed class AccessoryAutoSell : IDataObject
{
    [Key(0)] public long Id { get; set; }
    [Key(1)] public PossessionRarityFlag AutoSellRarity { get; set; }
}
